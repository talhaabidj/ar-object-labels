using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>One found surface that a ray meets.</summary>
    public readonly struct SurfaceHit
    {
        public readonly Vector3 Point;
        public readonly float Distance;
        public readonly bool FacesUp;
        public readonly bool Upright;
        public readonly float LongestSide;

        public SurfaceHit(Vector3 point, float distance, bool facesUp, bool upright, float longestSide)
        {
            Point = point;
            Distance = distance;
            FacesUp = facesUp;
            Upright = upright;
            LongestSide = longestSide;
        }
    }

    /// <summary>Which of the surfaces a ray meets an object is taken to stand on.</summary>
    public static class SurfaceChoice
    {
        /// <summary>
        /// The place in the list, nearest first, of the surface to use. That is the nearest one, with one
        /// exception: a low ray often meets the edge of a table before its top, and the edge is found as a
        /// small upright surface. When the nearest surface is small and upright and an upward-facing one
        /// lies just behind it at about the same height, the upward-facing one is used. A wall is not
        /// small, so an object on a wall stays on its wall.
        /// </summary>
        public static int Pick(IReadOnlyList<SurfaceHit> nearestFirst, DetectionSettings rules)
        {
            SurfaceHit first = nearestFirst[0];
            if (!first.Upright || first.LongestSide > rules.smallSurface)
            {
                return 0;
            }

            for (int i = 1; i < nearestFirst.Count; i++)
            {
                SurfaceHit later = nearestFirst[i];
                if (later.Distance - first.Distance > rules.justBehind)
                {
                    break;
                }
                if (later.FacesUp && Mathf.Abs(later.Point.y - first.Point.y) <= rules.sameHeight)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// The set-back moves a point from an object's front foot to under its middle. That only makes sense
        /// for an object standing on something. An object whose ray ends on an upright surface, such as a
        /// clock on a wall, is not set back: it would be pushed into the wall.
        /// </summary>
        public static bool AllowsSetBack(SurfaceHit chosen)
        {
            return !chosen.Upright;
        }

        /// <summary>
        /// For an object that stands on the floor: true when the surface its ray ends on faces up and lies at
        /// least <paramref name="raised"/> above another upward-facing surface the same ray meets. The chosen
        /// surface is then another object's top, such as a table in front of a chair, and
        /// <paramref name="floorHeight"/> is the height of the lowest surface below it.
        /// </summary>
        public static bool TryFloorUnder(IReadOnlyList<SurfaceHit> nearestFirst, int chosen, float raised, out float floorHeight)
        {
            floorHeight = 0f;
            SurfaceHit top = nearestFirst[chosen];
            if (!top.FacesUp)
            {
                return false;
            }
            bool found = false;
            foreach (SurfaceHit hit in nearestFirst)
            {
                if (hit.FacesUp && hit.Point.y <= top.Point.y - raised && (!found || hit.Point.y < floorHeight))
                {
                    floorHeight = hit.Point.y;
                    found = true;
                }
            }
            return found;
        }
    }
}
