using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>How clearly a label is drawn at a distance.</summary>
    public static class LabelFade
    {
        /// <summary>1 up to where the fade starts, 0 from where it ends, and evenly less in between.</summary>
        public static float Opacity(float distance, float fadeStart, float fadeEnd)
        {
            if (fadeEnd <= fadeStart)
            {
                return distance < fadeStart ? 1f : 0f;
            }
            return 1f - Mathf.Clamp01((distance - fadeStart) / (fadeEnd - fadeStart));
        }
    }
}
