using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// A stand-in detector: one box typed by hand. It always "finds" the same class in the same place of the
    /// picture, whatever the camera sees.
    /// </summary>
    public class FixedBoxSource : MonoBehaviour, IDetectionSource
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] string className = "laptop";
        [SerializeField, Range(0f, 1f)] float score = 1f;
        [Tooltip("The box as shares of the picture, from its top-left corner: left, top, width, height.")]
        [SerializeField] Rect box = new Rect(0.4f, 0.35f, 0.2f, 0.3f);

        public string Name => Messages.FixedBoxSource;

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        public bool TryRead(List<Detection> into, out CameraFrame frame)
        {
            into.Clear();
            if (viewCamera == null)
            {
                frame = default;
                return false;
            }
            frame = CameraFrames.Of(viewCamera);
            into.Add(new Detection(className, score,
                box.xMin * frame.PictureWidth, box.yMin * frame.PictureHeight,
                box.xMax * frame.PictureWidth, box.yMax * frame.PictureHeight));
            return true;
        }
    }
}
