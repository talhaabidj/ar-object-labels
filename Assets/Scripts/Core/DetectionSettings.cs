using System;

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
    }
}
