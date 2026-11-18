using System.Collections;
using System.Collections.Generic;
using ArObjectLabels.Core;
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
        IEnumerator layers;
        float lastRunStarted = float.NegativeInfinity;
        ModelPicture square;
        Texture2D cameraPicture;
        CameraFrame runFrame;
        CameraFrame foundFrame;
        bool hasResult;

        public string Name => Messages.ModelSource;

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
            worker = new Worker(ModelLoader.Load(modelFile), BackendType.GPUCompute);
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
            int share = settings.model.layersPerFrame > 0 ? settings.model.layersPerFrame : int.MaxValue;
            for (int i = 0; i < share; i++)
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

            ModelSettings rules = settings.model;
            float[] table;
            using (Tensor<float> result = awaited.ReadbackAndClone())
            {
                table = result.DownloadToArray();
            }
            awaited = null;

            found.Clear();
            ModelOutput.AddBoxes(table, table.Length / ModelOutput.Rows(classNames.Count), classNames, rules.minScore, found);
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
