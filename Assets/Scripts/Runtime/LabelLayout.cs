using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Places all labels of the scene together, once a frame: each at its own place first, then the labels of
    /// the objects that are biggest in the picture are chosen, and a label that would sit on another label or
    /// inside another object is moved aside.
    /// </summary>
    public class LabelLayout : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] Camera viewCamera;

        readonly List<WorldLabel> labels = new List<WorldLabel>();
        readonly List<WorldLabel> inView = new List<WorldLabel>();
        readonly List<float> sizes = new List<float>();
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
                // How far the picture reaches from its middle, one metre in front of the camera.
                Matrix4x4 lens = viewCamera.projectionMatrix;
                var halfPicture = new Vector2(1f / lens.m00, 1f / lens.m11);
                Arrange(viewCamera.transform.position, viewCamera.transform.rotation, halfPicture);
            }
        }

        /// <summary>
        /// Places every label for a camera at this place, looking this way. <paramref name="halfPicture"/>
        /// is half the picture's width and height, one metre in front of the camera.
        /// </summary>
        public void Arrange(Vector3 cameraPosition, Quaternion cameraRotation, Vector2 halfPicture)
        {
            Quaternion toPicture = Quaternion.Inverse(cameraRotation);
            LabelSettings rules = settings.label;
            Rect picture = Rect.MinMaxRect(-halfPicture.x + rules.labelMargin, -halfPicture.y + rules.labelMargin,
                halfPicture.x - rules.labelMargin, halfPicture.y - rules.labelMargin);

            // 1. Every label at its own place. Labels behind the camera or faded out are not shown.
            inView.Clear();
            sizes.Clear();
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
                Rect inPicture = BoxInPicture.Around(label.ObjectBounds, label.ObjectTurn, cameraPosition, cameraRotation);
                sizes.Add(inPicture.width * inPicture.height);
                scores.Add(label.Score);
            }

            // 2. At most a set number is shown: the biggest object in the picture first, then the higher score.
            List<int> chosen = LabelChoice.Pick(sizes, scores, rules.maxLabels, rules.sameSizeShare);
            for (int i = 0; i < inView.Count; i++)
            {
                inView[i].SetShown(chosen.Contains(i));
            }

            // 3. In that order, each label moves if another object or an earlier label is in its place, or if
            //    the picture's edge would cut it.
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
                Rect ownObject = BoxInPicture.Around(label.ObjectBounds, label.ObjectTurn, cameraPosition, cameraRotation);
                otherObjects.Clear();
                foreach (WorldLabel other in inView)
                {
                    if (other == label)
                    {
                        continue;
                    }
                    Rect otherObject = BoxInPicture.Around(other.ObjectBounds, other.ObjectTurn, cameraPosition, cameraRotation);
                    if (!LabelArrange.HoldsTheObject(otherObject, ownObject))
                    {
                        otherObjects.Add(otherObject);
                    }
                }
                Vector2 move = LabelArrange.Offset(own, ownObject, otherObjects, placedLabels, rules.labelMargin, picture);
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
    }
}
