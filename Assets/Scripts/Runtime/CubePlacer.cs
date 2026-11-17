using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ArObjectLabels
{
    /// <summary>Places one cube on a surface that faces up, and lets a press select it and a drag move it.</summary>
    public class CubePlacer : MonoBehaviour
    {
        [SerializeField] ARRaycastManager raycastManager;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] Camera viewCamera;
        [SerializeField] SelectableCube cubePrefab;
        [SerializeField] float wallHintSeconds = 3f;
        [SerializeField] float maxPressDistance = 20f;

        static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        InputAction press;
        InputAction point;
        float wallHintUntil;
        bool dragging;

        public SelectableCube Cube { get; private set; }
        public bool CubeExists => Cube != null;
        public bool CubeSelected { get; private set; }
        public bool WallWasClicked => Time.time < wallHintUntil;

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

        void Update()
        {
            Vector2 screenPoint = point.ReadValue<Vector2>();
            if (press.WasPressedThisFrame())
            {
                bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                HandlePress(screenPoint, overUi);
            }
            else if (press.IsPressed())
            {
                HandleDrag(screenPoint);
            }
            else
            {
                dragging = false;
            }
        }

        /// <summary>Acts on one press at a screen point. Public so a test can call it without a mouse.</summary>
        public PressOutcome HandlePress(Vector2 screenPoint, bool pointerOverUi)
        {
            bool onCube = CubeExists && Cube.IsUnder(viewCamera.ScreenPointToRay(screenPoint), maxPressDistance);
            bool hitSurface = TryHitSurface(screenPoint, out Vector3 hitPoint, out bool facesUp);
            PressOutcome outcome = PlacementRule.Decide(pointerOverUi, CubeExists, onCube, hitSurface, facesUp);

            switch (outcome)
            {
                case PressOutcome.PlaceCube:
                    Cube = Instantiate(cubePrefab, hitPoint, Quaternion.identity);
                    SetSelected(false);
                    break;
                case PressOutcome.ShowWallHint:
                    wallHintUntil = Time.time + wallHintSeconds;
                    break;
                case PressOutcome.SelectCube:
                    SetSelected(true);
                    dragging = true;
                    break;
                case PressOutcome.Deselect:
                    SetSelected(false);
                    break;
            }

            return outcome;
        }

        /// <summary>Moves the selected cube while the press is held. Public so a test can call it without a mouse.</summary>
        public void HandleDrag(Vector2 screenPoint)
        {
            if (!dragging || !CubeExists)
            {
                return;
            }

            bool hitSurface = TryHitSurface(screenPoint, out Vector3 hitPoint, out bool facesUp);
            Cube.transform.position = DragRule.NextPosition(Cube.transform.position, hitSurface, facesUp, hitPoint);
        }

        /// <summary>Removes the cube. Ends any drag first.</summary>
        public void ResetCube()
        {
            dragging = false;
            CubeSelected = false;
            if (CubeExists)
            {
                Destroy(Cube.gameObject);
                Cube = null;
            }
        }

        void SetSelected(bool selected)
        {
            CubeSelected = selected;
            dragging = false;
            Cube.SetSelected(selected);
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
