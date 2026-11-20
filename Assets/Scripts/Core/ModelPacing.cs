namespace ArObjectLabels.Core
{
    /// <summary>When the model runs.</summary>
    public static class ModelPacing
    {
        /// <summary>
        /// True when enough time has passed since the last run started for the wanted runs a second. Times
        /// are in seconds. With no runs a second wanted, a run is never due.
        /// </summary>
        public static bool RunIsDue(float now, float lastRunStarted, float runsPerSecond)
        {
            return runsPerSecond > 0f && now - lastRunStarted >= 1f / runsPerSecond;
        }

        /// <summary>The frame to start from before the model has ever been asked for a result.</summary>
        public const int NeverAsked = -2;

        /// <summary>
        /// True when the model was not asked for a result in the frame before this one, as when another source
        /// was chosen in between. A result read back from the graphics card can be read only in the frame it
        /// arrives in, so one that arrived in a missed frame is gone, and an older result is out of date.
        /// </summary>
        public static bool WasLeftAlone(int lastAskedFrame, int thisFrame)
        {
            return thisFrame - lastAskedFrame > 1;
        }
    }
}
