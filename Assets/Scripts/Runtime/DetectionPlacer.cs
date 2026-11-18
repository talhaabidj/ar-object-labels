using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Turns a found box into a place and a size in the room: a ray from the camera's stored pose through one
    /// point of the box, to the nearest found surface. The depth comes from the class's usual depth.
    /// </summary>
    public class DetectionPlacer : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] ARRaycastManager raycastManager;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

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
            if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon)
                || hits[0].distance > rules.maxDistance)
            {
                return false;
            }

            Vector3 point = hits[0].pose.position;
            float ahead = Vector3.Dot(point - frame.Position, frame.Rotation * Vector3.forward);
            float width = frame.MetresFor(detection.Width, ahead);
            float height = frame.MetresFor(detection.Height, ahead);
            // A class with no usual depth is taken to be as deep as it is wide, and its height stays as seen.
            float depth = width;
            if (ObjectDepth.TryUsual(detection.ClassName, rules.usualDepths, out float usualDepth))
            {
                depth = usualDepth;
                if (used != BoxPointKind.Centre)
                {
                    height = ObjectDepth.Height(height, depth, DegreesLookedDown(frame.RayThrough(detection.Centre)));
                }
            }

            Vector3 away = point - frame.Position;
            if (used == BoxPointKind.BottomCentreSetBack)
            {
                point = BoxPoint.SetBack(point, frame.Position, depth);
            }
            placed = new PlacedDetection(detection, point, width, height, depth, away, used);
            return true;
        }

        static float DegreesLookedDown(Ray ray)
        {
            return Mathf.Asin(Mathf.Clamp(-ray.direction.y, -1f, 1f)) * Mathf.Rad2Deg;
        }
    }
}
