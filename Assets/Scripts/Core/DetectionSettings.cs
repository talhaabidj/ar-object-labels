using System;
using System.Collections.Generic;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers used to turn a found box into a place in the room. They live in the app's settings file.</summary>
    [Serializable]
    public class DetectionSettings
    {
        /// <summary>Which point of a box is sent into the room.</summary>
        public BoxPointKind boxPoint = BoxPointKind.BottomCentreSetBack;

        /// <summary>A surface further away than this, in metres, gives no label.</summary>
        public float maxDistance = 5f;

        /// <summary>The true-box source gives no box when less than this share of it lies inside the picture.</summary>
        public float minVisibleShare = 0.5f;

        /// <summary>
        /// A box whose lower edge is this near the picture's bottom edge counts as touching it and is not set
        /// back. A share of the picture's height.
        /// </summary>
        public float edgeMargin = 0.01f;

        /// <summary>An upright surface whose longer side is at most this, in metres, counts as small: an edge of furniture, not a wall.</summary>
        public float smallSurface = 1.5f;

        /// <summary>How far behind a small upright surface, in metres along the ray, an upward-facing one may lie to be used instead.</summary>
        public float justBehind = 0.3f;

        /// <summary>How far apart in height, in metres, the two meeting points may be for that.</summary>
        public float sameHeight = 0.1f;

        /// <summary>
        /// Usual depths, front to back, from everyday sizes and not measured on the test room's objects.
        /// They give the room box its depth and correct the height seen from above. The set-back uses the
        /// box width, except for a class marked as limiting it: a plant's leaves make its box much wider
        /// than its pot. A class that is not listed is taken to be as deep as its box is wide, which is
        /// right for round things such as a vase, a cup or a bottle.
        /// </summary>
        public List<ClassDepth> usualDepths = new List<ClassDepth>
        {
            new ClassDepth { className = "laptop", depth = 0.24f },
            new ClassDepth { className = "keyboard", depth = 0.14f },
            new ClassDepth { className = "mouse", depth = 0.11f },
            new ClassDepth { className = "tv", depth = 0.20f },
            new ClassDepth { className = "chair", depth = 0.50f },
            new ClassDepth { className = "clock", depth = 0.05f },
            new ClassDepth { className = "potted plant", depth = 0.30f, limitsSetBack = true }
        };
    }
}
