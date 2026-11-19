using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>The usual depth of one class of object, front to back, in metres.</summary>
    [Serializable]
    public class ClassDepth
    {
        public string className;
        public float depth;

        /// <summary>
        /// For a class whose box is much wider than what it stands on, such as a plant with wide leaves:
        /// the set-back, which otherwise uses the box width, uses no more than this depth.
        /// </summary>
        public bool limitsSetBack;
    }

    /// <summary>
    /// A box in a picture gives an object's width and its height as seen, but not its depth. For the classes
    /// in a short table the usual depth is taken instead.
    /// </summary>
    public static class ObjectDepth
    {
        // Looking almost straight down, the box says nothing about the height any more.
        const float SteepestAngle = 75f;

        /// <summary>False for a class that is not in the table.</summary>
        public static bool TryUsual(string className, IReadOnlyList<ClassDepth> table, out ClassDepth usual)
        {
            for (int i = 0; i < table.Count; i++)
            {
                if (table[i].className == className)
                {
                    usual = table[i];
                    return true;
                }
            }
            usual = null;
            return false;
        }

        /// <summary>
        /// The depth the set-back uses: the box width, held to the usual depth for a class that limits it.
        /// <paramref name="usual"/> is null for a class that is not in the table.
        /// </summary>
        public static float ForSetBack(float boxWidth, ClassDepth usual)
        {
            return usual != null && usual.limitsSetBack ? Mathf.Min(boxWidth, usual.depth) : boxWidth;
        }

        /// <summary>
        /// The object's real height. Seen from above, the box in the picture also covers the object's top:
        /// its height as seen is the real height times the cosine of the angle looked down, plus the depth
        /// times the sine. This takes the depth's part out again. Never less than zero.
        /// </summary>
        public static float Height(float heightAsSeen, float depth, float degreesLookedDown)
        {
            float angle = Mathf.Clamp(degreesLookedDown, 0f, SteepestAngle) * Mathf.Deg2Rad;
            return Mathf.Max(0f, (heightAsSeen - depth * Mathf.Sin(angle)) / Mathf.Cos(angle));
        }
    }
}
