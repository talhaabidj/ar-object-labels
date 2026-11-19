using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>An object that has a label, kept at its place in the room.</summary>
    public class KeptObject
    {
        public string Name;
        public Bounds RoomBox;
        public Quaternion Turn;
        public float Score;

        /// <summary>Seconds the object has been in view without being found.</summary>
        public float UnseenInView;

        /// <summary>True when the object was found in the last frame taken.</summary>
        public bool FoundNow;

        /// <summary>What its label remembers from frame to frame.</summary>
        public readonly LabelMemory Memory = new LabelMemory();

        /// <summary>1 while the label is drawn fully, down to 0 as it fades out.</summary>
        public float Fade = 1f;
    }

    /// <summary>
    /// Keeps a label at its object's place in the room. A label whose object is out of view stays where it is,
    /// so it is there again when the object comes back. A label whose object is in view but not found waits a
    /// moment, then fades out and is dropped.
    /// </summary>
    public class LabelKeeper
    {
        readonly List<KeptObject> kept = new List<KeptObject>();
        readonly List<string> keptNames = new List<string>();
        readonly List<Vector3> keptPlaces = new List<Vector3>();
        readonly List<Vector3> places = new List<Vector3>();
        readonly List<int> continues = new List<int>();
        readonly HashSet<KeptObject> foundNow = new HashSet<KeptObject>();

        public IReadOnlyList<KeptObject> Kept => kept;

        public void Forget()
        {
            kept.Clear();
            foundNow.Clear();
        }

        /// <summary>
        /// Takes what was found in this frame. An object found again near its kept place, under the same name,
        /// is moved to where it is found now; an object not kept yet is added.
        /// </summary>
        public void Take(IReadOnlyList<string> names, IReadOnlyList<Bounds> boxes, IReadOnlyList<Quaternion> turns,
            IReadOnlyList<float> scores, float sameObjectDistance)
        {
            keptNames.Clear();
            keptPlaces.Clear();
            foreach (KeptObject one in kept)
            {
                keptNames.Add(one.Name);
                keptPlaces.Add(one.RoomBox.center);
            }
            places.Clear();
            foreach (Bounds box in boxes)
            {
                places.Add(box.center);
            }
            TrackMatch.Match(keptNames, keptPlaces, names, places, sameObjectDistance, continues);

            foundNow.Clear();
            foreach (KeptObject one in kept)
            {
                one.FoundNow = false;
            }
            for (int i = 0; i < names.Count; i++)
            {
                KeptObject one = continues[i] >= 0 ? kept[continues[i]] : new KeptObject { Name = names[i] };
                if (continues[i] < 0)
                {
                    kept.Add(one);
                }
                one.RoomBox = boxes[i];
                one.Turn = turns[i];
                one.Score = scores[i];
                one.UnseenInView = 0f;
                one.Fade = 1f;
                one.FoundNow = true;
                foundNow.Add(one);
            }
        }

        /// <summary>
        /// Counts the time each object not found in this frame has been in view. After
        /// <paramref name="waitSeconds"/> its label fades out over <paramref name="fadeSeconds"/> and is then
        /// dropped. <paramref name="isInView"/> tells whether a place in the room is in the camera's picture.
        /// </summary>
        public void Age(Func<Vector3, bool> isInView, float secondsSinceLast, float waitSeconds, float fadeSeconds)
        {
            for (int i = kept.Count - 1; i >= 0; i--)
            {
                KeptObject one = kept[i];
                if (foundNow.Contains(one) || !isInView(one.RoomBox.center))
                {
                    continue;
                }
                one.UnseenInView += secondsSinceLast;
                float fading = one.UnseenInView - waitSeconds;
                one.Fade = fading <= 0f ? 1f : fadeSeconds > 0f ? 1f - Mathf.Clamp01(fading / fadeSeconds) : 0f;
                if (one.Fade <= 0f)
                {
                    kept.RemoveAt(i);
                }
            }
        }
    }
}
