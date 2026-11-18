using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class FixedBoxSourceTests
    {
        GameObject cameraHolder;
        GameObject sourceHolder;
        Camera viewCamera;
        FixedBoxSource source;
        readonly List<Detection> found = new List<Detection>();

        [SetUp]
        public void MakeACameraAndASource()
        {
            cameraHolder = new GameObject("test camera");
            viewCamera = cameraHolder.AddComponent<Camera>();
            viewCamera.transform.SetPositionAndRotation(new Vector3(0f, 1.45f, -1.45f), Quaternion.Euler(8f, 0f, 0f));
            sourceHolder = new GameObject("test source");
            source = sourceHolder.AddComponent<FixedBoxSource>();
            var fields = new SerializedObject(source);
            fields.FindProperty("viewCamera").objectReferenceValue = viewCamera;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void RemoveThem()
        {
            Object.DestroyImmediate(cameraHolder);
            Object.DestroyImmediate(sourceHolder);
        }

        [Test]
        public void BeforeTheStartFrameIsKept_ThereIsNoBox()
        {
            Assert.IsFalse(source.TryRead(found, out _));
            CollectionAssert.IsEmpty(found);
        }

        [Test]
        public void AfterTheCameraMoves_TheBoxStillComesWithTheStartFrame()
        {
            source.KeepStartFrame();
            Assert.IsTrue(source.TryRead(found, out CameraFrame atStart));
            Detection boxAtStart = found[0];

            viewCamera.transform.SetPositionAndRotation(new Vector3(1.2f, 1.1f, 0.4f), Quaternion.Euler(20f, 250f, 0f));
            Assert.IsTrue(source.TryRead(found, out CameraFrame afterTheMove));

            Assert.AreEqual(atStart.Position, afterTheMove.Position);
            Assert.AreEqual(atStart.Rotation, afterTheMove.Rotation);
            Assert.AreEqual(new Vector3(0f, 1.45f, -1.45f), afterTheMove.Position);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(boxAtStart.Left, found[0].Left);
            Assert.AreEqual(boxAtStart.Bottom, found[0].Bottom);
        }

        [Test]
        public void TheRayThroughTheBox_DoesNotMoveWithTheCamera()
        {
            source.KeepStartFrame();
            source.TryRead(found, out CameraFrame atStart);
            Ray rayAtStart = atStart.RayThrough(found[0].BottomCentre);

            viewCamera.transform.SetPositionAndRotation(new Vector3(-1f, 1.2f, 0.8f), Quaternion.Euler(0f, 90f, 0f));
            source.TryRead(found, out CameraFrame afterTheMove);
            Ray rayAfterTheMove = afterTheMove.RayThrough(found[0].BottomCentre);

            Assert.AreEqual(rayAtStart.origin, rayAfterTheMove.origin);
            Assert.AreEqual(rayAtStart.direction, rayAfterTheMove.direction);
        }
    }
}
