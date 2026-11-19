namespace ArObjectLabels.Core
{
    /// <summary>What one press on the screen should do.</summary>
    public enum PressOutcome
    {
        Ignore,
        PlaceCube,
        ShowWallHint,
        ShowNoSurfaceHint,
        SelectCube,
        Deselect,
        PutDownCube
    }

    /// <summary>
    /// Decides what a press does from plain facts, so the rule can be tested without a scene.
    /// </summary>
    public static class PlacementRule
    {
        public static PressOutcome Decide(
            bool pointerOverUi, bool cubeExists, bool cubeIsCarried, bool pressIsOnCube, bool hitSurface,
            bool surfaceFacesUp)
        {
            if (pointerOverUi)
            {
                return PressOutcome.Ignore;
            }

            // A carried cube sits under the pointer, so this press is always on it. The surface decides.
            if (cubeIsCarried)
            {
                return OnSurface(hitSurface, surfaceFacesUp, PressOutcome.PutDownCube);
            }

            if (cubeExists)
            {
                return pressIsOnCube ? PressOutcome.SelectCube : PressOutcome.Deselect;
            }

            return OnSurface(hitSurface, surfaceFacesUp, PressOutcome.PlaceCube);
        }

        static PressOutcome OnSurface(bool hitSurface, bool surfaceFacesUp, PressOutcome onUpwardSurface)
        {
            // In a fresh session much of the table and floor is not found yet. Such a click says so.
            if (!hitSurface)
            {
                return PressOutcome.ShowNoSurfaceHint;
            }

            return surfaceFacesUp ? onUpwardSurface : PressOutcome.ShowWallHint;
        }
    }
}
