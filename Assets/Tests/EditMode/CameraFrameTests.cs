using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class CameraFrameTests
    {
        const int Width = 993;
        const int Height = 473;

        static CameraFrame FrameAt(Vector3 position, Quaternion rotation)
        {
            return new CameraFrame(position, rotation, 60f, Width, Height, 0f);
        }

        [Test]
        public void MiddleOfThePicture_IsStraightAhead()
        {
            Quaternion rotation = Quaternion.Euler(20f, 50f, 0f);
            Ray ray = FrameAt(new Vector3(1f, 2f, 3f), rotation).RayThrough(new Vector2(Width / 2f, Height / 2f));

            Assert.AreEqual(new Vector3(1f, 2f, 3f), ray.origin);
            Assert.Greater(Vector3.Dot(ray.direction, rotation * Vector3.forward), 0.99999f);
        }

        [Test]
        public void TopEdge_IsHalfTheFieldOfViewUp()
        {
            Ray ray = FrameAt(Vector3.zero, Quaternion.identity).RayThrough(new Vector2(Width / 2f, 0f));

            Assert.AreEqual(30f, Vector3.Angle(ray.direction, Vector3.forward), 0.001f);
            Assert.Greater(ray.direction.y, 0f, "the top of the picture is up in the room");
        }

        [Test]
        public void RightEdge_IsToTheRight()
        {
            Ray ray = FrameAt(Vector3.zero, Quaternion.identity).RayThrough(new Vector2(Width, Height / 2f));

            Assert.Greater(ray.direction.x, 0f);
            Assert.AreEqual(0f, ray.direction.y, 0.0001f);
        }

        [TestCase(0f, 0f)]
        [TestCase(993f, 473f)]
        [TestCase(120f, 400f)]
        [TestCase(700f, 55f)]
        [TestCase(496.5f, 236.5f)]
        public void AnyPoint_GivesTheSameRayAsUnitysOwnCamera(float x, float y)
        {
            var holder = new GameObject("test camera");
            // Unity's camera works in the pixels of what it draws to, so a picture of fixed size keeps its answer
            // from depending on the size of the Editor's Game view.
            var picture = new RenderTexture(Width, Height, 0);
            try
            {
                var camera = holder.AddComponent<Camera>();
                camera.targetTexture = picture;
                camera.fieldOfView = 60f;
                camera.aspect = (float)Width / Height;
                camera.transform.SetPositionAndRotation(new Vector3(0.4f, 1.45f, -1.2f), Quaternion.Euler(25f, -40f, 0f));
                CameraFrame frame = FrameAt(camera.transform.position, camera.transform.rotation);

                Ray ours = frame.RayThrough(new Vector2(x, y));
                Ray unitys = camera.ViewportPointToRay(PictureSpace.PictureToViewport(new Vector2(x, y), Width, Height));

                Assert.Less(Vector3.Angle(ours.direction, unitys.direction), 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(holder);
                Object.DestroyImmediate(picture);
            }
        }

        [TestCase(0f, 0f)]
        [TestCase(993f, 473f)]
        [TestCase(120f, 400f)]
        [TestCase(700f, 55f)]
        public void PicturePoint_IsTheWayBackFromTheRay(float x, float y)
        {
            CameraFrame frame = FrameAt(new Vector3(0.4f, 1.45f, -1.2f), Quaternion.Euler(25f, -40f, 0f));
            Vector3 roomPoint = frame.RayThrough(new Vector2(x, y)).GetPoint(2.7f);

            Assert.IsTrue(frame.TryPicturePoint(roomPoint, out Vector2 picturePoint));
            Assert.AreEqual(x, picturePoint.x, 0.01f);
            Assert.AreEqual(y, picturePoint.y, 0.01f);
        }

        [Test]
        public void PointBehindTheCamera_HasNoPlaceInThePicture()
        {
            CameraFrame frame = FrameAt(Vector3.zero, Quaternion.identity);

            Assert.IsFalse(frame.TryPicturePoint(new Vector3(0.2f, 0.1f, -3f), out _));
            Assert.IsFalse(frame.TryPicturePoint(new Vector3(1f, 0f, 0f), out _), "level with the camera, to its side");
        }

        [Test]
        public void PixelsToMetres_TheWholeHeightAtOneMetre()
        {
            float metres = FrameAt(Vector3.zero, Quaternion.identity).MetresFor(Height, 1f);

            Assert.AreEqual(2f * Mathf.Tan(30f * Mathf.Deg2Rad), metres, 0.0001f);
        }

        [Test]
        public void PixelsToMetres_TwiceAsFar_TwiceAsLong()
        {
            CameraFrame frame = FrameAt(Vector3.zero, Quaternion.identity);

            Assert.AreEqual(2f * frame.MetresFor(100f, 1.5f), frame.MetresFor(100f, 3f), 0.0001f);
        }

        // The focal length in pixels that gives the same picture as a 60 degree field of view.
        static readonly float SameFocal = Height / 2f / Mathf.Tan(30f * Mathf.Deg2Rad);

        [TestCase(0f, 0f)]
        [TestCase(250f, 300f)]
        [TestCase(993f, 473f)]
        public void LensWithItsMiddleAtThePicturesMiddle_GivesTheSameRay_AsTheFieldOfView(float x, float y)
        {
            CameraFrame plain = FrameAt(Vector3.zero, Quaternion.Euler(10f, 20f, 0f));
            CameraFrame withLens = plain.WithLens(new Vector2(SameFocal, SameFocal), new Vector2(Width / 2f, Height / 2f),
                new Vector2Int(Width, Height));

            Vector3 a = plain.RayThrough(new Vector2(x, y)).direction;
            Vector3 b = withLens.RayThrough(new Vector2(x, y)).direction;
            Assert.Less(Vector3.Angle(a, b), 0.01f);
        }

        [Test]
        public void LensMiddleLeftOfThePicturesMiddle_SendsTheMiddleRayToTheRight()
        {
            CameraFrame withLens = FrameAt(Vector3.zero, Quaternion.identity).WithLens(new Vector2(SameFocal, SameFocal),
                new Vector2(Width / 2f - 50f, Height / 2f), new Vector2Int(Width, Height));

            Assert.Greater(withLens.RayThrough(new Vector2(Width / 2f, Height / 2f)).direction.x, 0f);
        }

        [Test]
        public void LensGivenForAnotherPictureSize_IsScaledToThisPicture()
        {
            CameraFrame withLens = FrameAt(Vector3.zero, Quaternion.identity).WithLens(new Vector2(500f, 500f),
                new Vector2(320f, 240f), new Vector2Int(640, 480));

            Assert.AreEqual(500f * Width / 640f, withLens.FocalLength.x, 0.001f);
            Assert.AreEqual(Height / 2f, withLens.PrincipalPoint.y, 0.001f);
        }

        [Test]
        public void LensWithNoFocalLength_IsNotTaken()
        {
            CameraFrame withLens = FrameAt(Vector3.zero, Quaternion.identity).WithLens(Vector2.zero, Vector2.zero,
                new Vector2Int(Width, Height));

            Assert.IsFalse(withLens.HasLens);
        }

        [Test]
        public void WithALens_ThePointARayGoesThrough_ComesBackToTheSamePixel()
        {
            CameraFrame withLens = FrameAt(new Vector3(1f, 1.5f, 0f), Quaternion.Euler(25f, -10f, 0f)).WithLens(
                new Vector2(520f, 515f), new Vector2(480f, 250f), new Vector2Int(Width, Height));
            var pixel = new Vector2(700f, 120f);
            Ray ray = withLens.RayThrough(pixel);

            Assert.IsTrue(withLens.TryPicturePoint(ray.GetPoint(2f), out Vector2 back));
            Assert.AreEqual(pixel.x, back.x, 0.01f);
            Assert.AreEqual(pixel.y, back.y, 0.01f);
        }
    }
}
