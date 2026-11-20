using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>How large a label is drawn.</summary>
    public static class LabelSizing
    {
        /// <summary>
        /// The label is designed to be read from 1 m. Its scale follows its depth in front of the camera, so the
        /// text is the same height on the screen at every distance. Only nearer than
        /// <paramref name="nearLimit"/> does it stop shrinking, so it never shrinks to nothing.
        /// </summary>
        public static float Scale(float depth, float nearLimit)
        {
            return Mathf.Max(depth, nearLimit);
        }

        /// <summary>How far a point lies in front of the camera, along the way it looks. Negative behind it.</summary>
        public static float Depth(Vector3 point, Vector3 cameraPosition, Vector3 cameraForward)
        {
            return Vector3.Dot(point - cameraPosition, cameraForward);
        }

        /// <summary>
        /// The size factor held back so the label is never wider than <paramref name="maxWidth"/>, in metres.
        /// <paramref name="widthAtOne"/> is the label's width at a size factor of 1. No limit when maxWidth is 0.
        /// </summary>
        public static float CapToWidth(float scale, float widthAtOne, float maxWidth)
        {
            if (maxWidth <= 0f || widthAtOne <= 0f)
            {
                return scale;
            }
            return Mathf.Min(scale, maxWidth / widthAtOne);
        }
    }
}
