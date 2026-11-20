using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Says which of the objects found now are the same objects as in the frame before.</summary>
    public class TrackMatch
    {
        struct Pair
        {
            public float Distance;
            public int Earlier;
            public int Now;
        }

        sealed class NearestFirst : IComparer<Pair>
        {
            public static readonly NearestFirst Instance = new NearestFirst();

            public int Compare(Pair a, Pair b)
            {
                return a.Distance.CompareTo(b.Distance);
            }
        }

        // Kept between calls so a match every frame makes no new lists.
        readonly List<Pair> pairs = new List<Pair>();
        readonly List<bool> earlierTaken = new List<bool>();

        /// <summary>
        /// Fills <paramref name="into"/> with one number for each object found now: the place in the earlier
        /// list of the object it continues, or -1 for a new one. Two objects are the same when they have the
        /// same name and lie no further apart than <paramref name="maxDistance"/>. The nearest pairs are
        /// matched first, and an earlier object is continued by one object only.
        /// </summary>
        public void Match(IReadOnlyList<string> earlierNames, IReadOnlyList<Vector3> earlierPlaces,
            IReadOnlyList<string> names, IReadOnlyList<Vector3> places, float maxDistance, List<int> into)
        {
            into.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                into.Add(-1);
            }

            pairs.Clear();
            for (int now = 0; now < names.Count; now++)
            {
                for (int earlier = 0; earlier < earlierNames.Count; earlier++)
                {
                    float distance = Vector3.Distance(places[now], earlierPlaces[earlier]);
                    if (names[now] == earlierNames[earlier] && distance <= maxDistance)
                    {
                        pairs.Add(new Pair { Distance = distance, Earlier = earlier, Now = now });
                    }
                }
            }
            pairs.Sort(NearestFirst.Instance);

            earlierTaken.Clear();
            for (int i = 0; i < earlierNames.Count; i++)
            {
                earlierTaken.Add(false);
            }
            foreach (Pair pair in pairs)
            {
                if (into[pair.Now] == -1 && !earlierTaken[pair.Earlier])
                {
                    into[pair.Now] = pair.Earlier;
                    earlierTaken[pair.Earlier] = true;
                }
            }
        }
    }
}
