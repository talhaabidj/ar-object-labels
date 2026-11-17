using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Where the cube goes while it is being dragged.</summary>
    public static class DragRule
    {
        /// <summary>
        /// The cube follows the pointer onto a surface that faces up. Anywhere else it stays at its last good place.
        /// </summary>
        public static Vector3 NextPosition(Vector3 lastGoodPosition, bool hitSurface, bool surfaceFacesUp, Vector3 hitPoint)
        {
            return hitSurface && surfaceFacesUp ? hitPoint : lastGoodPosition;
        }
    }
}
