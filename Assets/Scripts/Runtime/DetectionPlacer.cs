using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Turns a found box into a place in the room: a ray from the camera's stored pose through one point of the
    /// box, to the nearest found surface.
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
            if (used == BoxPointKind.BottomCentreSetBack)
            {
                // The object's depth is not known. Its width is the nearest thing to it that the box gives.
                point = BoxPoint.SetBack(point, frame.Position, width);
            }
            placed = new PlacedDetection(detection, point, width, frame.MetresFor(detection.Height, ahead), used);
            return true;
        }
    }
}
