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

        /// <summary>
        /// Usual depths, front to back, of the classes met so far, from everyday sizes and not measured on
        /// the test room's objects. A class that is not listed is taken to be as deep as its box is wide.
        /// </summary>
        public List<ClassDepth> usualDepths = new List<ClassDepth>
        {
            new ClassDepth { className = "laptop", depth = 0.24f },
            new ClassDepth { className = "keyboard", depth = 0.14f },
            new ClassDepth { className = "mouse", depth = 0.11f },
            new ClassDepth { className = "tv", depth = 0.20f },
            new ClassDepth { className = "cup", depth = 0.08f },
            new ClassDepth { className = "bottle", depth = 0.07f },
            new ClassDepth { className = "vase", depth = 0.15f },
            new ClassDepth { className = "potted plant", depth = 0.30f },
            new ClassDepth { className = "chair", depth = 0.50f },
            new ClassDepth { className = "clock", depth = 0.05f }
        };
    }
}
