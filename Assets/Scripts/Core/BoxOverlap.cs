using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>A model finds the same object many times over. This keeps one box for each object.</summary>
    public static class BoxOverlap
    {
        /// <summary>The part of two boxes' joint area that they share: 0 when apart, 1 when the same box.</summary>
        public static float Share(Detection a, Detection b)
        {
            float width = Mathf.Min(a.Right, b.Right) - Mathf.Max(a.Left, b.Left);
            float height = Mathf.Min(a.Bottom, b.Bottom) - Mathf.Max(a.Top, b.Top);
            if (width <= 0f || height <= 0f)
            {
                return 0f;
            }
            float shared = width * height;
            float joint = a.Width * a.Height + b.Width * b.Height - shared;
            return joint > 0f ? shared / joint : 0f;
        }

        /// <summary>
        /// Sorts the boxes by score, highest first, and removes every box that shares more than
        /// <paramref name="overlapLimit"/> with a kept box of the same class.
        /// </summary>
        public static void KeepTheBest(List<Detection> boxes, float overlapLimit)
        {
            boxes.Sort((a, b) => b.Score.CompareTo(a.Score));
            int kept = 0;
            for (int i = 0; i < boxes.Count; i++)
            {
                if (!SharesWithAKeptBox(boxes, kept, boxes[i], overlapLimit))
                {
                    boxes[kept] = boxes[i];
                    kept++;
                }
            }
            boxes.RemoveRange(kept, boxes.Count - kept);
        }

        static bool SharesWithAKeptBox(List<Detection> boxes, int kept, Detection candidate, float overlapLimit)
        {
            for (int i = 0; i < kept; i++)
            {
                if (boxes[i].ClassName == candidate.ClassName && Share(boxes[i], candidate) > overlapLimit)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
