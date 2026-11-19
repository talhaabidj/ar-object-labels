using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Turns a found box into a place and a size in the room: a ray from the camera's stored pose through one
    /// point of the box, to the found surface the object stands on. For a class that stands on the floor, a ray
    /// that ends on another object's top uses the floor under that point.
    /// </summary>
    public class DetectionPlacer : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] ARRaycastManager raycastManager;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();
        readonly List<SurfaceHit> surfaces = new List<SurfaceHit>();

        /// <summary>False when the ray meets no found surface within the distance set in the settings.</summary>
        public bool TryPlace(Detection detection, CameraFrame frame, out PlacedDetection placed)
        {
            return TryPlace(detection, frame, settings.detection.boxPoint, out placed);
        }

        /// <summary>
        /// The same, with the box point given instead of read from the settings. A box that touches the
        /// picture's bottom edge is not set back; the result says which box point was used.
        /// </summary>
        public bool TryPlace(Detection detection, CameraFrame frame, BoxPointKind boxPoint, out PlacedDetection placed)
        {
            DetectionSettings rules = settings.detection;
            BoxPointKind used = BoxPoint.KindFor(detection, boxPoint, frame.PictureHeight, rules.edgeMargin);
            Ray ray = frame.RayThrough(BoxPoint.Pick(detection, used));
            placed = default;
            if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                return false;
            }
            int chosen = SurfaceChoice.Pick(AsSurfaces(hits), rules);
            ARRaycastHit hit = hits[chosen];
            if (hit.distance > rules.maxDistance)
            {
                return false;
            }
            if (used == BoxPointKind.BottomCentreSetBack && !SurfaceChoice.AllowsSetBack(surfaces[chosen]))
            {
                used = BoxPointKind.BottomCentre;
            }

            Vector3 point = hit.pose.position;
            float ahead = Vector3.Dot(point - frame.Position, frame.Rotation * Vector3.forward);
            float width = frame.MetresFor(detection.Width, ahead);
            float height = frame.MetresFor(detection.Height, ahead);
            // The usual depth of a class shapes the room box and corrects the height. A class with no usual
            // depth is taken to be as deep as it is wide, and its height stays as seen.
            float depth = width;
            bool hasUsual = ObjectDepth.TryUsual(detection.ClassName, rules.usualDepths, out ClassDepth usual);
            if (hasUsual)
            {
                depth = usual.depth;
            }
            if (hasUsual && usual.standsOnFloor && used != BoxPointKind.Centre
                && SurfaceChoice.TryFloorUnder(surfaces, chosen, rules.raisedSurface, out float floorHeight))
            {
                // The box ends on another object's top, which hides the object's foot: it stands on the floor
                // under that edge, and reaches up to the box's top.
                point.y = floorHeight;
                height = BoxPoint.TopAbove(frame.RayThrough(new Vector2(detection.Centre.x, detection.Top)), point);
                used = BoxPointKind.BottomCentre;
            }
            else if (hasUsual && used != BoxPointKind.Centre)
            {
                height = ObjectDepth.Height(height, depth, DegreesLookedDown(frame.RayThrough(detection.Centre)));
            }

            Vector3 away = point - frame.Position;
            if (used == BoxPointKind.BottomCentreSetBack)
            {
                // Set back by the box width: measured on the test room, that put objects nearer their true
                // place than the usual depths did. A plant is the exception; its box is far wider than its pot.
                point = BoxPoint.SetBack(point, frame.Position, ObjectDepth.ForSetBack(width, usual));
            }
            placed = new PlacedDetection(detection, point, width, height, depth, away, used);
            return true;
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
