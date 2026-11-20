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
        public void BoxAtTheTopLeftOfTheSquare_StaysAtTheTopLeftOfThePicture()
        {
            Detection inPicture = ImageMapping.ModelToPicture(new Detection("cup", 0.5f, 0f, 0f, 64f, 32f), Width, Height, Model);

            Assert.AreEqual(0f, inPicture.Left, 0.001f);
            Assert.AreEqual(0f, inPicture.Top, 0.001f);
            Assert.AreEqual(64f * 993f / 640f, inPicture.Right, 0.001f);
        }

        [Test]
        public void BoxReachingTheSquaresRightEdge_ReachesTheRightEdgeOfAWidePicture()
        {
            Detection inPicture = ImageMapping.ModelToPicture(new Detection("cup", 0.5f, 600f, 0f, 640f, 10f), Width, Height, Model);

            Assert.AreEqual(993f, inPicture.Right, 0.001f);
        }

        [Test]
        public void BottomOfTheShrunkPicture_IsTheBottomOfThePicture()
        {
            float bottomInModel = Height * 640f / Width;
            Detection inPicture = ImageMapping.ModelToPicture(new Detection("cup", 0.5f, 0f, 0f, 10f, bottomInModel), Width, Height, Model);

            Assert.AreEqual(473f, inPicture.Bottom, 0.001f);
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
    }
}
