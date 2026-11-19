using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Tells how long the camera has been held still, so the labels are laid out again only then.</summary>
    public class CameraStill
    {
        Vector3 lastPosition;
        Quaternion lastRotation;
        bool hasLast;

        /// <summary>
        /// Seconds since the camera last moved or turned faster than the limits. Endless until the first
        /// move, so the labels are laid out at the start.
        /// </summary>
        public float SecondsStill { get; private set; } = float.PositiveInfinity;

        /// <summary>True when the camera has gone more than <paramref name="metres"/> or turned more than <paramref name="degrees"/> from a pose.</summary>
        public static bool HasLeft(Vector3 startPosition, Quaternion startRotation, Vector3 position, Quaternion rotation,
            float metres, float degrees)
        {
            return Vector3.Distance(startPosition, position) > metres || Quaternion.Angle(startRotation, rotation) > degrees;
        }

        /// <summary>
        /// Takes the camera's pose in this frame. Moving slower than <paramref name="maxMoveSpeed"/>, in metres
        /// a second, and turning slower than <paramref name="maxTurnSpeed"/>, in degrees a second, counts as still.
        /// </summary>
        public void Take(Vector3 position, Quaternion rotation, float secondsSinceLast, float maxMoveSpeed, float maxTurnSpeed)
        {
            if (hasLast && secondsSinceLast > 0f)
            {
                float moveSpeed = Vector3.Distance(position, lastPosition) / secondsSinceLast;
                float turnSpeed = Quaternion.Angle(rotation, lastRotation) / secondsSinceLast;
                bool still = moveSpeed <= maxMoveSpeed && turnSpeed <= maxTurnSpeed;
                SecondsStill = still ? SecondsStill + secondsSinceLast : 0f;
            }
            lastPosition = position;
            lastRotation = rotation;
            hasLast = true;
        }
    }
}
