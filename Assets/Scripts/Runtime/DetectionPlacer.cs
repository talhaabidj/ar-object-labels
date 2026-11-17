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
            DetectionSettings rules = settings.detection;
            Ray ray = frame.RayThrough(BoxPoint.Pick(detection, rules.boxPoint));
            placed = default;
            if (!raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon) || hits[0].distance > rules.maxDistance)
            {
                return false;
            }

            Vector3 point = hits[0].pose.position;
            float ahead = Vector3.Dot(point - frame.Position, frame.Rotation * Vector3.forward);
            placed = new PlacedDetection(detection, point,
                frame.MetresFor(detection.Width, ahead), frame.MetresFor(detection.Height, ahead), rules.boxPoint);
            return true;
        }
    }
}
