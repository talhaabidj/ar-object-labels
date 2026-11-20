using System;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers for the cube, the hint line and the surface outlines. They live in the app's settings file.</summary>
    [Serializable]
    public class InteractionSettings
    {
        /// <summary>How long the hint after a click on a wall stays, in seconds.</summary>
        public float wallHintSeconds = 3f;

        /// <summary>How long the hint after a click where no surface is found stays, in seconds.</summary>
        public float noSurfaceHintSeconds = 2f;

        /// <summary>A press on the cube further than this from the camera, in metres, does not count.</summary>
        public float maxPressDistance = 20f;

        /// <summary>A press counts as a click when the pointer moved less than this share of the screen height.</summary>
        public float clickMoveShareOfHeight = 0.025f;

        /// <summary>How strongly found surfaces are drawn while the cube is not moved or carried, from 0 to 1.</summary>
        public float faintOutlineShare = 0.15f;

        /// <summary>The controls fold away once the camera has gone this far, in metres, or turned this much, in degrees.</summary>
        public float firstMoveMetres = 0.1f;
        public float firstMoveDegrees = 5f;

        /// <summary>For this many seconds after start, a session that is not running yet counts as starting.</summary>
        public float secondsBeforeNotRunning = 3f;
    }
}
