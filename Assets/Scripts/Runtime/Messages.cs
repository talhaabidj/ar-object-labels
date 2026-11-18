using ArObjectLabels.Core;

namespace ArObjectLabels
{
    /// <summary>Every sentence the app shows, in one place.</summary>
    public static class Messages
    {
        public const string Controls =
            "W A S D: move · Q E: down, up · Shift: faster · Hold right mouse: look\n" +
            "R removes the cube";

        public const string FixedBoxSource = "fixed box";
        public const string TrueBoxSource = "true box";
        public const string NoSource = "off";

        /// <summary>The text on the source switch.</summary>
        public static string ForSource(string sourceName)
        {
            return "Detection: " + sourceName;
        }

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
                    return "Move with W A S D. Hold the right mouse button and move the mouse to look.\n" +
                           "Find the table or the floor.";
                case Hint.PlaceCube:
                    return "Click the table or the floor to place the cube.";
                case Hint.SelectCube:
                    return "Click the cube to pick it up, or drag it to move it.";
                case Hint.DragCube:
                    return "Drag the cube to move it, or click it to pick it up.\n" +
                           "Click anywhere else to let go of it.";
                case Hint.CarryCube:
                    return "Carrying the cube. Click the table or the floor to put it down. R removes it.";
                default:
                    return string.Empty;
            }
        }
    }
}
