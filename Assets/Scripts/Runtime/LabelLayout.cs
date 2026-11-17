using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Places all labels of the scene together, once a frame: each at its own place first, then the nearest
    /// ones are chosen, and a label that would sit on another label or inside another object is moved aside.
    /// </summary>
    public class LabelLayout : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] Camera viewCamera;

        readonly List<WorldLabel> labels = new List<WorldLabel>();
        readonly List<WorldLabel> inView = new List<WorldLabel>();
        readonly List<float> distances = new List<float>();
        readonly List<float> scores = new List<float>();
        readonly List<Rect> placedLabels = new List<Rect>();
        readonly List<Rect> otherObjects = new List<Rect>();

        public void Add(WorldLabel label)
        {
            if (!labels.Contains(label))
            {
                labels.Add(label);
            }
        }

        public void Remove(WorldLabel label)
        {
            labels.Remove(label);
        }

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        void LateUpdate()
        {
            if (viewCamera != null)
            {
                Arrange(viewCamera.transform.position, viewCamera.transform.rotation);
            }
        }

        /// <summary>Places every label for a camera at this place, looking this way.</summary>
        public void Arrange(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            Quaternion toPicture = Quaternion.Inverse(cameraRotation);
            LabelSettings rules = settings.label;

            // 1. Every label at its own place. Labels behind the camera or faded out are not shown.
            inView.Clear();
            distances.Clear();
            scores.Clear();
            foreach (WorldLabel label in labels)
            {
                label.UpdatePose(cameraPosition, cameraRotation);
                float depth = (toPicture * (label.ObjectTop - cameraPosition)).z;
                if (depth <= 0f || label.Opacity <= 0f)
                {
                    label.SetShown(false);
                    continue;
                }
                inView.Add(label);
                distances.Add(Vector3.Distance(cameraPosition, label.ObjectTop));
                scores.Add(label.Score);
            }

            // 2. At most a set number is shown: nearest first, then the higher score.
            List<int> chosen = LabelChoice.Pick(distances, scores, rules.maxLabels, rules.sameDistanceStep);
            for (int i = 0; i < inView.Count; i++)
            {
                inView[i].SetShown(chosen.Contains(i));
            }

            // 3. From the nearest on, each label moves aside if another object or an earlier label is in its place.
            placedLabels.Clear();
            foreach (int index in chosen)
            {
                WorldLabel label = inView[index];
                float depth = (toPicture * (label.transform.position - cameraPosition)).z;
                if (depth <= 0f)
                {
                    continue;
                }
                Rect own = LabelInPicture(label, cameraPosition, toPicture);
                Rect ownObject = InPicture(label.ObjectBounds, cameraPosition, toPicture);
                otherObjects.Clear();
                foreach (WorldLabel other in inView)
                {
                    if (other == label)
                    {
                        continue;
                    }
                    Rect otherObject = InPicture(other.ObjectBounds, cameraPosition, toPicture);
                    if (!LabelArrange.HoldsTheObject(otherObject, ownObject))
                    {
                        otherObjects.Add(otherObject);
                    }
                }
                Vector2 move = LabelArrange.Offset(own, ownObject, otherObjects, placedLabels, rules.labelMargin);
                if (move != Vector2.zero)
                {
                    label.MoveAside(cameraRotation * new Vector3(move.x * depth, move.y * depth, 0f), cameraPosition);
                    own = new Rect(own.x + move.x, own.y + move.y, own.width, own.height);
                }
                placedLabels.Add(own);
            }
        }

        // A label as a rectangle in the viewer's flat picture, one metre in front of the camera.
        static Rect LabelInPicture(WorldLabel label, Vector3 cameraPosition, Quaternion toPicture)
        {
            Vector3 seen = toPicture * (label.transform.position - cameraPosition);
            Vector2 half = label.HalfSize / seen.z;
            return new Rect(seen.x / seen.z - half.x, seen.y / seen.z - half.y, half.x * 2f, half.y * 2f);
        }

        // An object's box as a rectangle in the same picture. Corners behind the camera are left out.
        static Rect InPicture(Bounds bounds, Vector3 cameraPosition, Quaternion toPicture)
        {
            var low = new Vector2(float.MaxValue, float.MaxValue);
            var high = new Vector2(float.MinValue, float.MinValue);
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                Vector3 seen = toPicture * (bounds.center + Vector3.Scale(bounds.extents, sign) - cameraPosition);
                if (seen.z <= 0.01f)
                {
                    continue;
                }
                var point = new Vector2(seen.x / seen.z, seen.y / seen.z);
                low = Vector2.Min(low, point);
                high = Vector2.Max(high, point);
            }
            return high.x < low.x ? new Rect(0f, 0f, 0f, 0f) : Rect.MinMaxRect(low.x, low.y, high.x, high.y);
        }
    }
}
