using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Says which of the objects found now are the same objects as in the frame before.</summary>
    public static class TrackMatch
    {
        /// <summary>
        /// Fills <paramref name="into"/> with one number for each object found now: the place in the earlier
        /// list of the object it continues, or -1 for a new one. Two objects are the same when they have the
        /// same name and lie no further apart than <paramref name="maxDistance"/>. The nearest pairs are
        /// matched first, and an earlier object is continued by one object only.
        /// </summary>
        public static void Match(IReadOnlyList<string> earlierNames, IReadOnlyList<Vector3> earlierPlaces,
            IReadOnlyList<string> names, IReadOnlyList<Vector3> places, float maxDistance, List<int> into)
        {
            into.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                into.Add(-1);
            }

            var pairs = new List<(float distance, int earlier, int now)>();
            for (int now = 0; now < names.Count; now++)
            {
                for (int earlier = 0; earlier < earlierNames.Count; earlier++)
                {
                    float distance = Vector3.Distance(places[now], earlierPlaces[earlier]);
                    if (names[now] == earlierNames[earlier] && distance <= maxDistance)
                    {
                        pairs.Add((distance, earlier, now));
                    }
                }
            }
            pairs.Sort((a, b) => a.distance.CompareTo(b.distance));

            var taken = new HashSet<int>();
            foreach ((float _, int earlier, int now) in pairs)
            {
                if (into[now] == -1 && taken.Add(earlier))
                {
                    into[now] = earlier;
                }
            }
        }
    }
}
