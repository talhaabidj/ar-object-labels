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
        /// was chosen in between. Its last result is then out of date.
        /// </summary>
        public static bool WasLeftAlone(int lastAskedFrame, int thisFrame)
        {
            return thisFrame - lastAskedFrame > 1;
        }

        /// <summary>
        /// What to do with a readback from the graphics card. Unity keeps a finished readback's data for one
        /// frame only and then disposes of it, which shows as an error; reading it then would throw. That can
        /// happen while the app is paused or out of focus, when no frame of the app runs at all.
        /// </summary>
        public static ReadbackStep NextStep(bool done, bool hasError, bool resultIsOld)
        {
            if (hasError)
            {
                return ReadbackStep.Drop;
            }
            if (!done)
            {
                return ReadbackStep.Wait;
            }
            return resultIsOld ? ReadbackStep.Drop : ReadbackStep.Use;
        }
    }

    /// <summary>What to do with a readback from the graphics card.</summary>
    public enum ReadbackStep
    {
        /// <summary>Not finished yet; ask again next frame.</summary>
        Wait,

        /// <summary>Gone, failed or out of date; do not read it.</summary>
        Drop,

        /// <summary>Finished and readable now.</summary>
        Use,
    }
}
