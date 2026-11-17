namespace ArObjectLabels.Core
{
    /// <summary>What one press on the screen should do.</summary>
    public enum PressOutcome
    {
        Ignore,
        PlaceCube,
        ShowWallHint,
        SelectCube,
        Deselect
    }

    /// <summary>
    /// Decides what a press does from plain facts, so the rule can be tested without a scene.
    /// </summary>
    public static class PlacementRule
    {
        public static PressOutcome Decide(
            bool pointerOverUi, bool cubeExists, bool pressIsOnCube, bool hitSurface, bool surfaceFacesUp)
        {
            if (pointerOverUi)
            {
                return PressOutcome.Ignore;
            }

            if (cubeExists)
            {
                return pressIsOnCube ? PressOutcome.SelectCube : PressOutcome.Deselect;
            }

            if (!hitSurface)
            {
                return PressOutcome.Ignore;
            }

            return surfaceFacesUp ? PressOutcome.PlaceCube : PressOutcome.ShowWallHint;
        }
    }
}
