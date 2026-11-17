using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>How large a label is drawn.</summary>
    public static class LabelSizing
    {
        /// <summary>
        /// The label is designed to be read from 1 m. Its scale follows the distance, so the text looks the same
        /// size to the viewer, but only between the two limits: nearer it stops shrinking, farther it stops growing.
        /// </summary>
        public static float Scale(float distance, float nearLimit, float farLimit)
        {
            return Mathf.Clamp(distance, nearLimit, farLimit);
        }

        /// <summary>How tall something looks from a distance, as a viewing angle in degrees.</summary>
        public static float ViewingAngle(float height, float distance)
        {
            return 2f * Mathf.Atan2(height / 2f, distance) * Mathf.Rad2Deg;
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
