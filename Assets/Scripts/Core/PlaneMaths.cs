using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>A ray against a flat surface, and a point against an outline.</summary>
    public static class PlaneMaths
    {
        const float AlmostParallel = 0.000001f;

        /// <summary>
        /// Where a ray meets an endless flat surface given by a point on it and its direction. False when the
        /// ray runs along the surface or the surface lies behind the ray's start.
        /// </summary>
        public static bool RayMeetsPlane(Ray ray, Vector3 planePoint, Vector3 planeNormal, out float distance)
        {
            distance = 0f;
            float towards = Vector3.Dot(ray.direction, planeNormal);
            if (Mathf.Abs(towards) < AlmostParallel)
            {
                return false;
            }
            distance = Vector3.Dot(planePoint - ray.origin, planeNormal) / towards;
            return distance >= 0f;
        }

        /// <summary>
        /// True when a flat point lies inside an outline given by its corners in order. A point on the edge
        /// may count either way.
        /// </summary>
        public static bool IsInside(Vector2 point, IReadOnlyList<Vector2> outline)
        {
            bool inside = false;
            for (int i = 0, previous = outline.Count - 1; i < outline.Count; previous = i++)
            {
                Vector2 a = outline[i];
                Vector2 b = outline[previous];
                bool straddles = (a.y > point.y) != (b.y > point.y);
                if (straddles && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
