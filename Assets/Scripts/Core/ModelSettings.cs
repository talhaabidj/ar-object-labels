using System;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers used to run the detection model. They live in the app's settings file.</summary>
    [Serializable]
    public class ModelSettings
    {
        /// <summary>The side of the square picture the model takes, in pixels.</summary>
        public int inputSize = 640;

        /// <summary>The score an object needs, in each of several runs in a row, to get a label.</summary>
        public float minScore = 0.40f;

        /// <summary>An object that has a label keeps it while it is found with this score or more.</summary>
        public float stayScore = 0.30f;

        /// <summary>In how many runs in a row an object must be found before its label appears.</summary>
        public int runsToAppear = 3;

        /// <summary>
        /// Two boxes of one class that share more than this part of their joint area count as the same
        /// object; only the one with the higher score is kept.
        /// </summary>
        public float overlapLimit = 0.45f;

        /// <summary>How many runs of the model start in a second at most. A run also waits for the one before it to finish.</summary>
        public float runsPerSecond = 10f;

        /// <summary>
        /// One run is spread over frames: this many of the model's layers are started in a frame. 0 starts
        /// all of them in one frame. Measured in the Editor at about 300 frames a second: with 40, ten runs
        /// a second are still reached and the long frame at the start of each run is gone.
        /// </summary>
        public int layersPerFrame = 40;
    }
}
