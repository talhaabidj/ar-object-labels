using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>
    /// Places one cube on a surface that faces up. A drag moves it; a click picks it up and a second click puts it down.
    /// </summary>
    public class CubePlacer : MonoBehaviour
    {
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] Camera viewCamera;
        [SerializeField] SelectableCube cubePrefab;
        [SerializeField] AppSettings settings;

        readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        InputAction press;
        InputAction point;
        float wallHintUntil;
        float noSurfaceHintUntil;
        bool dragging;
        Vector2 pressPoint;
        float farthestMove;

        public SelectableCube Cube { get; private set; }
        public bool CubeExists => Cube != null;
        public bool CubeSelected { get; private set; }
        public bool CubeCarried { get; private set; }
        public bool WallWasClicked => Time.time < wallHintUntil;
        public bool EmptySpotWasClicked => Time.time < noSurfaceHintUntil;

        void Awake()
        {
            press = new InputAction("Press", InputActionType.Button, "<Pointer>/press");
            point = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
        }

        void OnEnable()
        {
            press.Enable();
            point.Enable();
        }

        void OnDisable()
        {
            press.Disable();
            point.Disable();
        }

        // Actions made in code hold memory until they are disposed.
        void OnDestroy()
        {
            press.Dispose();
            point.Dispose();
        }

        void Update()
        {
            Vector2 screenPoint = point.ReadValue<Vector2>();
            bool held = press.IsPressed();
            if (press.WasPressedThisFrame())
            {
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                HandlePress(screenPoint, overUi);
            }
            else if (held)
            {
                HandleDrag(screenPoint);
            }

            // Checked apart from the press, because a quick tap can go down and up within one frame.
            if (!held && dragging)
            {
                HandleRelease();
            }
            else if (!held && CubeCarried)
            {
                MoveCubeTo(screenPoint);
            }
        }

        /// <summary>Acts on one press at a screen point.</summary>
        public PressOutcome HandlePress(Vector2 screenPoint, bool pointerOverUi)
        {
            bool onCube = CubeExists && Cube.IsUnder(viewCamera.ScreenPointToRay(screenPoint), settings.interaction.maxPressDistance);
            bool hitSurface = TryHitSurface(screenPoint, out Vector3 hitPoint, out bool facesUp);
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi, CubeExists, CubeCarried, onCube, hitSurface, facesUp);

            switch (outcome)
            {
                case PressOutcome.PlaceCube:
                    Cube = Instantiate(cubePrefab, hitPoint, Quaternion.identity);
                    SetSelected(false);
                    break;
                case PressOutcome.ShowWallHint:
                    wallHintUntil = Time.time + settings.interaction.wallHintSeconds;
                    noSurfaceHintUntil = 0f;
                    break;
                case PressOutcome.ShowNoSurfaceHint:
                    noSurfaceHintUntil = Time.time + settings.interaction.noSurfaceHintSeconds;
                    wallHintUntil = 0f;
                    break;
                case PressOutcome.SelectCube:
                    SetSelected(true);
                    dragging = true;
                    pressPoint = screenPoint;
                    farthestMove = 0f;
                    break;
                case PressOutcome.Deselect:
                    SetSelected(false);
                    break;
                case PressOutcome.PutDownCube:
                    Cube.transform.position = hitPoint;
                    SetSelected(false);
                    break;
            }

            return outcome;
        }

        /// <summary>Moves the selected cube while the press is held.</summary>
        public void HandleDrag(Vector2 screenPoint)
        {
            if (!dragging || !CubeExists)
            {
                return;
            }

            farthestMove = Mathf.Max(farthestMove, Vector2.Distance(pressPoint, screenPoint));
            MoveCubeTo(screenPoint);
        }

        /// <summary>
        /// Ends a press that began on the cube. If the pointer hardly moved, it was a click and the cube is picked up.
        /// </summary>
        public void HandleRelease()
        {
            bool pressBeganOnCube = dragging;
            dragging = false;
            if (pressBeganOnCube && CubeExists)
            {
                CubeCarried = ClickRule.IsClick(farthestMove, viewCamera.pixelHeight, settings.interaction.clickMoveShareOfHeight);
            }
        }

        /// <summary>Removes the cube. Ends any drag or carry first.</summary>
        public void ResetCube()
        {
            dragging = false;
            CubeSelected = false;
            CubeCarried = false;
            if (CubeExists)
            {
                Destroy(Cube.gameObject);
                Cube = null;
            }
        }

        void SetSelected(bool selected)
        {
            CubeSelected = selected;
            CubeCarried = false;
            dragging = false;
            Cube.SetSelected(selected);
        }

        void MoveCubeTo(Vector2 screenPoint)
        {
            bool hitSurface = TryHitSurface(screenPoint, out Vector3 hitPoint, out bool facesUp);
            Cube.transform.position = DragRule.NextPosition(Cube.transform.position, hitSurface, facesUp, hitPoint);
        }

        bool TryHitSurface(Vector2 screenPoint, out Vector3 hitPoint, out bool facesUp)
        {
            hitPoint = default;
            facesUp = false;
            if (!raycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon))
            {
                return false;
            }

            ARRaycastHit nearest = hits[0];
            ARPlane plane = planeManager.GetPlane(nearest.trackableId);
            hitPoint = nearest.pose.position;
            facesUp = plane != null && plane.alignment == PlaneAlignment.HorizontalUp;
            return true;
        }
    }
}
