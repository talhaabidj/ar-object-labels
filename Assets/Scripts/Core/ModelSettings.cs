using System;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers used to run the detection model. They live in the app's settings file.</summary>
    [Serializable]
    public class ModelSettings
    {
        /// <summary>The side of the square picture the model takes, in pixels.</summary>
        public int inputSize = 640;

        /// <summary>A found object with a lower score than this gives no label.</summary>
        public float minScore = 0.45f;

        /// <summary>
        /// Two boxes of one class that share more than this part of their joint area count as the same
        /// object; only the one with the higher score is kept.
        /// </summary>
        public float overlapLimit = 0.45f;

        /// <summary>How long the model may work in one frame, in milliseconds. One run is spread over frames.</summary>
        public float millisecondsPerFrame = 5f;

        /// <summary>Run the model on the graphics card. Off: on the processor.</summary>
        public bool onGraphicsCard = true;
    }
}
