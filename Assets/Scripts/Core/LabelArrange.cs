using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// Finds a free place for a label among other objects and the labels placed before it, inside the picture.
    /// All rectangles are in the viewer's flat picture, in the same units.
    /// </summary>
    public static class LabelArrange
    {
        /// <summary>
        /// How far the label must move in the picture from its own place above its object. Zero when that place
        /// is free. Otherwise the nearby places are tried in a fixed order and the first free one is taken: one
        /// step up, beside the object on the right, on the left, up and to the right, up and to the left, two
        /// steps up. A free place touches no other object and no label placed before. If none of them is free,
        /// the label moves straight up until it is clear of everything.
        ///
        /// Every place is first pulled inside <paramref name="picture"/>, the part of the flat picture the
        /// viewer sees, so a label is never cut by the picture's edge, and out from under the app's
        /// <paramref name="panels"/>, which count as more of that edge. Only the label of an object found now
        /// (<paramref name="objectFound"/>) or with its middle in the picture is pulled in; any other label
        /// stays at its place in the room.
        /// </summary>
        public static Vector2 Offset(Rect label, Rect ownObject, IReadOnlyList<Rect> otherObjects,
            IReadOnlyList<Rect> placedLabels, float margin, Rect picture, IReadOnlyList<Rect> panels = null,
            bool objectFound = false)
        {
            bool objectInPicture = Touches(ownObject, picture, 0f) && (objectFound || picture.Contains(ownObject.center));
            for (int choice = 0; choice < NearbyPlaces; choice++)
            {
                Rect place = Nearby(choice, label, ownObject, margin);
                if (objectInPicture)
                {
                    place = PanelEdges.KeepOut(KeepInside(place, picture), panels, margin, picture);
                }
                if (IsFree(place, otherObjects, placedLabels, margin) && PanelEdges.IsClear(place, panels))
                {
                    return new Vector2(place.x - label.x, place.y - label.y);
                }
            }

            var lifted = new Rect(label.x, label.y + LabelOverlap.LiftToClear(label, placedLabels, otherObjects, margin), label.width, label.height);
            if (objectInPicture)
            {
                lifted = PanelEdges.KeepOut(KeepInside(lifted, picture), panels, margin, picture);
            }
            return new Vector2(lifted.x - label.x, lifted.y - label.y);
        }

        /// <summary>
        /// True when a label may take one of the places among the labels shown: its object reaches into the
        /// picture, and the object is found now, or has its middle in the picture, or the label at its own
        /// place would be at least partly in the picture. A label that would only sit off the picture, or whose
        /// own place is behind the camera (<paramref name="labelInFront"/> false), takes none.
        /// </summary>
        public static bool TakesAPlace(Rect ownObject, Rect ownLabel, bool objectFound, Rect picture, bool labelInFront = true)
        {
            return labelInFront && Touches(ownObject, picture, 0f)
                && (objectFound || picture.Contains(ownObject.center) || Touches(ownLabel, picture, 0f));
        }

        /// <summary>
        /// The rectangle moved by the least needed to lie inside the picture. One that is wider or higher
        /// than the picture is put in its middle.
        /// </summary>
        public static Rect KeepInside(Rect place, Rect picture)
        {
            float x = place.width > picture.width ? picture.center.x - place.width / 2f
                : Mathf.Clamp(place.x, picture.xMin, picture.xMax - place.width);
            float y = place.height > picture.height ? picture.center.y - place.height / 2f
                : Mathf.Clamp(place.y, picture.yMin, picture.yMax - place.height);
            return new Rect(x, y, place.width, place.height);
        }

        /// <summary>
        /// True when a place the label already holds can be kept: it touches no other object, no label placed
        /// before and none of the app's panels, and lies inside the picture.
        /// </summary>
        public static bool CanStay(Rect place, IReadOnlyList<Rect> otherObjects, IReadOnlyList<Rect> placedLabels,
            float margin, Rect picture, IReadOnlyList<Rect> panels = null)
        {
            // A place that was pulled to the picture's edge lies on it, give or take rounding.
            bool inside = place.xMin >= picture.xMin - Rounding && place.xMax <= picture.xMax + Rounding
                && place.yMin >= picture.yMin - Rounding && place.yMax <= picture.yMax + Rounding;
            return inside && IsFree(place, otherObjects, placedLabels, margin) && PanelEdges.IsClear(place, panels);
        }

        const float Rounding = 0.0001f;
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
