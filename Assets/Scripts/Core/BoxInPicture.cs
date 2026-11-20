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
        /// given by its middle and size in its own axes, and the turn of those axes. A box that reaches behind
        /// the camera is cut where it crosses just in front of the camera, so it fills the picture on that side.
        /// An empty rectangle when the whole box is behind the camera.
        /// </summary>
        public static Rect Around(Bounds box, Quaternion turn, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            Quaternion toPicture = Quaternion.Inverse(cameraRotation);
            var low = new Vector2(float.MaxValue, float.MaxValue);
            var high = new Vector2(float.MinValue, float.MinValue);
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 seen = Corner(corner, box, turn, cameraPosition, toPicture);
                if (seen.z > NearestSeen)
                {
                    Widen(ref low, ref high, seen);
                }
                // Each edge from this corner to a corner one step along an axis, taken once.
                for (int axis = 1; axis <= 4; axis <<= 1)
                {
                    if ((corner & axis) != 0)
                    {
                        continue;
                    }
                    Vector3 other = Corner(corner | axis, box, turn, cameraPosition, toPicture);
                    if ((seen.z > NearestSeen) != (other.z > NearestSeen))
                    {
                        float share = (NearestSeen - seen.z) / (other.z - seen.z);
                        Widen(ref low, ref high, Vector3.Lerp(seen, other, share));
                    }
                }
            }
            return high.x < low.x ? new Rect(0f, 0f, 0f, 0f) : Rect.MinMaxRect(low.x, low.y, high.x, high.y);
        }

        // One of the box's eight corners, as the camera sees it: x right, y up, z ahead.
        static Vector3 Corner(int corner, Bounds box, Quaternion turn, Vector3 cameraPosition, Quaternion toPicture)
        {
            var side = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
            return toPicture * (box.center + turn * Vector3.Scale(box.extents, side) - cameraPosition);
        }

        static void Widen(ref Vector2 low, ref Vector2 high, Vector3 seen)
        {
            var point = new Vector2(seen.x / seen.z, seen.y / seen.z);
            low = Vector2.Min(low, point);
            high = Vector2.Max(high, point);
        }
    }
}
