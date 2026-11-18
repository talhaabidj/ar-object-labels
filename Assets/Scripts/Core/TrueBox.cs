using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>Works out the box a perfect detector would give for an object whose place in the room is known.</summary>
    public static class TrueBox
    {
        /// <summary>
        /// The smallest picture box around the given room points, cut off at the picture's edges. False when
        /// any point is behind the camera, or when less than <paramref name="minVisibleShare"/> of the box
        /// lies inside the picture.
        /// </summary>
        public static bool TryMake(string className, CameraFrame frame, IReadOnlyList<Vector3> roomPoints,
            float minVisibleShare, out Detection detection)
        {
            detection = default;
            float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
            for (int i = 0; i < roomPoints.Count; i++)
            {
                if (!frame.TryPicturePoint(roomPoints[i], out Vector2 point))
                {
                    return false;
                }
                left = Mathf.Min(left, point.x);
                right = Mathf.Max(right, point.x);
                top = Mathf.Min(top, point.y);
                bottom = Mathf.Max(bottom, point.y);
            }

            float wholeArea = (right - left) * (bottom - top);
            if (roomPoints.Count == 0 || wholeArea <= 0f)
            {
                return false;
            }

            left = Mathf.Clamp(left, 0f, frame.PictureWidth);
            right = Mathf.Clamp(right, 0f, frame.PictureWidth);
            top = Mathf.Clamp(top, 0f, frame.PictureHeight);
            bottom = Mathf.Clamp(bottom, 0f, frame.PictureHeight);
            float seenArea = (right - left) * (bottom - top);
            if (seenArea <= 0f || seenArea / wholeArea < minVisibleShare)
            {
                return false;
            }

            detection = new Detection(className, 1f, left, top, right, bottom);
            return true;
        }
    }
}
