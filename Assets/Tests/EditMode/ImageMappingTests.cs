using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ImageMappingTests
    {
        const int Width = 993;
        const int Height = 473;
        const int Model = 640;

        [Test]
        public void WidePicture_IsShrunkByItsWidth()
        {
            Assert.AreEqual(640f / 993f, ImageMapping.ScaleToModel(Width, Height, Model), 0.00001f);
        }

        [Test]
        public void TallPicture_IsShrunkByItsHeight()
        {
            Assert.AreEqual(640f / 1000f, ImageMapping.ScaleToModel(500, 1000, Model), 0.00001f);
        }

        [Test]
        public void TopLeftCorner_StaysTheTopLeftCorner()
        {
            Assert.AreEqual(Vector2.zero, ImageMapping.ModelToPicture(Vector2.zero, Width, Height, Model));
        }

        [Test]
        public void RightEdgeOfTheSquare_IsTheRightEdgeOfAWidePicture()
        {
            Vector2 inPicture = ImageMapping.ModelToPicture(new Vector2(640f, 0f), Width, Height, Model);

            Assert.AreEqual(993f, inPicture.x, 0.001f);
        }

        [Test]
        public void BottomOfTheShrunkPicture_IsTheBottomOfThePicture()
        {
            float bottomInModel = Height * 640f / Width;
            Vector2 inPicture = ImageMapping.ModelToPicture(new Vector2(0f, bottomInModel), Width, Height, Model);

            Assert.AreEqual(473f, inPicture.y, 0.001f);
        }

        [TestCase(0f, 0f)]
        [TestCase(993f, 473f)]
        [TestCase(321.5f, 77.25f)]
        public void ThereAndBack_GivesTheSamePoint(float x, float y)
        {
            Vector2 inModel = ImageMapping.PictureToModel(new Vector2(x, y), Width, Height, Model);
            Vector2 back = ImageMapping.ModelToPicture(inModel, Width, Height, Model);

            Assert.AreEqual(x, back.x, 0.001f);
            Assert.AreEqual(y, back.y, 0.001f);
        }

        [Test]
        public void BoxInTheGreyBorder_IsCutAtThePicturesEdge()
        {
            var inModel = new Detection("vase", 0.9f, 600f, 250f, 640f, 400f);
            Detection inPicture = ImageMapping.ModelToPicture(inModel, Width, Height, Model);

            Assert.AreEqual(473f, inPicture.Bottom, 0.001f);
            Assert.AreEqual(993f, inPicture.Right, 0.001f);
            Assert.AreEqual("vase", inPicture.ClassName);
            Assert.AreEqual(0.9f, inPicture.Score);
        }

        [Test]
        public void TopLeftOfThePicture_IsTheTopLeftOfTheViewport()
        {
            Assert.AreEqual(new Vector2(0f, 1f), ImageMapping.PictureToViewport(Vector2.zero, Width, Height));
        }

        [Test]
        public void BottomRightOfThePicture_IsTheBottomRightOfTheViewport()
        {
            Assert.AreEqual(new Vector2(1f, 0f), ImageMapping.PictureToViewport(new Vector2(Width, Height), Width, Height));
        }

        [Test]
        public void ViewportThereAndBack_GivesTheSamePoint()
        {
            var point = new Vector2(250f, 300f);
            Vector2 back = ImageMapping.ViewportToPicture(ImageMapping.PictureToViewport(point, Width, Height), Width, Height);

            Assert.AreEqual(point.x, back.x, 0.001f);
            Assert.AreEqual(point.y, back.y, 0.001f);
        }
    }
}
