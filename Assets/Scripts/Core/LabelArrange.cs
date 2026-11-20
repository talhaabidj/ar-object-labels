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
        /// How far the label must move in the picture from its own place above its object. Only the label of an
        /// object found now (<paramref name="objectFound"/>) or with its middle in the picture is moved; any other
        /// label stays at its place in the room.
        ///
        /// A free place touches no other object and no label placed before. Every place is first pulled inside
        /// <paramref name="picture"/> and out from under the app's <paramref name="panels"/>. The nearby places are
        /// tried in a fixed order: the own place, one step up, beside the object on the right, on the left, up and
        /// to the right, up and to the left, two steps up.
        ///
        /// What is taken, in order: a free nearby place near enough; the place straight up until clear, if near
        /// enough; a nearby place clear of earlier labels and panels, even with another object behind it; any free
        /// nearby place; any nearby place clear of earlier labels and panels; the place straight up. Near enough
        /// means the line to <paramref name="objectTop"/> (where the object's top shows in the picture; left out,
        /// the middle of the label's own lower edge) is no longer than <paramref name="maxLeader"/>, or no longer
        /// than the line from the label's own place, so looking down does not push a label off its own place.
        /// </summary>
        public static Vector2 Offset(Rect label, Rect ownObject, IReadOnlyList<Rect> otherObjects,
            IReadOnlyList<Rect> placedLabels, float margin, Rect picture, IReadOnlyList<Rect> panels = null,
            bool objectFound = false, float maxLeader = float.PositiveInfinity, Vector2? objectTop = null)
        {
            bool objectInPicture = Touches(ownObject, picture, 0f) && (objectFound || picture.Contains(ownObject.center));
            // The label of an object that is out of the picture, or not found and mostly out of it, is a kept label:
            // it stays at its room point and is not moved aside.
            if (!objectInPicture)
            {
                return Vector2.zero;
            }
            Vector2 top = objectTop ?? new Vector2(label.center.x, label.yMin);
            float nearEnough = Mathf.Max(maxLeader, LeaderLength(label, top));
            Rect place;
            if (TryNearby(label, ownObject, otherObjects, placedLabels, margin, picture, panels, top, nearEnough, out place))
            {
                return MoveTo(label, place);
            }

            var lifted = new Rect(label.x, label.y + LabelOverlap.LiftToClear(label, placedLabels, otherObjects, margin), label.width, label.height);
            lifted = PanelEdges.KeepOut(KeepInside(lifted, picture), panels, margin, picture);
            if (LeaderLength(lifted, top) <= nearEnough)
            {
                return MoveTo(label, lifted);
            }

            // Seen in the review: the desk chair's label was lifted clear of the desk and its line crossed the whole
            // desk. It stays near, in front of the desk.
            if (TryNearby(label, ownObject, NoRects, placedLabels, margin, picture, panels, top, nearEnough, out place))
            {
                return MoveTo(label, place);
            }

            // Seen in Play on 20 Nov: a label whose own place is above the picture, or whose near places all held
            // labels, stayed there, off the picture or on another label. A long line is hidden instead.
            if (TryNearby(label, ownObject, otherObjects, placedLabels, margin, picture, panels, top, float.PositiveInfinity, out place)
                || TryNearby(label, ownObject, NoRects, placedLabels, margin, picture, panels, top, float.PositiveInfinity, out place))
            {
                return MoveTo(label, place);
            }
            return MoveTo(label, lifted);
        }

        static readonly Rect[] NoRects = new Rect[0];

        // The first nearby place, kept inside the picture and out of the panels, that touches none of these objects
        // and labels and is no further from the object's top than the longest line.
        static bool TryNearby(Rect label, Rect ownObject, IReadOnlyList<Rect> objectsInTheWay, IReadOnlyList<Rect> placedLabels,
            float margin, Rect picture, IReadOnlyList<Rect> panels, Vector2 objectTop, float maxLeader, out Rect place)
        {
            for (int choice = 0; choice < NearbyPlaces; choice++)
            {
                place = PanelEdges.KeepOut(KeepInside(Nearby(choice, label, ownObject, margin), picture), panels, margin, picture);
                if (IsFree(place, objectsInTheWay, placedLabels, margin) && PanelEdges.IsClear(place, panels)
                    && LeaderLength(place, objectTop) <= maxLeader)
                {
                    return true;
                }
            }
            place = label;
            return false;
        }

        static Vector2 MoveTo(Rect label, Rect place)
        {
            return new Vector2(place.x - label.x, place.y - label.y);
        }

        /// <summary>How long the line from a label at this place to a point would be: zero when the point is on the label.</summary>
        public static float LeaderLength(Rect place, Vector2 point)
        {
            float dx = Mathf.Max(place.xMin - point.x, 0f, point.x - place.xMax);
            float dy = Mathf.Max(place.yMin - point.y, 0f, point.y - place.yMax);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// True when a label may take one of the places among the labels shown: its object reaches into the
        /// picture, and the object is found now, or has its middle in the picture, or the label at its own
        /// place would be at least partly in the picture. A label that would only sit off the picture, or whose
        /// own place is behind the camera (<paramref name="labelInFront"/> false), takes none. Nor does the label
        /// of an object less than <paramref name="minShareInPicture"/> in the picture: pulled in from the edge,
        /// such a label would jump to a new place in every frame while the camera moves.
        /// </summary>
        public static bool TakesAPlace(Rect ownObject, Rect ownLabel, bool objectFound, Rect picture, bool labelInFront = true,
            float minShareInPicture = 0f)
        {
            return labelInFront && Touches(ownObject, picture, 0f)
                && ShareInPicture(ownObject, picture) >= minShareInPicture
                && (objectFound || picture.Contains(ownObject.center) || Touches(ownLabel, picture, 0f));
        }

        /// <summary>
        /// How much of an object is in the picture, from 0 to 1, taken across and up apart. Along a side where
        /// the object is bigger than the picture, it counts as wholly in when it fills the picture, so a big
        /// object close to the camera is not left out.
        /// </summary>
        public static float ShareInPicture(Rect ownObject, Rect picture)
        {
            return ShareAlong(ownObject.xMin, ownObject.xMax, picture.xMin, picture.xMax)
                * ShareAlong(ownObject.yMin, ownObject.yMax, picture.yMin, picture.yMax);
        }

        static float ShareAlong(float objectMin, float objectMax, float pictureMin, float pictureMax)
        {
            float overlap = Mathf.Max(0f, Mathf.Min(objectMax, pictureMax) - Mathf.Max(objectMin, pictureMin));
            float whole = Mathf.Min(objectMax - objectMin, pictureMax - pictureMin);
            return whole > 0f ? Mathf.Clamp01(overlap / whole) : 0f;
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
