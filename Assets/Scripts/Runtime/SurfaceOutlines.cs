using ArObjectLabels.Core;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ArObjectLabels
{
    /// <summary>
    /// Draws the found surfaces at full strength only while the cube is being moved or carried, and faint the
    /// rest of the time. Surfaces found later get the same strength.
    /// </summary>
    public class SurfaceOutlines : MonoBehaviour
    {
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] CubePlacer cubePlacer;
        [Tooltip("How strongly the surfaces are drawn while the cube is not being moved or carried. 1 is full strength.")]
        [SerializeField] AppSettings settings;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        MaterialPropertyBlock block;
        float shownStrength = -1f;

        void Awake()
        {
            block = new MaterialPropertyBlock();
        }

        void OnEnable()
        {
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        }

        void OnDisable()
        {
            planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }

        void LateUpdate()
        {
            bool cubeInHand = cubePlacer.CubeSelected || cubePlacer.CubeCarried;
            float strength = OutlineRule.Strength(cubeInHand, settings.interaction.faintOutlineShare);
            if (strength == shownStrength)
            {
                return;
            }
            shownStrength = strength;
            foreach (ARPlane plane in planeManager.trackables)
            {
                TintPlane(plane, strength);
            }
        }

        void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
        {
            float strength = shownStrength < 0f ? OutlineRule.Strength(false, settings.interaction.faintOutlineShare) : shownStrength;
            foreach (ARPlane plane in changes.added)
            {
                TintPlane(plane, strength);
            }
        }

        void TintPlane(ARPlane plane, float strength)
        {
            foreach (Renderer renderer in plane.GetComponents<Renderer>())
            {
                Tint(renderer, strength, block);
            }
        }

        /// <summary>
        /// Draws one renderer at a share of its material's own opacity. The shared material is left as it is.
        /// </summary>
        public static void Tint(Renderer renderer, float strength, MaterialPropertyBlock block)
        {
            if (renderer.sharedMaterial == null || !renderer.sharedMaterial.HasProperty(BaseColor))
            {
                return;
            }
            Color color = renderer.sharedMaterial.GetColor(BaseColor);
            color.a *= strength;
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColor, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
