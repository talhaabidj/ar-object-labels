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

        /// <summary>Seconds since the object was last found, in view or not.</summary>
        public float Unseen;

        /// <summary>True when the object was found in the last frame taken.</summary>
        public bool FoundNow;

        /// <summary>What its label remembers from frame to frame.</summary>
        public readonly LabelMemory Memory = new LabelMemory();

        /// <summary>1 while the label is drawn fully, down to 0 as it fades out.</summary>
        public float Fade = 1f;
    }

    /// <summary>
    /// Keeps a label at its object's place in the room. A label whose object is out of view stays where it is
    /// for a while, so it is there again when the object comes back. A label whose object is in view but not
    /// found waits a moment, then fades out and is dropped. Any label whose object has not been found for long
    /// is dropped too, in view or not.
    /// </summary>
    public class LabelKeeper
    {
        readonly List<KeptObject> kept = new List<KeptObject>();
        readonly List<string> keptNames = new List<string>();
        readonly List<Vector3> keptPlaces = new List<Vector3>();
        readonly List<Vector3> places = new List<Vector3>();
        readonly List<int> continues = new List<int>();
        readonly TrackMatch matcher = new TrackMatch();
        readonly HashSet<KeptObject> foundNow = new HashSet<KeptObject>();
        readonly HashSet<int> taken = new HashSet<int>();

        public IReadOnlyList<KeptObject> Kept => kept;

        public void Forget()
        {
            kept.Clear();
            foundNow.Clear();
        }

        /// <summary>
        /// Takes what was found in this frame. An object found again near its kept place, under the same name,
        /// is moved to where it is found now. A found object with no kept object that near is still the same as
        /// a kept object of its name that is not found now when the kept room point falls inside the found box
        /// on the screen (<paramref name="seenInsideBox"/>: found number, room point), or when the two room boxes
        /// overlap; that object is moved, so one object never gets a second label because its place moved.
        /// Only an object that matches neither way is added.
        /// </summary>
        public void Take(IReadOnlyList<string> names, IReadOnlyList<Bounds> boxes, IReadOnlyList<Quaternion> turns,
            IReadOnlyList<float> scores, float sameObjectDistance, Func<int, Vector3, bool> seenInsideBox = null)
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
            matcher.Match(keptNames, keptPlaces, names, places, sameObjectDistance, continues);
            MatchMovedObjects(names, boxes, turns, seenInsideBox);

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
                one.Unseen = 0f;
                one.Fade = 1f;
                one.FoundNow = true;
                foundNow.Add(one);
            }
        }

        // The duplicate rule from Meta's object detection sample: a kept object of the same name that lies inside
        // the new box is the same object, wherever its room point was put before.
        void MatchMovedObjects(IReadOnlyList<string> names, IReadOnlyList<Bounds> boxes, IReadOnlyList<Quaternion> turns,
            Func<int, Vector3, bool> seenInsideBox)
        {
            taken.Clear();
            foreach (int k in continues)
            {
                if (k >= 0)
                {
                    taken.Add(k);
                }
            }
            for (int i = 0; i < names.Count; i++)
            {
                if (continues[i] >= 0)
                {
                    continue;
                }
                int best = -1;
                float bestDistance = float.PositiveInfinity;
                for (int k = 0; k < kept.Count; k++)
                {
                    KeptObject one = kept[k];
                    if (one.Name != names[i] || taken.Contains(k))
                    {
                        continue;
                    }
                    bool inside = seenInsideBox != null && seenInsideBox(i, one.RoomBox.center);
                    bool overlap = TurnedBoxesOverlap(one.RoomBox, one.Turn, boxes[i], turns[i]);
                    float distance = Vector3.Distance(one.RoomBox.center, boxes[i].center);
                    if ((inside || overlap) && distance < bestDistance)
                    {
                        best = k;
                        bestDistance = distance;
                    }
                }
                if (best >= 0)
                {
                    continues[i] = best;
                    taken.Add(best);
                }
            }
        }

        /// <summary>
        /// True when two room boxes, each given in its own axes with the turn of those axes, overlap. Each is
        /// widened to the upright box around its turned corners first, which is enough to tell one place from another.
        /// </summary>
        public static bool TurnedBoxesOverlap(Bounds a, Quaternion turnA, Bounds b, Quaternion turnB)
        {
            return Upright(a, turnA).Intersects(Upright(b, turnB));
        }

        static Bounds Upright(Bounds box, Quaternion turn)
        {
            var upright = new Bounds(box.center, Vector3.zero);
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                upright.Encapsulate(box.center + turn * Vector3.Scale(box.extents, sign));
            }
            return upright;
        }

        /// <summary>True when a room point is seen inside a box in the picture of this camera frame.</summary>
        public static bool SeenInside(Detection box, CameraFrame frame, Vector3 roomPoint)
        {
            return frame.TryPicturePoint(roomPoint, out Vector2 point)
                && point.x >= box.Left && point.x <= box.Right && point.y >= box.Top && point.y <= box.Bottom;
        }

        /// <summary>
        /// Counts the time each object not found in this frame has gone unfound, and how much of it in view.
        /// After <paramref name="waitSeconds"/> in view, or <paramref name="forgetSeconds"/> in all, its label
        /// fades out over <paramref name="fadeSeconds"/> and is then dropped. <paramref name="isInView"/> tells
        /// whether a place in the room is in the camera's picture.
        /// </summary>
        public void Age(Func<Vector3, bool> isInView, float secondsSinceLast, float waitSeconds, float fadeSeconds,
            float forgetSeconds = float.PositiveInfinity)
        {
            for (int i = kept.Count - 1; i >= 0; i--)
            {
                KeptObject one = kept[i];
                if (foundNow.Contains(one))
                {
                    continue;
                }
                one.Unseen += secondsSinceLast;
                if (isInView(one.RoomBox.center))
                {
                    one.UnseenInView += secondsSinceLast;
                }
                float fading = Mathf.Max(one.UnseenInView - waitSeconds, one.Unseen - forgetSeconds);
                one.Fade = fading <= 0f ? 1f : fadeSeconds > 0f ? 1f - Mathf.Clamp01(fading / fadeSeconds) : 0f;
                if (one.Fade <= 0f)
                {
                    kept.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// How strongly a kept object's label is drawn: as its fade says while the object is found, and only
        /// <paramref name="notFoundShare"/> of that once it has gone unfound for more than
        /// <paramref name="graceSeconds"/>, so a label for something not seen now does not look like a fresh one.
        /// </summary>
        public static float Strength(KeptObject one, float graceSeconds, float notFoundShare)
        {
            return one.Fade * (one.Unseen > graceSeconds ? Mathf.Clamp01(notFoundShare) : 1f);
        }
    }
}
