using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>Takes down a camera's pose and picture size at this moment.</summary>
    public static class CameraFrames
    {
        public static CameraFrame Of(Camera camera)
        {
            return Of(camera, camera.pixelWidth, camera.pixelHeight);
        }

        /// <summary>The same, for a picture of the camera's whole view that has another size than the screen.</summary>
        public static CameraFrame Of(Camera camera, int pictureWidth, int pictureHeight)
        {
            // Read from the projection the camera really uses; AR sets it and may leave "field of view" stale.
            float verticalFieldOfView = 2f * Mathf.Atan(1f / camera.projectionMatrix.m11) * Mathf.Rad2Deg;
            return new CameraFrame(camera.transform.position, camera.transform.rotation, verticalFieldOfView,
                pictureWidth, pictureHeight, Time.unscaledTime);
        }
    }
}
