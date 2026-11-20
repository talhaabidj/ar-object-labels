namespace ArObjectLabels.Core
{
    /// <summary>The one line of help the screen can show.</summary>
    public enum Hint
    {
        Starting,
        ArNotRunning,
        TrackingLost,
        UseFlatSurface,
        NoSurfaceThere,
        FindSurface,
        PlaceCube,
        SelectCube,
        DragCube,
        CarryCube
    }

    /// <summary>The facts the hint depends on, gathered once per frame.</summary>
    public struct HintFacts
    {
        public bool SessionReady;
        public bool Tracking;
        public float SecondsSinceStart;
        public bool WallWasClicked;
        public bool EmptySpotWasClicked;
        public bool AnySurfaceFound;
        public bool CubeExists;
        public bool CubeSelected;
        public bool CubeCarried;
    }

    /// <summary>Picks the hint. The first rule that applies wins.</summary>
    public static class HintRule
    {

        /// <summary>
        /// For <paramref name="secondsBeforeNotRunning"/> after start, a session that is not running yet counts as
        /// starting rather than broken.
        /// </summary>
        public static Hint Pick(HintFacts facts, float secondsBeforeNotRunning)
        {
            bool justStarted = facts.SecondsSinceStart < secondsBeforeNotRunning;
            if (!facts.SessionReady) return justStarted ? Hint.Starting : Hint.ArNotRunning;
            if (!facts.Tracking) return justStarted ? Hint.Starting : Hint.TrackingLost;
            if (facts.WallWasClicked) return Hint.UseFlatSurface;
            if (facts.EmptySpotWasClicked) return Hint.NoSurfaceThere;
            if (!facts.CubeExists) return facts.AnySurfaceFound ? Hint.PlaceCube : Hint.FindSurface;
            if (facts.CubeCarried) return Hint.CarryCube;
            return facts.CubeSelected ? Hint.DragCube : Hint.SelectCube;
        }
    }
}
