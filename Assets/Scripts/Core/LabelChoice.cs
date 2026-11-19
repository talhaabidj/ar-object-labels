using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which labels are shown when there are more than fit.</summary>
    public static class LabelChoice
    {
        /// <summary>
        /// The labels to show, at most <paramref name="max"/> of them: the object that is biggest in the picture
        /// first, and among those about equally big, the higher score first. Returns their places in the
        /// lists, in that order.
        /// </summary>
        public static List<int> Pick(IReadOnlyList<float> sizes, IReadOnlyList<float> scores, int max, float sameSizeShare)
        {
            var order = new List<int>();
            for (int i = 0; i < sizes.Count; i++)
            {
                order.Add(i);
            }
            order.Sort((a, b) =>
            {
                int byRule = Compare(sizes[a], scores[a], sizes[b], scores[b], sameSizeShare);
                // Two labels the rule cannot tell apart keep the order they came in, so the choice is the same every frame.
                return byRule != 0 ? byRule : a.CompareTo(b);
            });
            if (order.Count > max)
            {
                order.RemoveRange(Mathf.Max(max, 0), order.Count - Mathf.Max(max, 0));
            }
            return order;
        }

        /// <summary>
        /// The choice while the camera moves: every label that was shown stays, in the order it was laid out
        /// in (<paramref name="ranks"/>), and only places left free up to <paramref name="max"/> go to new
        /// labels, chosen as in <see cref="Pick"/>.
        /// </summary>
        public static List<int> Keep(IReadOnlyList<float> sizes, IReadOnlyList<float> scores, IReadOnlyList<bool> wasShown,
            IReadOnlyList<int> ranks, int max, float sameSizeShare)
        {
            var kept = new List<int>();
            for (int i = 0; i < sizes.Count; i++)
            {
                if (wasShown[i])
                {
                    kept.Add(i);
                }
            }
            kept.Sort((a, b) => ranks[a] != ranks[b] ? ranks[a].CompareTo(ranks[b]) : a.CompareTo(b));
            if (kept.Count > max)
            {
                kept.RemoveRange(Mathf.Max(max, 0), kept.Count - Mathf.Max(max, 0));
            }

            foreach (int i in Pick(sizes, scores, sizes.Count, sameSizeShare))
            {
                if (kept.Count >= max)
                {
                    break;
                }
                if (!wasShown[i])
                {
                    kept.Add(i);
                }
            }
            return kept;
        }

        static int Compare(float sizeA, float scoreA, float sizeB, float scoreB, float sameSizeShare)
        {
            int stepA = SizeStep(sizeA, sameSizeShare);
            int stepB = SizeStep(sizeB, sameSizeShare);
            if (stepA != stepB)
            {
                return stepB.CompareTo(stepA);
            }
            if (scoreA != scoreB)
            {
                return scoreB.CompareTo(scoreA);
            }
            return sizeB.CompareTo(sizeA);
        }

        // Sizes are sorted into steps, each bigger than the one before by the given share. Two sizes on one
        // step differ by less than that share and count as equally big.
        static int SizeStep(float size, float sameSizeShare)
        {
            if (size <= 0f)
            {
                return int.MinValue;
            }
            return sameSizeShare > 0f ? Mathf.FloorToInt(Mathf.Log(size) / Mathf.Log(1f + sameSizeShare)) : 0;
        }
    }
}
