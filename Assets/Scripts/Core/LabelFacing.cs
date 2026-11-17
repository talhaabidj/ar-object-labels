using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which way a label turns.</summary>
    public static class LabelFacing
    {
        const float TooSmall = 0.0001f;

        /// <summary>
        /// The label lies parallel to the viewer's screen: it faces the way the camera looks, not the camera's
        /// place, so it does not look slanted near the edge of a wide picture. The top of its text points to the
        /// top of the picture, so the text is never mirrored or upside down, also seen from straight above.
        /// </summary>
        public static Quaternion Rotation(Vector3 cameraForward, Vector3 cameraUp)
        {
            if (cameraForward.sqrMagnitude < TooSmall * TooSmall)
            {
                return Quaternion.identity;
            }
            Vector3 up = cameraUp;
            if (Vector3.Cross(cameraForward, up).sqrMagnitude < TooSmall)
            {
                // The viewer's "up" lies along the line of sight; any upright direction will do.
                up = Mathf.Abs(cameraForward.normalized.y) < 0.99f ? Vector3.up : Vector3.forward;
            }
            return Quaternion.LookRotation(cameraForward, up);
        }
    }
}
