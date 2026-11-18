using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// A stand-in detector that is never wrong: it works out the picture box of every marked object of the
    /// simulated room from where the object really is. It does not check whether something stands in front
    /// of the object. On a phone there is no simulated room, so it finds nothing.
    /// </summary>
    public class TrueBoxSource : MonoBehaviour, IDetectionSource
    {
        const float SecondsBetweenSearches = 1f;

        [SerializeField] Camera viewCamera;
        [SerializeField] AppSettings settings;

        readonly List<Vector3> corners = new List<Vector3>();
        readonly List<DetectableObject> objects = new List<DetectableObject>();
        float nextSearch;

        public string Name => Messages.TrueBoxSource;

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        public bool TryRead(List<Detection> into, out CameraFrame frame)
        {
            into.Clear();
            if (viewCamera == null)
            {
                frame = default;
                return false;
            }

            frame = CameraFrames.Of(viewCamera);
            FindObjectsIfNeeded();
            foreach (DetectableObject found in objects)
            {
                if (found != null && found.isActiveAndEnabled && TryBoxOf(found, frame, out Detection detection))
                {
                    into.Add(detection);
                }
            }
            return true;
        }

        /// <summary>The picture box of one marked object, seen from the given frame. False when it is not in the picture.</summary>
        public bool TryBoxOf(DetectableObject found, CameraFrame frame, out Detection detection)
        {
            corners.Clear();
            found.AddCorners(corners);
            // The simulated room is a scene of its own. Its coordinates are those of the space the AR camera
            // moves in, and in this scene that space is the camera's parent.
            Transform roomSpace = viewCamera.transform.parent;
            for (int i = 0; roomSpace != null && i < corners.Count; i++)
            {
                corners[i] = roomSpace.TransformPoint(corners[i]);
            }
            return TrueBox.TryMake(found.ClassName, frame, corners, settings.detection.minVisibleShare, out detection);
        }

        // The room is loaded after this scene, and may be loaded again later.
        void FindObjectsIfNeeded()
        {
            bool anyGone = objects.Count == 0;
            foreach (DetectableObject found in objects)
            {
                anyGone |= found == null;
            }

            if (anyGone && Time.time >= nextSearch)
            {
                DetectableObject.FindAll(objects);
                nextSearch = Time.time + SecondsBetweenSearches;
            }
        }
    }
}
