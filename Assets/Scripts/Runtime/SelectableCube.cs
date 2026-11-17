using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>The cube's own parts: the collider a press is tested against, the outline shown when selected, and its label.</summary>
    public class SelectableCube : MonoBehaviour
    {
        [SerializeField] Collider pressCollider;
        [SerializeField] GameObject outline;
        [SerializeField] Renderer body;
        [SerializeField] WorldLabel labelPrefab;
        [SerializeField] string labelText = "Cube";

        WorldLabel label;

        void Start()
        {
            // The label is not a child of the cube, so it keeps its own size and turns freely.
            label = Instantiate(labelPrefab);
            label.SetText(labelText);
            label.Follow(body);
        }

        void OnDestroy()
        {
            if (label != null)
            {
                Destroy(label.gameObject);
            }
        }

        public bool IsUnder(Ray ray, float maxDistance)
        {
            return pressCollider.Raycast(ray, out _, maxDistance);
        }

        public void SetSelected(bool selected)
        {
            outline.SetActive(selected);
        }
    }
}
