using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class TrueBoxTests
    {
        const int Width = 993;
        const int Height = 473;

        static readonly CameraFrame frame = new CameraFrame(Vector3.zero, Quaternion.identity, 60f, Width, Height, 0f);

        static List<Vector3> CornersOf(Vector3 middle, float side)
        {
            var corners = new List<Vector3>();
            for (int corner = 0; corner < 8; corner++)
            {
                var sign = new Vector3((corner & 1) == 0 ? -1f : 1f, (corner & 2) == 0 ? -1f : 1f, (corner & 4) == 0 ? -1f : 1f);
                corners.Add(middle + sign * (side / 2f));
            }
            return corners;
        }

        [Test]
        public void CubeStraightAhead_GivesACentredSquareBox()
        {
            bool made = TrueBox.TryMake("cube", frame, CornersOf(new Vector3(0f, 0f, 5f), 1f), 0.5f, out Detection box);

            // The cube's near face is 4.5 m ahead and reaches 0.5 m up; half the picture's height is tan(30 degrees) at 1 m.
            float half = 0.5f / 4.5f / Mathf.Tan(30f * Mathf.Deg2Rad) * (Height / 2f);
            Assert.IsTrue(made);
            Assert.AreEqual("cube", box.ClassName);
            Assert.AreEqual(Width / 2f, box.Centre.x, 0.01f);
            Assert.AreEqual(Height / 2f, box.Centre.y, 0.01f);
            Assert.AreEqual(2f * half, box.Height, 0.01f);
            Assert.AreEqual(box.Height, box.Width, 0.01f);
        }

        [Test]
        public void CubeHigherInTheRoom_IsHigherInThePicture()
        {
            TrueBox.TryMake("cube", frame, CornersOf(new Vector3(0f, 1f, 5f), 1f), 0.5f, out Detection box);

            Assert.Less(box.Centre.y, Height / 2f, "y counts down from the top of the picture");
        }

        [Test]
        public void CubeBehindTheCamera_GivesNoBox()
        {
            Assert.IsFalse(TrueBox.TryMake("cube", frame, CornersOf(new Vector3(0f, 0f, -5f), 1f), 0.5f, out _));
        }

        [Test]
        public void CubeAroundTheCamera_GivesNoBox()
        {
            Assert.IsFalse(TrueBox.TryMake("cube", frame, CornersOf(Vector3.zero, 1f), 0.5f, out _));
        }

        [Test]
        public void CubeHalfOutOfThePicture_IsCutAtTheEdge()
        {
            // The picture's left edge is 5 * tan(30 degrees) * 993 / 473 = 6.06 m to the left at 5 m ahead.
            bool made = TrueBox.TryMake("cube", frame, CornersOf(new Vector3(-6.06f, 0f, 5f), 1f), 0.3f, out Detection box);

            Assert.IsTrue(made);
            Assert.AreEqual(0f, box.Left, 0.0001f);
            Assert.Greater(box.Right, 0f);
        }

        [Test]
        public void CubeMostlyOutOfThePicture_GivesNoBox()
        {
            Assert.IsFalse(TrueBox.TryMake("cube", frame, CornersOf(new Vector3(-6.5f, 0f, 5f), 1f), 0.5f, out _));
        }

        [Test]
        public void CubeOutsideThePicture_GivesNoBox()
        {
            Assert.IsFalse(TrueBox.TryMake("cube", frame, CornersOf(new Vector3(-9f, 0f, 5f), 1f), 0f, out _));
        }

        [Test]
        public void NoPoints_GiveNoBox()
        {
            Assert.IsFalse(TrueBox.TryMake("cube", frame, new List<Vector3>(), 0.5f, out _));
        }

        [Test]
        public void TheBox_IsTheOneUnitysOwnCameraGives()
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
                var turned = new CameraFrame(camera.transform.position, camera.transform.rotation, 60f, Width, Height, 0f);
                List<Vector3> corners = CornersOf(camera.transform.TransformPoint(new Vector3(0.2f, -0.1f, 2f)), 0.4f);

                float left = float.MaxValue, top = float.MaxValue, right = float.MinValue, bottom = float.MinValue;
                foreach (Vector3 corner in corners)
                {
                    Vector3 seen = camera.WorldToViewportPoint(corner);
                    Vector2 point = PictureSpace.ViewportToPicture(seen, Width, Height);
                    left = Mathf.Min(left, point.x);
                    right = Mathf.Max(right, point.x);
                    top = Mathf.Min(top, point.y);
                    bottom = Mathf.Max(bottom, point.y);
                }

                Assert.IsTrue(TrueBox.TryMake("cube", turned, corners, 0.5f, out Detection box));
                Assert.AreEqual(left, box.Left, 0.05f);
                Assert.AreEqual(top, box.Top, 0.05f);
                Assert.AreEqual(right, box.Right, 0.05f);
                Assert.AreEqual(bottom, box.Bottom, 0.05f);
            }
            finally
            {
                Object.DestroyImmediate(holder);
                Object.DestroyImmediate(picture);
            }
        }
    }
}
