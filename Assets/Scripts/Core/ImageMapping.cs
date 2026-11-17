using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// Between the camera's picture and the square picture a model is given. The picture is shrunk to fit the
    /// square and put in its top-left corner; the rest of the square is a grey border.
    /// </summary>
    public static class ImageMapping
    {
        /// <summary>How much the picture is shrunk (or grown) to fit the model's square.</summary>
        public static float ScaleToModel(int pictureWidth, int pictureHeight, int modelSize)
        {
            return Mathf.Min((float)modelSize / pictureWidth, (float)modelSize / pictureHeight);
        }

        /// <summary>A point of the model's square as a point of the camera's picture, both in pixels from the top-left.</summary>
        public static Vector2 ModelToPicture(Vector2 modelPoint, int pictureWidth, int pictureHeight, int modelSize)
        {
            return modelPoint / ScaleToModel(pictureWidth, pictureHeight, modelSize);
        }

        /// <summary>A point of the camera's picture as a point of the model's square.</summary>
        public static Vector2 PictureToModel(Vector2 picturePoint, int pictureWidth, int pictureHeight, int modelSize)
        {
            return picturePoint * ScaleToModel(pictureWidth, pictureHeight, modelSize);
        }

        /// <summary>A box given in the model's square as a box in the camera's picture, kept inside the picture.</summary>
        public static Detection ModelToPicture(Detection inModel, int pictureWidth, int pictureHeight, int modelSize)
        {
            float scale = ScaleToModel(pictureWidth, pictureHeight, modelSize);
            return new Detection(inModel.ClassName, inModel.Score,
                Mathf.Clamp(inModel.Left / scale, 0f, pictureWidth),
                Mathf.Clamp(inModel.Top / scale, 0f, pictureHeight),
                Mathf.Clamp(inModel.Right / scale, 0f, pictureWidth),
                Mathf.Clamp(inModel.Bottom / scale, 0f, pictureHeight));
        }

        /// <summary>
        /// A picture point in pixels from the top-left as a viewport point: 0 to 1 from the bottom-left, the
        /// way Unity's camera counts.
        /// </summary>
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
