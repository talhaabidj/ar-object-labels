using System;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers used to turn a found box into a place in the room. They live in the app's settings file.</summary>
    [Serializable]
    public class DetectionSettings
    {
        /// <summary>Which point of a box is sent into the room.</summary>
        public BoxPointKind boxPoint = BoxPointKind.Centre;

        /// <summary>A surface further away than this, in metres, gives no label.</summary>
        public float maxDistance = 5f;
    }
}
