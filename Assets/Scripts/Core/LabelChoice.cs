using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which labels are shown when there are more than fit.</summary>
    public static class LabelChoice
    {
        /// <summary>
        /// Fills <paramref name="into"/> with the labels to show, at most <paramref name="max"/> of them: the object
        /// that is biggest in the picture first, and among those about equally big, the higher score first. Two
        /// labels the rule cannot tell apart keep the order they came in, so the choice is the same every frame.
        /// </summary>
        public static void Pick(IReadOnlyList<float> sizes, IReadOnlyList<float> scores, int max, float sameSizeShare,
            List<int> into)
        {
            into.Clear();
            for (int i = 0; i < sizes.Count; i++)
            {
                int at = into.Count;
                while (at > 0 && Compare(sizes[i], scores[i], sizes[into[at - 1]], scores[into[at - 1]], sameSizeShare) < 0)
                {
                    at--;
                }
                into.Insert(at, i);
            }
            KeepFirst(into, max);
        }

        /// <summary>
        /// The choice while the camera moves: every label that was shown stays, in the order it was laid out
        /// in (<paramref name="ranks"/>), and only places left free up to <paramref name="max"/> go to new
        /// labels, chosen as in <see cref="Pick"/>. <paramref name="scratch"/> is a list to work in.
        /// </summary>
        public static void Keep(IReadOnlyList<float> sizes, IReadOnlyList<float> scores, IReadOnlyList<bool> wasShown,
            IReadOnlyList<int> ranks, int max, float sameSizeShare, List<int> into, List<int> scratch)
        {
            into.Clear();
            for (int i = 0; i < sizes.Count; i++)
            {
                if (!wasShown[i])
                {
                    continue;
                }
                int at = into.Count;
                while (at > 0 && ranks[into[at - 1]] > ranks[i])
                {
                    at--;
                }
                into.Insert(at, i);
            }
            KeepFirst(into, max);

            Pick(sizes, scores, sizes.Count, sameSizeShare, scratch);
            foreach (int i in scratch)
            {
                if (into.Count >= max)
                {
                    break;
                }
                if (!wasShown[i])
                {
                    into.Add(i);
                }
            }
        }

        static void KeepFirst(List<int> order, int max)
        {
            int keep = Mathf.Max(max, 0);
            if (order.Count > keep)
            {
                order.RemoveRange(keep, order.Count - keep);
            }
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
