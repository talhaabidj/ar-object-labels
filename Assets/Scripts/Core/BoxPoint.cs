using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Which point of a box stands for the object.</summary>
    public enum BoxPointKind
    {
        Centre,
        BottomCentre,
        BottomCentreSetBack
    }

    /// <summary>Picks the point of a box that the ray into the room goes through.</summary>
    public static class BoxPoint
    {
        public static Vector2 Pick(Detection detection, BoxPointKind kind)
        {
            return kind == BoxPointKind.Centre ? detection.Centre : detection.BottomCentre;
        }

        /// <summary>
        /// The box point to use for this box. A box that touches the picture's bottom edge is cut off there,
        /// and setting it back then overshoots, so it is given the plain bottom centre.
        /// </summary>
        public static BoxPointKind KindFor(Detection detection, BoxPointKind wanted, int pictureHeight, float edgeMargin)
        {
            bool cutOff = wanted == BoxPointKind.BottomCentreSetBack && TouchesBottomEdge(detection, pictureHeight, edgeMargin);
            return cutOff ? BoxPointKind.BottomCentre : wanted;
        }

        /// <summary>
        /// True when the box's lower edge is at the picture's bottom edge, or within the margin of it. The
        /// margin is a share of the picture's height.
        /// </summary>
        public static bool TouchesBottomEdge(Detection detection, int pictureHeight, float edgeMargin)
        {
            return detection.Bottom >= pictureHeight * (1f - edgeMargin);
        }

        /// <summary>
        /// A ray through the lower edge of a box meets the surface at the object's front foot. This moves that
        /// point to under the object's middle: half the object's depth further from the camera, at the same height.
        /// </summary>
        public static Vector3 SetBack(Vector3 frontFoot, Vector3 cameraPosition, float depth)
        {
            Vector3 away = frontFoot - cameraPosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.000001f)
            {
                return frontFoot;
            }
            return frontFoot + away.normalized * (depth / 2f);
        }

        /// <summary>
        /// How high the box's top reaches above a foot on the floor: where the ray through the top of the box
        /// passes over the foot, measured up from the foot. Zero when the ray never gets that far.
        /// </summary>
        public static float TopAbove(Ray topRay, Vector3 foot)
        {
            var across = new Vector2(foot.x - topRay.origin.x, foot.z - topRay.origin.z);
            var step = new Vector2(topRay.direction.x, topRay.direction.z);
            if (step.sqrMagnitude < 0.000001f)
            {
                return 0f;
            }
            float along = across.magnitude / step.magnitude;
            return Mathf.Max(0f, topRay.origin.y + topRay.direction.y * along - foot.y);
        }
    }
}
