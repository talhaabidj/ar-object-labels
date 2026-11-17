using ArObjectLabels.Core;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ArObjectLabels
{
    /// <summary>Shows one line of help that fits what the app is doing right now.</summary>
    public class SessionHint : MonoBehaviour
    {
        [SerializeField] TMP_Text hintText;
        [SerializeField] TMP_Text controlsText;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] CubePlacer cubePlacer;

        public Hint Current { get; private set; }

        void Start()
        {
            controlsText.text = Messages.Controls;
        }

        void Update()
        {
            var facts = new HintFacts
            {
                SessionReady = ARSession.state >= ARSessionState.SessionInitializing,
                Tracking = ARSession.state == ARSessionState.SessionTracking,
                SecondsSinceStart = Time.timeSinceLevelLoad,
                WallWasClicked = cubePlacer.WallWasClicked,
                AnySurfaceFound = planeManager.trackables.count > 0,
                CubeExists = cubePlacer.CubeExists,
                CubeSelected = cubePlacer.CubeSelected,
                CubeCarried = cubePlacer.CubeCarried
            };

            Current = HintRule.Pick(facts);
            hintText.text = Messages.For(Current);
        }
    }
}
