using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// Finds a free place for a label among other objects and the labels placed before it. All rectangles are in
    /// the viewer's flat picture, in the same units.
    /// </summary>
    public static class LabelArrange
    {
        /// <summary>
        /// How far the label must move in the picture from its own place above its object. Zero when that place
        /// is free. Otherwise the nearby places are tried in a fixed order and the first free one is taken: one
        /// step up, beside the object on the right, on the left, up and to the right, up and to the left, two
        /// steps up. A free place touches no other object and no label placed before. If none of them is free,
        /// the label moves straight up until it is clear of everything.
        /// </summary>
        public static Vector2 Offset(Rect label, Rect ownObject, IReadOnlyList<Rect> otherObjects,
            IReadOnlyList<Rect> placedLabels, float margin)
        {
            for (int choice = 0; choice < NearbyPlaces; choice++)
            {
                Rect place = Nearby(choice, label, ownObject, margin);
                if (IsFree(place, otherObjects, placedLabels, margin))
                {
                    return new Vector2(place.x - label.x, place.y - label.y);
                }
            }
            return new Vector2(0f, LabelOverlap.LiftToClear(label, placedLabels, otherObjects, margin));
        }

        const int NearbyPlaces = 7;

        // The places a label may take, nearest to its own place first.
        static Rect Nearby(int choice, Rect label, Rect ownObject, float margin)
        {
            float step = label.height + margin;
            float rightOfObject = ownObject.xMax + margin;
            float leftOfObject = ownObject.xMin - margin - label.width;
            float besideHeight = ownObject.center.y - label.height / 2f;
            switch (choice)
            {
                case 0: return label;
                case 1: return new Rect(label.x, label.y + step, label.width, label.height);
                case 2: return new Rect(rightOfObject, besideHeight, label.width, label.height);
                case 3: return new Rect(leftOfObject, besideHeight, label.width, label.height);
                case 4: return new Rect(rightOfObject, label.y, label.width, label.height);
                case 5: return new Rect(leftOfObject, label.y, label.width, label.height);
                default: return new Rect(label.x, label.y + 2f * step, label.width, label.height);
            }
        }

        static bool IsFree(Rect place, IReadOnlyList<Rect> otherObjects, IReadOnlyList<Rect> placedLabels, float margin)
        {
            return !TouchesAny(place, otherObjects, margin) && !TouchesAny(place, placedLabels, margin);
        }

        /// <summary>
        /// True when another object lies behind or under the label's own object in the picture: its rectangle
        /// holds the middle of the own object. Such an object cannot be avoided, so it is not in the way.
        /// </summary>
        public static bool HoldsTheObject(Rect other, Rect ownObject)
        {
            return other.Contains(ownObject.center);
        }

        /// <summary>True when two rectangles overlap or are closer than the margin.</summary>
        public static bool Touches(Rect a, Rect b, float margin)
        {
            return a.xMin < b.xMax + margin && b.xMin < a.xMax + margin
                && a.yMin < b.yMax + margin && b.yMin < a.yMax + margin;
        }

        static bool TouchesAny(Rect rect, IReadOnlyList<Rect> others, float margin)
        {
            foreach (Rect other in others)
            {
                if (Touches(rect, other, margin))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
