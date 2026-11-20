using ArObjectLabels.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

namespace ArObjectLabels
{
    /// <summary>
    /// Shows one line of help that fits what the app is doing right now, in a bar only as wide as its text.
    /// The list of controls folds down to one short line once the camera has first moved; H opens and folds it.
    /// </summary>
    public class SessionHint : MonoBehaviour
    {
        [SerializeField] TMP_Text hintText;
        [SerializeField] RectTransform hintPanel;
        [SerializeField] TMP_Text controlsText;
        [SerializeField] RectTransform controlsPanel;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] CubePlacer cubePlacer;
        [SerializeField] Camera viewCamera;
        [Tooltip("Space between a panel's edge and its text, in canvas units.")]
        [SerializeField] Vector2 panelPadding = new Vector2(16f, 8f);
        [Tooltip("Gap between the hint bar and the screen's sides, in canvas units.")]
        [SerializeField] float screenGap = 16f;
        [Tooltip("The controls fold away once the camera has gone this far, in metres, or turned this much, in degrees.")]
        [SerializeField] float firstMoveMetres = 0.1f;
        [SerializeField] float firstMoveDegrees = 5f;

        InputAction controlsKey;
        bool hasStartPose;
        Vector3 startPosition;
        Quaternion startRotation;
        bool cameraHasMoved;
        bool controlsOpen = true;
        string shownHint;

        void Awake()
        {
            controlsKey = new InputAction("Controls", InputActionType.Button, "<Keyboard>/h");
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        void OnEnable()
        {
            controlsKey.Enable();
        }

        void OnDisable()
        {
            controlsKey.Disable();
        }

        // An action made in code holds memory until it is disposed.
        void OnDestroy()
        {
            controlsKey.Dispose();
        }

        void Start()
        {
            ShowControls();
        }

        void Update()
        {
            var facts = new HintFacts
            {
                SessionReady = ARSession.state >= ARSessionState.SessionInitializing,
                Tracking = ARSession.state == ARSessionState.SessionTracking,
                SecondsSinceStart = Time.timeSinceLevelLoad,
                WallWasClicked = cubePlacer.WallWasClicked,
                EmptySpotWasClicked = cubePlacer.EmptySpotWasClicked,
                AnySurfaceFound = planeManager.trackables.count > 0,
                CubeExists = cubePlacer.CubeExists,
                CubeSelected = cubePlacer.CubeSelected,
                CubeCarried = cubePlacer.CubeCarried
            };

            string hint = Messages.For(HintRule.Pick(facts));
            if (hint != shownHint)
            {
                shownHint = hint;
                hintText.text = hint;
                hintPanel.gameObject.SetActive(hint.Length > 0);
                FitToText(hintPanel, hintText);
            }

            FoldControlsAfterFirstMove(facts.Tracking);
            if (controlsKey.WasPressedThisFrame())
            {
                controlsOpen = !controlsOpen;
                ShowControls();
            }
        }

        // The start pose is taken once tracking runs, so the camera settling at the start does not count.
        void FoldControlsAfterFirstMove(bool tracking)
        {
            if (cameraHasMoved || !tracking || viewCamera == null)
            {
                return;
            }
            Transform view = viewCamera.transform;
            if (!hasStartPose)
            {
                startPosition = view.position;
                startRotation = view.rotation;
                hasStartPose = true;
            }
            else if (CameraStill.HasLeft(startPosition, startRotation, view.position, view.rotation, firstMoveMetres, firstMoveDegrees))
            {
                cameraHasMoved = true;
                controlsOpen = false;
                ShowControls();
            }
        }

        void ShowControls()
        {
            controlsText.text = controlsOpen ? Messages.Controls : Messages.ControlsFolded;
            FitToText(controlsPanel, controlsText);
        }

        // Makes the panel just big enough for its text, never wider than the screen allows.
        void FitToText(RectTransform panel, TMP_Text text)
        {
            var canvas = (RectTransform)panel.parent;
            float widest = canvas.rect.width - 2f * screenGap - 2f * panelPadding.x;
            Vector2 needed = text.GetPreferredValues(text.text, widest, 0f);
            panel.sizeDelta = new Vector2(Mathf.Min(needed.x, widest) + 2f * panelPadding.x, needed.y + 2f * panelPadding.y);
        }
    }
}
