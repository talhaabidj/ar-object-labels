using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ArObjectLabels
{
    /// <summary>Shows the Reset button only while a cube exists, and removes the cube on a click or the R key.</summary>
    public class ResetButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] CubePlacer cubePlacer;

        InputAction resetKey;

        void Awake()
        {
            resetKey = new InputAction("Reset", InputActionType.Button, "<Keyboard>/r");
            button.onClick.AddListener(cubePlacer.ResetCube);
        }

        void OnEnable()
        {
            resetKey.Enable();
        }

        void OnDisable()
        {
            resetKey.Disable();
        }

        void Update()
        {
            button.gameObject.SetActive(cubePlacer.CubeExists);
            if (cubePlacer.CubeExists && resetKey.WasPressedThisFrame())
            {
                cubePlacer.ResetCube();
            }
        }
    }
}
