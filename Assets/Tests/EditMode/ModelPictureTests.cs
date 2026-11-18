using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ModelPictureTests
    {
        const int ModelSize = 8;
        const int Grey = 114;

        ModelPicture square;
        Texture2D cameraPicture;

        [SetUp]
        public void MakeTheSquare()
        {
            square = new ModelPicture(ModelSize);
        }

        [TearDown]
        public void RemoveEverything()
        {
            square.Dispose();
            Object.DestroyImmediate(cameraPicture);
        }

        // A picture filled by a rule: colour for each pixel, x from the left, y from the top.
        void MakePicture(int width, int height, System.Func<int, int, Color32> colourAt)
        {
            cameraPicture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // A texture counts its rows from the bottom.
                    cameraPicture.SetPixel(x, height - 1 - y, colourAt(x, y));
                }
            }
            cameraPicture.Apply(false);
        }

        // The square's pixels, x from the left, y from the top.
        Color32[,] Read(RenderTexture drawn)
        {
            RenderTexture before = RenderTexture.active;
            RenderTexture.active = drawn;
            var copy = new Texture2D(ModelSize, ModelSize, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, ModelSize, ModelSize), 0, 0);
            RenderTexture.active = before;
            var pixels = new Color32[ModelSize, ModelSize];
            for (int y = 0; y < ModelSize; y++)
            {
                for (int x = 0; x < ModelSize; x++)
                {
                    pixels[x, y] = copy.GetPixel(x, ModelSize - 1 - y);
                }
            }
            Object.DestroyImmediate(copy);
            return pixels;
        }

        static void AssertColour(int red, int green, int blue, Color32 actual, string where)
        {
            Assert.AreEqual(red, actual.r, 2, where + ", red");
            Assert.AreEqual(green, actual.g, 2, where + ", green");
            Assert.AreEqual(blue, actual.b, 2, where + ", blue");
        }

        [Test]
        public void WidePicture_FillsTheTopOfTheSquare_AndTheRestIsGrey()
        {
            // 16 x 8 shrinks by half to 8 x 4: the top four rows of the square.
            MakePicture(16, 8, (x, y) => new Color32(255, 0, 51, 255));

            Color32[,] pixels = Read(square.Draw(cameraPicture));

            AssertColour(255, 0, 51, pixels[0, 0], "top-left");
            AssertColour(255, 0, 51, pixels[7, 3], "last row of the picture");
            AssertColour(Grey, Grey, Grey, pixels[0, 4], "first row under the picture");
            AssertColour(Grey, Grey, Grey, pixels[7, 7], "bottom-right");
        }

        [Test]
        public void TallPicture_FillsTheLeftOfTheSquare()
        {
            MakePicture(8, 16, (x, y) => new Color32(0, 255, 0, 255));

            Color32[,] pixels = Read(square.Draw(cameraPicture));

            AssertColour(0, 255, 0, pixels[3, 7], "last column of the picture");
            AssertColour(Grey, Grey, Grey, pixels[4, 0], "first column beside the picture");
        }

        [Test]
        public void LeftHalfRedRightHalfBlue_StaysThatWayRound()
        {
            MakePicture(16, 8, (x, y) => x < 8 ? new Color32(255, 0, 0, 255) : new Color32(0, 0, 255, 255));

            Color32[,] pixels = Read(square.Draw(cameraPicture));

            AssertColour(255, 0, 0, pixels[1, 1], "left");
            AssertColour(0, 0, 255, pixels[6, 1], "right");
        }

        [Test]
        public void TopRowOfThePicture_IsTheTopRowOfTheSquare()
        {
            MakePicture(8, 8, (x, y) => y == 0 ? new Color32(0, 255, 0, 255) : new Color32(0, 0, 0, 255));

            Color32[,] pixels = Read(square.Draw(cameraPicture));

            AssertColour(0, 255, 0, pixels[3, 0], "top row");
            AssertColour(0, 0, 0, pixels[3, 1], "second row");
        }

        [Test]
        public void SecondPicture_LeavesNothingOfTheFirst()
        {
            MakePicture(8, 8, (x, y) => new Color32(255, 255, 255, 255));
            square.Draw(cameraPicture);
            Object.DestroyImmediate(cameraPicture);
            MakePicture(16, 8, (x, y) => new Color32(0, 0, 0, 255));

            Color32[,] pixels = Read(square.Draw(cameraPicture));

            AssertColour(Grey, Grey, Grey, pixels[0, 7], "bottom-left");
        }
    }
}
