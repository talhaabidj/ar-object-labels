using System;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// The square picture a model is given, made on the graphics card: the camera's picture shrunk to fit and
    /// drawn into the top-left corner, the rest grey.
    /// </summary>
    public sealed class ModelPicture : IDisposable
    {
        // The grey models of this family were trained with: 114 of 255.
        const float BorderGrey = 114f / 255f;

        readonly RenderTexture square;

        public ModelPicture(int modelSize)
        {
            square = new RenderTexture(modelSize, modelSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            square.Create();
        }

        /// <summary>Draws the camera's picture into the square and gives the square back.</summary>
        public RenderTexture Draw(Texture cameraPicture)
        {
            float scale = ImageMapping.ScaleToModel(cameraPicture.width, cameraPicture.height, square.width);
            var shrunk = new Rect(0f, 0f, (int)(cameraPicture.width * scale), (int)(cameraPicture.height * scale));

            RenderTexture before = RenderTexture.active;
            RenderTexture.active = square;
            GL.Clear(false, true, BorderColour());
            GL.PushMatrix();
            // Pixels counted from the top-left corner, as the model's boxes are.
            GL.LoadPixelMatrix(0f, square.width, square.height, 0f);
            Graphics.DrawTexture(shrunk, cameraPicture);
            GL.PopMatrix();
            RenderTexture.active = before;
            return square;
        }

        public void Dispose()
        {
            square.Release();
            // Outside Play, as in the Editor's tests, Unity only allows the immediate way.
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(square);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(square);
            }
        }

        // The square stores colours the way pictures do. In a project that works in linear colours the grey
        // must be given in linear form to come out as 114.
        static Color BorderColour()
        {
            float grey = QualitySettings.activeColorSpace == ColorSpace.Linear ? Mathf.GammaToLinearSpace(BorderGrey) : BorderGrey;
            return new Color(grey, grey, grey, 1f);
        }
    }
}
