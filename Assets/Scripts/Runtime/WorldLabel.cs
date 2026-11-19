using ArObjectLabels.Core;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// A label in the room. It stays a little above its object, always faces the camera, keeps its text the
    /// same height on the screen at every distance, fades out far away and keeps out of walls. When several labels share a scene, the
    /// <see cref="LabelLayout"/> also keeps them off each other.
    /// </summary>
    public class WorldLabel : MonoBehaviour
    {
        [SerializeField] AppSettings settings;
        [SerializeField] TMP_Text text;
        [SerializeField] RectTransform plate;
        [SerializeField] Vector2 platePadding = new Vector2(14f, 8f);
        [SerializeField] CanvasGroup fader;
        [SerializeField] RectTransform leader;
        [SerializeField] Camera viewCamera;
        [SerializeField] ARPlaneManager planeManager;

        Renderer followed;
        Bounds? placedBox;
        Quaternion placedTurn = Quaternion.identity;
        Vector3 objectTop;
        float objectRadius;
        LabelLayout layout;
        string shownText;

        /// <summary>Half the plate's width and height in metres, at the size the label is drawn now.</summary>
        public Vector2 HalfSize => plate.rect.size * 0.5f * plate.lossyScale.x;

        /// <summary>The top of the object this label names.</summary>
        public Vector3 ObjectTop => objectTop;

        /// <summary>The space the label's object takes up, as a box in its own axes. <see cref="ObjectTurn"/> turns them into the room.</summary>
        public Bounds ObjectBounds => followed != null
            ? followed.bounds
            : placedBox ?? new Bounds(objectTop + Vector3.down * objectRadius, Vector3.one * objectRadius * 2f);

        /// <summary>The turn of the object's box. A followed object's box is not turned.</summary>
        public Quaternion ObjectTurn => followed == null && placedBox.HasValue ? placedTurn : Quaternion.identity;

        /// <summary>How sure the source of this label is, from 0 to 1. Used when there are more labels than are shown.</summary>
        public float Score { get; set; } = 1f;

        /// <summary>
        /// What the label remembers from frame to frame. Labels that are handed from object to object are
        /// given the memory that belongs to their object.
        /// </summary>
        public LabelMemory Memory { get; set; } = new LabelMemory();

        /// <summary>How clearly the label is drawn at its distance, from 0 to 1.</summary>
        public float Opacity { get; private set; } = 1f;

        public void SetText(string labelText)
        {
            if (labelText == shownText)
            {
                return;
            }
            shownText = labelText;
            text.text = labelText;
            text.ForceMeshUpdate();
            plate.sizeDelta = text.GetPreferredValues(labelText) + platePadding * 2f;
        }

        /// <summary>The label follows this object and sits above its top.</summary>
        public void Follow(Renderer target)
        {
            followed = target;
            placedBox = null;
        }

        /// <summary>
        /// The label sits above this box, for an object known as a place and a size in the room. The box is
        /// given in its own axes, z along the view it was found from, with the turn of those axes.
        /// </summary>
        public void PlaceAt(Bounds box, Quaternion turn)
        {
            // Seen from above, the label clears the object's far edge, which is half its depth away.
            PlaceAt(new Vector3(box.center.x, box.max.y, box.center.z), box.extents.z);
            placedBox = box;
            placedTurn = turn;
        }

        /// <summary>The label sits above this point, for an object known only as a place.</summary>
        public void PlaceAt(Vector3 top, float radius)
        {
            followed = null;
            placedBox = null;
            objectTop = top;
            objectRadius = radius;
        }

        void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
            if (planeManager == null)
            {
                planeManager = FindFirstObjectByType<ARPlaneManager>();
            }
        }

        void OnEnable()
        {
            layout = FindFirstObjectByType<LabelLayout>();
            if (layout != null)
            {
                layout.Add(this);
            }
        }

        void OnDisable()
        {
            if (layout != null)
            {
                layout.Remove(this);
            }
        }

        void LateUpdate()
        {
            // With a layout in the scene, the layout places every label. Alone, the label places itself.
            if (layout == null && viewCamera != null)
            {
                UpdatePose(viewCamera.transform.position, viewCamera.transform.rotation);
            }
        }

        /// <summary>Puts the label where its own rules say, for a camera at this place, turned this way.</summary>
        public void UpdatePose(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            Vector3 cameraUp = cameraRotation * Vector3.up;
            if (followed != null)
            {
                Bounds bounds = followed.bounds;
                objectTop = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                objectRadius = Mathf.Max(bounds.extents.x, bounds.extents.z);
            }
            LabelSettings rules = settings.label;
            float distance = Vector3.Distance(cameraPosition, objectTop);
            // Depth, not distance: a label at the side of the picture would otherwise be drawn larger.
            float depth = LabelSizing.Depth(objectTop, cameraPosition, cameraRotation * Vector3.forward);
            float scale = LabelSizing.Scale(depth, rules.nearLimit);
            scale = LabelSizing.CapToWidth(scale, plate.rect.width * plate.parent.localScale.x, rules.maxWidth);
            transform.localScale = Vector3.one * scale;

            Vector3 centre = LabelPlacement.Centre(objectTop, objectRadius, HalfSize.y, rules.gap * scale, cameraPosition, cameraUp);
            Quaternion rotation = LabelFacing.Rotation(cameraRotation * Vector3.forward, cameraUp);
            centre = KeepOutOfWalls(centre, rotation, cameraPosition, rules.wallMargin);
            transform.SetPositionAndRotation(centre, rotation);

            Opacity = LabelFade.Opacity(distance, rules.fadeStart, rules.fadeEnd);
            fader.alpha = Opacity;
            leader.gameObject.SetActive(false);
        }

        /// <summary>Shows or hides the whole label.</summary>
        public void SetShown(bool shown)
        {
            fader.gameObject.SetActive(shown);
        }

        /// <summary>
        /// Moves the label from its own place by this much and draws a thin line from it to its object, so it is
        /// still clear what it names.
        /// </summary>
        public void MoveAside(Vector3 offset, Vector3 cameraPosition)
        {
            transform.position += offset;
            DrawLeader(cameraPosition);
        }

        // The line runs in the label's own flat plane, from the plate's nearest edge to where the camera sees the
        // object's top through that plane.
        void DrawLeader(Vector3 cameraPosition)
        {
            Vector3 eye = transform.InverseTransformPoint(cameraPosition);
            Vector3 top = transform.InverseTransformPoint(objectTop);
            float depthBetween = eye.z - top.z;
            Vector3 toObject = Mathf.Abs(depthBetween) > 0.0001f ? eye + (top - eye) * (eye.z / depthBetween) : top;
            Vector2 half = plate.rect.size * 0.5f * plate.localScale.x * plate.parent.localScale.x;
            var start = new Vector2(Mathf.Clamp(toObject.x, -half.x, half.x), Mathf.Clamp(toObject.y, -half.y, half.y));
            var end = new Vector2(toObject.x, toObject.y);
            Vector2 along = end - start;
            if (along.magnitude < 0.0001f)
            {
                leader.gameObject.SetActive(false);
                return;
            }

            float canvasScale = leader.parent.localScale.x;
            leader.gameObject.SetActive(true);
            leader.localPosition = (Vector3)((start + end) * 0.5f) / canvasScale;
            leader.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg);
            leader.sizeDelta = new Vector2(along.magnitude, settings.label.leaderThickness) / canvasScale;
        }

        Vector3 KeepOutOfWalls(Vector3 centre, Quaternion rotation, Vector3 cameraPosition, float margin)
        {
            if (planeManager == null)
            {
                return centre;
            }
            foreach (ARPlane wall in planeManager.trackables)
            {
                if (wall.alignment == PlaneAlignment.Vertical && IsBesideWall(wall, centre))
                {
                    bool cameraInFront = Vector3.Dot(cameraPosition - wall.center, wall.normal) >= 0f;
                    Vector3 intoTheRoom = cameraInFront ? wall.normal : -wall.normal;
                    centre = KeepOutOfWall(centre, rotation, wall.center, intoTheRoom, margin);
                }
            }
            return centre;
        }

        // A found wall is a patch, not an endless plane: only a label next to the patch itself is moved.
        bool IsBesideWall(ARPlane wall, Vector3 centre)
        {
            Vector3 onWall = wall.transform.InverseTransformPoint(centre);
            Vector2 fromMiddle = new Vector2(onWall.x, onWall.z) - wall.centerInPlaneSpace;
            Vector2 reach = wall.extents + Vector2.one * HalfSize.x;
            return Mathf.Abs(fromMiddle.x) <= reach.x && Mathf.Abs(fromMiddle.y) <= reach.y;
        }

        /// <summary>Moves the label out of one wall, given as a point on it and its direction into the room.</summary>
        public Vector3 KeepOutOfWall(Vector3 centre, Quaternion rotation, Vector3 wallPoint, Vector3 wallNormal, float margin)
        {
            return LabelPlacement.KeepOutOfWall(centre, rotation * Vector3.right, rotation * Vector3.up,
                HalfSize.x, HalfSize.y, wallPoint, wallNormal, margin);
        }
    }
}
