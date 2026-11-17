using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// One found object in a picture: its class, how sure the source is, and its box. The box is in the
    /// picture's own pixels, counted from the top-left corner, x to the right and y down, as detectors give it.
    /// </summary>
    public readonly struct Detection
    {
        public readonly string ClassName;
        public readonly float Score;
        public readonly float Left;
        public readonly float Top;
        public readonly float Right;
        public readonly float Bottom;

        public Detection(string className, float score, float left, float top, float right, float bottom)
        {
            ClassName = className;
            Score = score;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public Vector2 Centre => new Vector2((Left + Right) / 2f, (Top + Bottom) / 2f);
        public Vector2 BottomCentre => new Vector2((Left + Right) / 2f, Bottom);
    }
}
