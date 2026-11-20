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
        // Nothing nearer than this, in metres ahead of the camera, has a place in the picture.
        const float NearestSeen = 0.01f;

        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float VerticalFieldOfView;
        public readonly int PictureWidth;
        public readonly int PictureHeight;
        /// <summary>When the picture was taken, in seconds that keep running while the game's time is paused.</summary>
        public readonly float Time;
        /// <summary>True when the camera picture's own lens is known; then it is used instead of the field of view.</summary>
        public readonly bool HasLens;
        /// <summary>The lens's focal length in picture pixels, across and up.</summary>
        public readonly Vector2 FocalLength;
        /// <summary>Where the lens's middle falls in the picture, in pixels from the top-left corner.</summary>
        public readonly Vector2 PrincipalPoint;

        public CameraFrame(Vector3 position, Quaternion rotation, float verticalFieldOfView, int pictureWidth,
            int pictureHeight, float time)
            : this(position, rotation, verticalFieldOfView, pictureWidth, pictureHeight, time, false, Vector2.zero, Vector2.zero)
        {
        }

        CameraFrame(Vector3 position, Quaternion rotation, float verticalFieldOfView, int pictureWidth, int pictureHeight,
            float time, bool hasLens, Vector2 focalLength, Vector2 principalPoint)
        {
            Position = position;
            Rotation = rotation;
            VerticalFieldOfView = verticalFieldOfView;
            PictureWidth = pictureWidth;
            PictureHeight = pictureHeight;
            Time = time;
            HasLens = hasLens;
            FocalLength = focalLength;
            PrincipalPoint = principalPoint;
        }

        /// <summary>
        /// The same frame with the camera picture's own lens, given for a picture of <paramref name="lensPictureSize"/>
        /// pixels and scaled to this frame's picture. A lens with no focal length is not taken.
        /// </summary>
        public CameraFrame WithLens(Vector2 focalLength, Vector2 principalPoint, Vector2Int lensPictureSize)
        {
            if (focalLength.x <= 0f || focalLength.y <= 0f || lensPictureSize.x <= 0 || lensPictureSize.y <= 0)
            {
                return this;
            }
            var scale = new Vector2((float)PictureWidth / lensPictureSize.x, (float)PictureHeight / lensPictureSize.y);
            return new CameraFrame(Position, Rotation, VerticalFieldOfView, PictureWidth, PictureHeight, Time, true,
                Vector2.Scale(focalLength, scale), Vector2.Scale(principalPoint, scale));
        }

        /// <summary>
        /// The ray from the camera through a point of the picture, given in pixels from the top-left corner. With
        /// the picture's own lens known, its focal length and middle are used; otherwise the middle of the picture
        /// is taken as the middle of the lens.
        /// </summary>
        public Ray RayThrough(Vector2 picturePoint)
        {
            if (HasLens)
            {
                var throughLens = new Vector3((picturePoint.x - PrincipalPoint.x) / FocalLength.x,
                    (PrincipalPoint.y - picturePoint.y) / FocalLength.y, 1f);
                return new Ray(Position, Rotation * throughLens.normalized);
            }
            float across = picturePoint.x / PictureWidth * 2f - 1f;
            float upwards = 1f - picturePoint.y / PictureHeight * 2f;
            float halfHeight = Mathf.Tan(VerticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * PictureWidth / PictureHeight;
            var inCamera = new Vector3(across * halfWidth, upwards * halfHeight, 1f);
            return new Ray(Position, Rotation * inCamera.normalized);
        }

        /// <summary>
        /// Where a point of the room shows in the picture, in pixels from the top-left corner. It is the way
        /// back from <see cref="RayThrough"/>. False for a point beside or behind the camera.
        /// </summary>
        public bool TryPicturePoint(Vector3 roomPoint, out Vector2 picturePoint)
        {
            Vector3 inCamera = Quaternion.Inverse(Rotation) * (roomPoint - Position);
            picturePoint = default;
            if (inCamera.z < NearestSeen)
            {
                return false;
            }
            if (HasLens)
            {
                picturePoint = new Vector2(PrincipalPoint.x + FocalLength.x * inCamera.x / inCamera.z,
                    PrincipalPoint.y - FocalLength.y * inCamera.y / inCamera.z);
                return true;
            }

            float halfHeight = Mathf.Tan(VerticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * PictureWidth / PictureHeight;
            float across = inCamera.x / inCamera.z / halfWidth;
            float upwards = inCamera.y / inCamera.z / halfHeight;
            picturePoint = new Vector2((across + 1f) / 2f * PictureWidth, (1f - upwards) / 2f * PictureHeight);
            return true;
        }

        /// <summary>How long a stretch of this many pixels is in the room, at this distance straight ahead.</summary>
        public float MetresFor(float pixels, float distanceAhead)
        {
            if (HasLens)
            {
                return pixels / FocalLength.y * distanceAhead;
            }
            float halfHeight = Mathf.Tan(VerticalFieldOfView * 0.5f * Mathf.Deg2Rad);
            return pixels / PictureHeight * 2f * halfHeight * distanceAhead;
        }
    }
}
