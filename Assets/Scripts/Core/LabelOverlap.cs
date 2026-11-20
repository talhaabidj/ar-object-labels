using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Keeps two labels from sitting on top of each other.</summary>
    public static class LabelOverlap
    {
        // Two edges closer than this count as touching, so rounding can never ask for the same move twice.
        const float Tolerance = 0.00001f;

        /// <summary>
        /// How far a label must move up the viewer's picture to clear the rectangles of two lists. The rectangles
        /// are in the picture's own flat coordinates. Zero when nothing is in the way.
        /// </summary>
        public static float LiftToClear(Rect label, IReadOnlyList<Rect> first, IReadOnlyList<Rect> second, float margin)
        {
            float lift = 0f;
            // Each pass moves the label above at least one more rectangle, so one pass per rectangle is enough.
            int passes = first.Count + second.Count;
            for (int pass = 0; pass < passes; pass++)
            {
                float needed = Mathf.Max(LiftPastTheHighestInTheWay(label, lift, first, margin),
                    LiftPastTheHighestInTheWay(label, lift, second, margin));
                if (needed <= lift) break;
                lift = needed;
            }
            return lift;
        }

        static float LiftPastTheHighestInTheWay(Rect label, float lift, IReadOnlyList<Rect> placedBefore, float margin)
        {
            float bottom = label.yMin + lift;
            float top = label.yMax + lift;
            float needed = lift;
            foreach (Rect other in placedBefore)
            {
                bool sideBySide = label.xMin < other.xMax + margin && other.xMin < label.xMax + margin;
                float clearHeight = other.yMax + margin;
                bool inTheWay = sideBySide && bottom < clearHeight - Tolerance && other.yMin < top + margin;
                if (inTheWay) needed = Mathf.Max(needed, clearHeight - label.yMin);
            }
            return needed;
        }
    }
}
