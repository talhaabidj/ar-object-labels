using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Turns a found box into a place and a size in the room: a ray from the camera's stored pose through one
    /// point of the box, to the found surface the object stands on.
    /// </summary>
    public class DetectionPlacer : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] ARRaycastManager raycastManager;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
        readonly List<SurfaceHit> surfaces = new List<SurfaceHit>();

        /// <summary>
        /// False when the ray meets no found surface within the distance set in the settings. A box that touches
        /// the picture's bottom edge, or an object on an upright surface, is not set back; the result says which
        /// box point was used.
        /// </summary>
        public bool TryPlace(Detection detection, CameraFrame frame, out PlacedDetection placed)
        {
            DetectionSettings rules = settings.detection;
            placed = default;
            BoxPointKind used = BoxPoint.KindFor(detection, rules.boxPoint, frame.PictureHeight, rules.edgeMargin);
            if (!TryFindSurface(frame.RayThrough(BoxPoint.Pick(detection, used)), rules, out Vector3 point, out SurfaceHit surface))
            {
                return false;
            }
            if (used == BoxPointKind.BottomCentreSetBack && !SurfaceChoice.AllowsSetBack(surface))
            {
                used = BoxPointKind.BottomCentre;
            }
            placed = Measure(detection, frame, point, used, rules);
            return true;
        }

        // The point on the found surface the object stands on: the surface the choice rule picks among those the
        // ray meets, no further than the furthest distance.
        bool TryFindSurface(Ray ray, DetectionSettings rules, out Vector3 point, out SurfaceHit surface)
        {
            point = default;
            surface = default;
            if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                return false;
            }
            int chosen = SurfaceChoice.Pick(AsSurfaces(hits), rules);
            if (hits[chosen].distance > rules.maxDistance)
            {
                return false;
            }
            point = hits[chosen].pose.position;
            surface = surfaces[chosen];
            return true;
        }

        // The object's room box from its picture box and the point where it stands.
        static PlacedDetection Measure(Detection detection, CameraFrame frame, Vector3 point, BoxPointKind used,
            DetectionSettings rules)
        {
            float ahead = Vector3.Dot(point - frame.Position, frame.Rotation * Vector3.forward);
            float width = frame.MetresFor(detection.Width, ahead);
            float height = frame.MetresFor(detection.Height, ahead);
            // The usual depth of a class shapes the room box and corrects the height. A class with no usual
            // depth is taken to be as deep as it is wide, and its height stays as seen.
            float depth = width;
            if (ObjectDepth.TryUsual(detection.ClassName, rules.usualDepths, out ClassDepth usual))
            {
                depth = usual.depth;
                if (used != BoxPointKind.Centre)
                {
                    height = ObjectDepth.Height(height, depth, DegreesLookedDown(frame.RayThrough(detection.Centre)));
                }
            }

            Vector3 away = point - frame.Position;
            if (used == BoxPointKind.BottomCentreSetBack)
            {
                // Set back by the box width: measured on the test room, that put objects nearer their true
                // place than the usual depths did. A plant is the exception; its box is far wider than its pot.
                point = BoxPoint.SetBack(point, frame.Position, ObjectDepth.ForSetBack(width, usual));
            }
            return new PlacedDetection(detection, point, width, height, depth, away, used);
        }

        // What the rule for choosing a surface needs to know about each surface the ray meets, nearest first.
        List<SurfaceHit> AsSurfaces(List<ARRaycastHit> met)
        {
            surfaces.Clear();
            foreach (ARRaycastHit one in met)
            {
                var plane = one.trackable as ARPlane;
                bool facesUp = plane != null && plane.alignment == PlaneAlignment.HorizontalUp;
                bool upright = plane != null && plane.alignment == PlaneAlignment.Vertical;
                float longestSide = plane != null ? Mathf.Max(plane.size.x, plane.size.y) : 0f;
                surfaces.Add(new SurfaceHit(one.pose.position, one.distance, facesUp, upright, longestSide));
            }
            return surfaces;
        }

        static float DegreesLookedDown(Ray ray)
        {
            return Mathf.Asin(Mathf.Clamp(-ray.direction.y, -1f, 1f)) * Mathf.Rad2Deg;
        }
    }
}
