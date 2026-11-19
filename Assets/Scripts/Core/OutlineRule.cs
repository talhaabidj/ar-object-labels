using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>How strongly the found surfaces are drawn.</summary>
    public static class OutlineRule
    {
        /// <summary>
        /// The share of their full strength: all of it while the cube is being moved or carried, and
        /// <paramref name="faintShare"/> the rest of the time, so they do not cross every picture.
        /// </summary>
        public static float Strength(bool cubeInHand, float faintShare)
        {
            return cubeInHand ? 1f : Mathf.Clamp01(faintShare);
        }
    }
}
