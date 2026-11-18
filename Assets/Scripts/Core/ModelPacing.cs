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
    }
}
