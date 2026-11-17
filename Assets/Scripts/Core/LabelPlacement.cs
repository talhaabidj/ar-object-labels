using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Where a label sits.</summary>
    public static class LabelPlacement
    {
        /// <summary>
        /// The label's centre. Seen from the side it sits a gap above the object's top. Seen from above it also
        /// moves towards the top of the viewer's picture, so that from every height it stays at least a gap
        /// clear of the object and never covers it.
        /// </summary>
        public static Vector3 Centre(Vector3 objectTop, float objectRadius, float labelHalfHeight, float gap,
            Vector3 cameraPosition, Vector3 cameraUp)
        {
            Vector3 toObject = (objectTop - cameraPosition).normalized;
            float steepness = Mathf.Clamp01(-toObject.y);
            Vector3 aboveTheTop = Vector3.up * (gap + labelHalfHeight);
            Vector3 upThePicture = cameraUp.normalized * (steepness * (objectRadius + labelHalfHeight + gap));
            return objectTop + aboveTheTop + upThePicture;
        }

        /// <summary>
        /// Moves a label out of a wall. The label is a flat rectangle; the wall is a plane through a point with a
        /// direction pointing into the room. A label that is already clear is not moved, and neither is one
        /// that lies wholly behind the wall: that is a hidden object, not a label touching a wall.
        /// </summary>
        public static Vector3 KeepOutOfWall(Vector3 centre, Vector3 labelRight, Vector3 labelUp, float halfWidth,
            float halfHeight, Vector3 wallPoint, Vector3 wallNormal, float margin)
        {
            Vector3 normal = wallNormal.normalized;
            float reach = halfWidth * Mathf.Abs(Vector3.Dot(labelRight.normalized, normal))
                        + halfHeight * Mathf.Abs(Vector3.Dot(labelUp.normalized, normal));
            float distance = Vector3.Dot(centre - wallPoint, normal);
            float missing = reach + margin - distance;
            bool touchesTheWall = missing > 0f && distance > -reach;
            return touchesTheWall ? centre + normal * missing : centre;
        }
    }
}
