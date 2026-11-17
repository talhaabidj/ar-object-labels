using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which labels are shown when there are more than fit.</summary>
    public static class LabelChoice
    {
        /// <summary>
        /// The labels to show, at most <paramref name="max"/> of them: nearest first, and among those about
        /// equally near, the higher score first. Returns their places in the lists, in that order.
        /// </summary>
        public static List<int> Pick(IReadOnlyList<float> distances, IReadOnlyList<float> scores, int max, float sameDistanceStep)
        {
            var order = new List<int>();
            for (int i = 0; i < distances.Count; i++)
            {
                order.Add(i);
            }
            order.Sort((a, b) => Compare(distances[a], scores[a], distances[b], scores[b], sameDistanceStep));
            if (order.Count > max)
            {
                order.RemoveRange(Mathf.Max(max, 0), order.Count - Mathf.Max(max, 0));
            }
            return order;
        }

        static int Compare(float distanceA, float scoreA, float distanceB, float scoreB, float sameDistanceStep)
        {
            int bandA = Band(distanceA, sameDistanceStep);
            int bandB = Band(distanceB, sameDistanceStep);
            if (bandA != bandB)
            {
                return bandA.CompareTo(bandB);
            }
            if (scoreA != scoreB)
            {
                return scoreB.CompareTo(scoreA);
            }
            return distanceA.CompareTo(distanceB);
        }

        static int Band(float distance, float sameDistanceStep)
        {
            return sameDistanceStep > 0f ? Mathf.FloorToInt(distance / sameDistanceStep) : 0;
        }
    }
}
