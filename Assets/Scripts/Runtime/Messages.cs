using ArObjectLabels.Core;

namespace ArObjectLabels
{
    /// <summary>Every sentence the app shows, in one place.</summary>
    public static class Messages
    {
        public const string Controls =
            "Right mouse + move: look · W A S D: move · Q E: down, up · Shift: faster\n" +
            "R Reset";

        public static string For(Hint hint)
        {
            switch (hint)
            {
                case Hint.Starting:
                    return "Starting AR…";
                case Hint.ArNotRunning:
                    return "AR is not running. In Unity, open Edit → Project Settings → " +
                           "XR Plug-in Management and tick XR Simulation.";
                case Hint.TrackingLost:
                    return "Tracking lost. Move slowly and point at a surface with some detail.";
                case Hint.UseFlatSurface:
                    return "Place it on a flat surface, such as the table or the floor.";
                case Hint.FindSurface:
                    return "Hold the right mouse button and move the mouse to look around. W A S D to move.\n" +
                           "Find the table or the floor.";
                case Hint.PlaceCube:
                    return "Click the table or the floor to place the cube.";
                case Hint.SelectCube:
                    return "Click the cube to select it.";
                case Hint.DragCube:
                    return "Drag the cube to move it. Click anywhere else to let go of it.";
                default:
                    return string.Empty;
            }
        }
    }
}
