using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>The cube's own parts: the collider a press is tested against, and the outline shown when selected.</summary>
    public class SelectableCube : MonoBehaviour
    {
        [SerializeField] Collider pressCollider;
        [SerializeField] GameObject outline;

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
