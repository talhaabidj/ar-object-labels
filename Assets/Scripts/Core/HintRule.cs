namespace ArObjectLabels.Core
{
    /// <summary>The one line of help the screen can show.</summary>
    public enum Hint
    {
        Starting,
        ArNotRunning,
        TrackingLost,
        UseFlatSurface,
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
        public bool AnySurfaceFound;
        public bool CubeExists;
        public bool CubeSelected;
        public bool CubeCarried;
    }

    /// <summary>Picks the hint. The first rule that applies wins.</summary>
    public static class HintRule
    {
        public const float SecondsBeforeNotRunning = 3f;

        public static Hint Pick(HintFacts facts)
        {
            bool justStarted = facts.SecondsSinceStart < SecondsBeforeNotRunning;
            if (!facts.SessionReady) return justStarted ? Hint.Starting : Hint.ArNotRunning;
            if (!facts.Tracking) return justStarted ? Hint.Starting : Hint.TrackingLost;
            if (facts.WallWasClicked) return Hint.UseFlatSurface;
            if (!facts.CubeExists) return facts.AnySurfaceFound ? Hint.PlaceCube : Hint.FindSurface;
            if (facts.CubeCarried) return Hint.CarryCube;
            return facts.CubeSelected ? Hint.DragCube : Hint.SelectCube;
        }
    }
}
