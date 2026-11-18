using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ArObjectLabels
{
    /// <summary>
    /// A stand-in detector: one box typed by hand for the view the app starts with. It keeps the camera's
    /// frame from that view and always gives the box with it, so the label stays on the same thing in the
    /// room when the camera moves.
    /// </summary>
    public class FixedBoxSource : MonoBehaviour, IDetectionSource
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] string className = "laptop";
        [SerializeField, Range(0f, 1f)] float score = 1f;
        [Tooltip("The box as shares of the picture, from its top-left corner: left, top, width, height.")]
        [SerializeField] Rect box = new Rect(0.4f, 0.35f, 0.2f, 0.3f);

        CameraFrame startFrame;
        bool hasStartFrame;

        public string Name => Messages.FixedBoxSource;

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        void Update()
        {
            // Before the session tracks, the camera is not yet where the app starts.
            if (!hasStartFrame && viewCamera != null && ARSession.state == ARSessionState.SessionTracking)
            {
                KeepStartFrame();
            }
        }

        /// <summary>Takes the camera's frame as it is now and keeps it as the view the box was typed for.</summary>
        public void KeepStartFrame()
        {
            startFrame = CameraFrames.Of(viewCamera);
            hasStartFrame = true;
        }

        /// <summary>False until the start frame has been kept.</summary>
        public bool TryRead(List<Detection> into, out CameraFrame frame)
        {
            into.Clear();
            frame = startFrame;
            if (!hasStartFrame)
            {
                return false;
            }
            into.Add(new Detection(className, score,
                box.xMin * frame.PictureWidth, box.yMin * frame.PictureHeight,
                box.xMax * frame.PictureWidth, box.yMax * frame.PictureHeight));
            return true;
        }
    }
}
