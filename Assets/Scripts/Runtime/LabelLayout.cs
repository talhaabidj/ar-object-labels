using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Places all labels of the scene together, once a frame: each at its own place first, then the labels of
    /// the objects that are biggest in the picture are chosen, and a label that would sit on another label or
    /// inside another object is moved aside. Labels are kept steady: a shown label is not easily replaced, a
    /// moved label keeps its place for a while, and a label glides to a new place. While the camera moves,
    /// the shown labels keep their places, sides and order; a label is moved then only if it would leave the
    /// picture or sit on another label. The whole layout is worked out again once the camera is still.
    /// </summary>
    public class LabelLayout : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] Camera viewCamera;
        [Tooltip("The app's panels and buttons on the screen. Labels are kept out from under them.")]
        [SerializeField] RectTransform[] keepClearOf;

        readonly List<WorldLabel> labels = new List<WorldLabel>();
        readonly List<WorldLabel> inView = new List<WorldLabel>();
        readonly List<float> sizes = new List<float>();
        readonly List<float> scores = new List<float>();
        readonly List<Rect> placedLabels = new List<Rect>();
        readonly List<Rect> otherObjects = new List<Rect>();
        readonly List<Rect> noObjects = new List<Rect>();
        readonly List<bool> wasShown = new List<bool>();
        readonly List<int> ranks = new List<int>();
        readonly List<Rect> panels = new List<Rect>();
        readonly Vector3[] corners = new Vector3[4];
        readonly CameraStill cameraStill = new CameraStill();

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
                Transform view = viewCamera.transform;
                LabelSettings rules = settings.label;
                cameraStill.Take(view.position, view.rotation, Time.deltaTime, rules.stillMoveSpeed, rules.stillTurnSpeed);
                bool settled = cameraStill.SecondsStill >= rules.stillSeconds;
                FindPanels(halfPicture);
                Arrange(view.position, view.rotation, halfPicture, Time.deltaTime, settled);
            }
        }

        // The panels shown now, as rectangles in the flat picture. A screen-space panel's corners are in pixels.
        void FindPanels(Vector2 halfPicture)
        {
            panels.Clear();
            if (keepClearOf == null)
            {
                return;
            }
            var screenSize = new Vector2(viewCamera.pixelWidth, viewCamera.pixelHeight);
            foreach (RectTransform panel in keepClearOf)
            {
                if (panel == null || !panel.gameObject.activeInHierarchy)
                {
                    continue;
                }
                panel.GetWorldCorners(corners);
                Rect onScreen = Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
                panels.Add(PanelEdges.ToPicture(onScreen, screenSize, halfPicture));
            }
        }

        /// <summary>
        /// Places every label for a camera at this place, looking this way. <paramref name="halfPicture"/>
        /// is half the picture's width and height, one metre in front of the camera;
        /// <paramref name="secondsSinceLast"/> is the time since this was last done. With
        /// <paramref name="cameraSettled"/> false the camera is moving, and the shown labels keep their layout.
        /// </summary>
        public void Arrange(Vector3 cameraPosition, Quaternion cameraRotation, Vector2 halfPicture, float secondsSinceLast,
            bool cameraSettled)
        {
            Quaternion toPicture = Quaternion.Inverse(cameraRotation);
            LabelSettings rules = settings.label;
            Rect picture = Rect.MinMaxRect(-halfPicture.x + rules.labelMargin, -halfPicture.y + rules.labelMargin,
                halfPicture.x - rules.labelMargin, halfPicture.y - rules.labelMargin);

            // 1. Every label at its own place. Labels behind the camera or faded out are not shown.
            inView.Clear();
            sizes.Clear();
            scores.Clear();
            wasShown.Clear();
            ranks.Clear();
            foreach (WorldLabel label in labels)
            {
                label.UpdatePose(cameraPosition, cameraRotation);
                float depth = (toPicture * (label.ObjectTop - cameraPosition)).z;
                if (depth <= 0f || label.Opacity <= 0f)
                {
                    label.SetShown(false);
                    label.Memory.Forget();
                    continue;
                }
                Rect inPicture = BoxInPicture.Around(label.ObjectBounds, label.ObjectTurn, cameraPosition, cameraRotation);
                // A label kept for an object out of the picture waits at its place and takes no place among the shown.
                float labelDepth = (toPicture * (label.transform.position - cameraPosition)).z;
                Rect ownLabel = labelDepth > 0f ? LabelInPicture(label, cameraPosition, toPicture) : Rect.zero;
                if (!LabelArrange.TakesAPlace(inPicture, ownLabel, label.IsFound, picture))
                {
                    label.SetShown(false);
                    label.Memory.Forget();
                    continue;
                }
                inView.Add(label);
                sizes.Add(LabelSteady.SizeForChoice(inPicture.width * inPicture.height, label.Memory.WasShown, rules.stayBonus));
                scores.Add(label.Score);
                wasShown.Add(label.Memory.WasShown);
                ranks.Add(label.Memory.Rank);
            }

            // 2. At most a set number is shown: the biggest object in the picture first, then the higher score.
            //    A label that was shown counts as bigger, so the choice does not flip between two alike. While
            //    the camera moves, the shown labels stay in their order and new ones only fill free places.
            List<int> chosen = cameraSettled
                ? LabelChoice.Pick(sizes, scores, rules.maxLabels, rules.sameSizeShare)
                : LabelChoice.Keep(sizes, scores, wasShown, ranks, rules.maxLabels, rules.sameSizeShare);
            for (int i = 0; i < inView.Count; i++)
            {
                int rank = chosen.IndexOf(i);
                bool shown = rank >= 0;
                inView[i].SetShown(shown);
                if (shown)
                {
                    inView[i].Memory.WasShown = true;
                    inView[i].Memory.Rank = rank;
                }
                else
                {
                    inView[i].Memory.Forget();
                }
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
                Vector2 needed = LabelArrange.Offset(own, ownObject, otherObjects, placedLabels, rules.labelMargin, picture, panels,
                    label.IsFound);
                placedLabels.Add(Settle(label, own, needed, depth, cameraPosition, cameraRotation, rules, picture,
                    secondsSinceLast, cameraSettled));
            }
        }

        // Decides where the label stays this frame, draws it on its way there, and gives back the place it
        // settles on, which the labels after it must keep clear of. While the camera moves, only leaving the
        // picture or sitting on an earlier label makes a label move; other objects passing behind it do not.
        Rect Settle(WorldLabel label, Rect own, Vector2 needed, float depth, Vector3 cameraPosition, Quaternion cameraRotation,
            LabelSettings rules, Rect picture, float secondsSinceLast, bool cameraSettled)
        {
            LabelMemory memory = label.Memory;
            var held = new Rect(own.x + memory.HeldMove.x, own.y + memory.HeldMove.y, own.width, own.height);
            List<Rect> objectsInTheWay = cameraSettled ? otherObjects : noObjects;
            bool heldPlaceIsFree = LabelArrange.CanStay(held, objectsInTheWay, placedLabels, rules.labelMargin, picture, panels);
            Vector2 move = LabelSteady.Move(memory, heldPlaceIsFree, needed, secondsSinceLast, rules.holdSeconds,
                keepWhileFree: !cameraSettled && memory.HasBeenDrawn);
            memory.HeldMove = move;
            memory.DrawnMove = memory.HasBeenDrawn ? LabelSteady.Glide(memory.DrawnMove, move, secondsSinceLast, rules.glideSeconds) : move;
            memory.HasBeenDrawn = true;

            if (memory.DrawnMove != Vector2.zero)
            {
                Vector2 drawn = memory.DrawnMove;
                label.MoveAside(cameraRotation * new Vector3(drawn.x * depth, drawn.y * depth, 0f), cameraPosition);
            }
            return new Rect(own.x + move.x, own.y + move.y, own.width, own.height);
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
