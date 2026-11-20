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
    }
}
