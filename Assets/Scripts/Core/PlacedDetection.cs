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
        public readonly BoxPointKind Kind;

        public PlacedDetection(Detection detection, Vector3 point, float width, float height, BoxPointKind kind)
        {
            Detection = detection;
            Point = point;
            Width = width;
            Height = height;
            Kind = kind;
        }

        /// <summary>
        /// The room box a label is given. A point found through the box's lower edge is the object's foot, so
        /// the box stands on it. A point found through the box's middle is taken as the object's middle.
        /// </summary>
        public Bounds RoomBox
        {
            get
            {
                Vector3 middle = Kind == BoxPointKind.BottomCentre ? Point + Vector3.up * (Height / 2f) : Point;
                return new Bounds(middle, new Vector3(Width, Height, Width));
            }
        }
    }
}
