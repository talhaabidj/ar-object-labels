using UnityEngine;

namespace ArObjectLabels.Tests
{
    /// <summary>Converts between picture pixels and Unity's viewport, so tests can compare with Unity's own camera.</summary>
    static class PictureSpace
    {
        /// <summary>A picture point in pixels from the top-left as a viewport point: 0 to 1 from the bottom-left.</summary>
        public static Vector2 PictureToViewport(Vector2 picturePoint, int pictureWidth, int pictureHeight)
        {
            return new Vector2(picturePoint.x / pictureWidth, 1f - picturePoint.y / pictureHeight);
        }

        /// <summary>A viewport point as a picture point in pixels from the top-left.</summary>
        public static Vector2 ViewportToPicture(Vector2 viewportPoint, int pictureWidth, int pictureHeight)
        {
            return new Vector2(viewportPoint.x * pictureWidth, (1f - viewportPoint.y) * pictureHeight);
        }
    }
}
