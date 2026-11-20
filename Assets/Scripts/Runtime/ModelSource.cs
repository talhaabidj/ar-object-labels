using System.Collections;
using System.Collections.Generic;
using ArObjectLabels.Core;
using Unity.Collections;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.Rendering;
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
        AsyncGPUReadbackRequest readback;
        bool waitingForReadback;
        int resultLength;
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


        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
            ModelSettings rules = settings.model;
            classNames = ClassNames.ParseClassList(classListFile.text);
            // A device with no compute shaders cannot run the model on the graphics card; the CPU is slower but works.
            BackendType backend = SystemInfo.supportsComputeShaders ? BackendType.GPUCompute : BackendType.CPU;
            worker = new Worker(ModelLoader.Load(modelFile), backend);
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
            // Back from another source, the last result is out of date.
            if (ModelPacing.WasLeftAlone(lastAskedFrame, Time.frameCount))
            {
                ForgetEarlierResult();
            }
            lastAskedFrame = Time.frameCount;

            if (waitingForReadback)
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

        // Forgets the last result, so nothing from before shows. A run still under way is carried on to its end and
        // its result thrown away, because leaving a run halfway breaks every later run of the model.
        void ForgetEarlierResult()
        {
            resultIsOld = waitingForReadback || layers != null;
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
            // The camera picture's own lens, where known, can differ from the screen camera's on a phone.
            if (cameraManager.TryGetIntrinsics(out XRCameraIntrinsics lens))
            {
                runFrame = runFrame.WithLens(lens.focalLength, lens.principalPoint, lens.resolution);
            }
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
                    AskForResult();
                    return;
                }
            }
        }

        // The readback is asked for here, not through the tensor, because only the request itself tells a finished
        // result from one Unity has already disposed of (see ModelPacing.NextStep).
        void AskForResult()
        {
            var output = worker.PeekOutput() as Tensor<float>;
            resultLength = output.shape.length;
            if (output.dataOnBackend is ComputeTensorData onGraphicsCard)
            {
                readback = AsyncGPUReadback.Request(onGraphicsCard.buffer, resultLength * sizeof(float), 0);
                waitingForReadback = true;
                return;
            }

            // On the CPU the result can be read at once; the array is freed by the backend at the end of the frame.
            NativeArray<float> result = output.DownloadToNativeArray();
            if (ModelPacing.NextStep(true, false, resultIsOld) == ReadbackStep.Use)
            {
                MakeBoxes(result);
            }
            resultIsOld = false;
        }

        void PickUpResultIfReady()
        {
            ReadbackStep step = ModelPacing.NextStep(readback.done, readback.hasError, resultIsOld);
            if (step == ReadbackStep.Wait)
            {
                return;
            }
            waitingForReadback = false;
            resultIsOld = false;
            if (step == ReadbackStep.Use)
            {
                // The data belongs to the readback, so it is not disposed here.
                MakeBoxes(readback.GetData<float>());
            }
        }

        void MakeBoxes(NativeArray<float> result)
        {
            ModelSettings rules = settings.model;
            // The result is copied into one kept array, so a run makes no new 2.8 MB array.
            int length = resultLength;
            table = ModelOutput.TableFor(table, length);
            NativeArray<float>.Copy(result, table, length);

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
        }
    }
}
