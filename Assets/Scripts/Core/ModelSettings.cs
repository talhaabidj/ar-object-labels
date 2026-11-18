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

        /// <summary>
        /// A new run of the model starts at most every this many frames: 1 is every frame, 3 every third.
        /// A run also waits for the one before it to finish.
        /// </summary>
        public int framesBetweenRuns = 3;
    }
}
