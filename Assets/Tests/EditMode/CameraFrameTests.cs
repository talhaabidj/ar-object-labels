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
            try
            {
                var camera = holder.AddComponent<Camera>();
                camera.fieldOfView = 60f;
                camera.aspect = (float)Width / Height;
                camera.transform.SetPositionAndRotation(new Vector3(0.4f, 1.45f, -1.2f), Quaternion.Euler(25f, -40f, 0f));
                CameraFrame frame = FrameAt(camera.transform.position, camera.transform.rotation);

                Ray ours = frame.RayThrough(new Vector2(x, y));
                Ray unitys = camera.ViewportPointToRay(ImageMapping.PictureToViewport(new Vector2(x, y), Width, Height));

                Assert.Less(Vector3.Angle(ours.direction, unitys.direction), 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
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
    }
}
