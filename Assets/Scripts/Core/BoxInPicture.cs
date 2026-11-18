using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>How a box in the room shows in the viewer's flat picture.</summary>
    public static class BoxInPicture
    {
        // Corners nearer than this, in metres ahead of the camera, are left out.
        const float NearestSeen = 0.01f;

        /// <summary>
        /// The rectangle around a room box in the flat picture one metre in front of the camera. The box is
        /// given by its middle and size in its own axes, and the turn of those axes. An empty rectangle
        /// when the whole box is behind the camera.
        /// </summary>
        public static Rect Around(Bounds box, Quaternion turn, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            Quaternion toPicture = Quaternion.Inverse(cameraRotation);
            var low = new Vector2(float.MaxValue, float.MaxValue);
            var high = new Vector2(float.MinValue, float.MinValue);
            for (int corner = 0; corner < 8; corner++)
            {
                var side = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                Vector3 seen = toPicture * (box.center + turn * Vector3.Scale(box.extents, side) - cameraPosition);
                if (seen.z <= NearestSeen)
                {
                    continue;
                }
                var point = new Vector2(seen.x / seen.z, seen.y / seen.z);
                low = Vector2.Min(low, point);
                high = Vector2.Max(high, point);
            }
            return high.x < low.x ? new Rect(0f, 0f, 0f, 0f) : Rect.MinMaxRect(low.x, low.y, high.x, high.y);
        }
    }
}
