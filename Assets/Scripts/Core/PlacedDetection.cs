using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>A found object with its place in the room and its size there.</summary>
    public readonly struct PlacedDetection
    {
        public readonly Detection Detection;
        public readonly Vector3 Point;
        public readonly float Width;
        public readonly float Height;
        public readonly float Depth;
        public readonly Vector3 Away;
        public readonly BoxPointKind Kind;

        /// <summary>
        /// <paramref name="away"/> is the level direction from the camera to the object. The object's width
        /// lies across it and its depth along it.
        /// </summary>
        public PlacedDetection(Detection detection, Vector3 point, float width, float height, float depth,
            Vector3 away, BoxPointKind kind)
        {
            Detection = detection;
            Point = point;
            Width = width;
            Height = height;
            Depth = depth;
            away.y = 0f;
            Away = away.sqrMagnitude > 0.000001f ? away.normalized : Vector3.forward;
            Kind = kind;
        }

        /// <summary>
        /// The room box a label is given, in its own axes: x across the view it was found from, z along it.
        /// <see cref="Turn"/> turns those axes into the room. A point found through the box's middle is taken
        /// as the object's middle. A point found through the box's lower edge is under the object, so the
        /// box stands on it.
        /// </summary>
        public Bounds RoomBox
        {
            get
            {
                Vector3 middle = Kind == BoxPointKind.Centre ? Point : Point + Vector3.up * (Height / 2f);
                return new Bounds(middle, new Vector3(Width, Height, Depth));
            }
        }

        /// <summary>The turn of the room box: its z axis points away from the camera that found the object.</summary>
        public Quaternion Turn => Quaternion.LookRotation(Away, Vector3.up);
    }
}
