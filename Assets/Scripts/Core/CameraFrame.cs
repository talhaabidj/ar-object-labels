using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// What is kept with a picture: where the camera was, how it was turned, how wide it saw, and the picture's
    /// size. A box found in that picture is sent into the room from this pose, not from where the camera is
    /// by the time the result arrives.
    /// </summary>
    public readonly struct CameraFrame
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float VerticalFieldOfView;
        public readonly int PictureWidth;
        public readonly int PictureHeight;
        public readonly float Time;

        public CameraFrame(Vector3 position, Quaternion rotation, float verticalFieldOfView, int pictureWidth,
            int pictureHeight, float time)
        {
            Position = position;
            Rotation = rotation;
            VerticalFieldOfView = verticalFieldOfView;
            PictureWidth = pictureWidth;
            PictureHeight = pictureHeight;
            Time = time;
        }

        /// <summary>
        /// The ray from the camera through a point of the picture, given in pixels from the top-left corner.
        /// It takes the middle of the picture as the middle of the lens, which holds in the Editor's simulation.
        /// </summary>
        public Ray RayThrough(Vector2 picturePoint)
        {
            float across = picturePoint.x / PictureWidth * 2f - 1f;
            float upwards = 1f - picturePoint.y / PictureHeight * 2f;
            float halfHeight = Mathf.Tan(VerticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * PictureWidth / PictureHeight;
            var inCamera = new Vector3(across * halfWidth, upwards * halfHeight, 1f);
            return new Ray(Position, Rotation * inCamera.normalized);
        }

        /// <summary>How long a stretch of this many pixels is in the room, at this distance straight ahead.</summary>
        public float MetresFor(float pixels, float distanceAhead)
        {
            float halfHeight = Mathf.Tan(VerticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            return pixels / PictureHeight * 2f * halfHeight * distanceAhead;
        }
    }
}
