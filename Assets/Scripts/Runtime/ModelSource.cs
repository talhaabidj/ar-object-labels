using System.Collections;
using System.Collections.Generic;
using ArObjectLabels.Core;
using Unity.Collections;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Finds objects in the camera's picture with the detection model. One run takes longer than a frame, so
    /// a run is started, spread over frames a few layers at a time, and its result is picked up in a later
    /// frame, with the camera's pose from the moment the picture was taken. It works only while it is the
    /// chosen source.
    /// </summary>
    public class ModelSource : MonoBehaviour, IDetectionSource
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] ARCameraManager cameraManager;
        [SerializeField] AppSettings settings;
        [SerializeField] ModelAsset modelFile;
        [SerializeField] TextAsset classListFile;

        readonly List<Detection> found = new List<Detection>();
        List<string> classNames;
        Worker worker;
        Tensor<float> input;
        Tensor<float> awaited;
        float[] table;
        IEnumerator layers;
        float lastRunStarted = float.NegativeInfinity;
        ModelPicture square;
        Texture2D cameraPicture;
        CameraFrame runFrame;
        CameraFrame foundFrame;
        bool hasResult;
        bool resultIsOld;
        int lastAskedFrame = ModelPacing.NeverAsked;

        public string Name => Messages.ModelSource;

        public bool NeedsConfirming => true;

        /// <summary>Where the model runs: the graphics card, or the CPU on a device without compute shaders.</summary>
        public BackendType Backend { get; private set; }

        /// <summary>How many runs have finished since the app started. For measurements.</summary>
        public int RunsFinished { get; private set; }

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
            ModelSettings rules = settings.model;
            classNames = ClassNames.ParseClassList(classListFile.text);
            // A device with no compute shaders cannot run the model on the graphics card; the CPU is slower but works.
            Backend = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.CPU;
            worker = new Worker(ModelLoader.Load(modelFile), Backend);
            input = new Tensor<float>(new TensorShape(1, 3, rules.inputSize, rules.inputSize));
            square = new ModelPicture(rules.inputSize);
        }

        void OnDestroy()
        {
            worker?.Dispose();
            input?.Dispose();
            square?.Dispose();
            if (cameraPicture != null)
            {
                Destroy(cameraPicture);
            }
        }

        /// <summary>Gives the result of the last finished run. False until the first run has finished.</summary>
        public bool TryRead(List<Detection> into, out CameraFrame frame)
        {
            // Seen in Play on 20 Nov: back from another source, reading a result that had arrived meanwhile threw,
            // because the graphics card's readback is gone after the frame it arrives in.
            if (ModelPacing.WasLeftAlone(lastAskedFrame, Time.frameCount))
            {
                ForgetEarlierResult();
            }
            lastAskedFrame = Time.frameCount;

            if (awaited != null)
            {
                PickUpResultIfReady();
            }
            else if (layers != null)
            {
                StartMoreLayers();
            }
            else if (ModelPacing.RunIsDue(Time.unscaledTime, lastRunStarted, settings.model.runsPerSecond) && TryTakePicture())
            {
                StartRun();
            }

            into.Clear();
            into.AddRange(found);
            frame = foundFrame;
            return hasResult;
        }

        // Forgets the last result, so nothing from before shows. A finished run's readback may be gone, so it is not
        // read. A run still being started is carried on to its end and its result thrown away: seen in Play on
        // 20 Nov, a run left halfway broke every later run of the model.
        void ForgetEarlierResult()
        {
            awaited = null;
            resultIsOld = layers != null;
            found.Clear();
            hasResult = false;
        }

        bool TryTakePicture()
        {
            if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                return false;
            }

            using (image)
            {
                if (cameraPicture == null || cameraPicture.width != image.width || cameraPicture.height != image.height)
                {
                    if (cameraPicture != null)
                    {
                        Destroy(cameraPicture);
                    }
                    cameraPicture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
                }
                // In the Editor's simulation the picture arrives the right way round for a texture. Seen in
                // Play: with a mirror setting here, every box came out on the wrong side. Not checked on a phone.
                var conversion = new XRCpuImage.ConversionParams(image, TextureFormat.RGBA32, XRCpuImage.Transformation.None);
                image.Convert(conversion, cameraPicture.GetRawTextureData<byte>());
                cameraPicture.Apply(false);
            }
            runFrame = CameraFrames.Of(viewCamera, cameraPicture.width, cameraPicture.height);
            return true;
        }

        void StartRun()
        {
            var layout = new TextureTransform().SetCoordOrigin(CoordOrigin.TopLeft);
            TextureConverter.ToTensor(square.Draw(cameraPicture), input, layout);
            layers = worker.ScheduleIterable(input);
            lastRunStarted = Time.unscaledTime;
            StartMoreLayers();
        }

        // Each step starts one layer of the model. When none is left, the result is asked for.
        void StartMoreLayers()
        {
            int layersThisFrame = settings.model.layersPerFrame > 0 ? settings.model.layersPerFrame : int.MaxValue;
            for (int i = 0; i < layersThisFrame; i++)
            {
                if (!layers.MoveNext())
                {
                    layers = null;
                    awaited = worker.PeekOutput() as Tensor<float>;
                    awaited.ReadbackRequest();
                    return;
                }
            }
        }

        void PickUpResultIfReady()
        {
            if (!awaited.IsReadbackRequestDone())
            {
                return;
            }
            if (resultIsOld)
            {
                awaited = null;
                resultIsOld = false;
                return;
            }

            ModelSettings rules = settings.model;
            // Seen in Play on 20 Nov: a new array for each result made about 28 MB of garbage a second. The result
            // is copied into one kept array instead. The native array is not disposed: on the graphics card it
            // belongs to the readback, and on the CPU it is freed at the end of the frame.
            NativeArray<float> result = awaited.DownloadToNativeArray();
            int length = awaited.shape.length;
            table = ModelOutput.TableFor(table, length);
            NativeArray<float>.Copy(result, table, length);
            awaited = null;

            found.Clear();
            // Boxes are kept down to the score for staying; which of them get a label is decided over several runs.
            ModelOutput.AddBoxes(table, length / ModelOutput.Rows(classNames.Count), classNames, rules.stayScore, found);
            BoxOverlap.KeepTheBest(found, rules.overlapLimit);
            for (int i = 0; i < found.Count; i++)
            {
                found[i] = ImageMapping.ModelToPicture(found[i], runFrame.PictureWidth, runFrame.PictureHeight, rules.inputSize);
            }
            foundFrame = runFrame;
            hasResult = true;
            RunsFinished++;
        }
    }
}
