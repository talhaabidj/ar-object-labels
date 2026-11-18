using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArObjectLabels
{
    /// <summary>
    /// Marks an object of the simulated room that the true-box source may "find", with the class name a
    /// detector would give it.
    /// </summary>
    public class DetectableObject : MonoBehaviour
    {
        [SerializeField] string className = "object";

        Renderer[] parts;

        public string ClassName => className;

        /// <summary>
        /// Fills the list with the marked objects of every loaded scene. It walks the scenes itself, because
        /// Unity's own search leaves out objects flagged "don't save", and the simulator sets that flag on the
        /// room's meshes.
        /// </summary>
        public static void FindAll(List<DetectableObject> into)
        {
            into.Clear();
            var roots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                scene.GetRootGameObjects(roots);
                foreach (GameObject root in roots)
                {
                    into.AddRange(root.GetComponentsInChildren<DetectableObject>());
                }
            }
        }

        /// <summary>Adds the corners of the boxes around this object's visible parts, in its own scene's coordinates.</summary>
        public void AddCorners(List<Vector3> into)
        {
            if (parts == null)
            {
                parts = GetComponentsInChildren<Renderer>();
            }

            foreach (Renderer part in parts)
            {
                Bounds box = part.localBounds;
                Matrix4x4 toScene = part.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    var side = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                    into.Add(toScene.MultiplyPoint3x4(box.center + Vector3.Scale(box.extents, side)));
                }
            }
        }
    }
}
