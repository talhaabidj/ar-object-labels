using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which point of a box stands for the object.</summary>
    public enum BoxPointKind
    {
        Centre,
        BottomCentre
    }

    /// <summary>Picks the point of a box that the ray into the room goes through.</summary>
    public static class BoxPoint
    {
        public static Vector2 Pick(Detection detection, BoxPointKind kind)
        {
            return kind == BoxPointKind.BottomCentre ? detection.BottomCentre : detection.Centre;
        }
    }
}
