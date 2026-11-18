using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class PlacedDetectionTests
    {
        static readonly Detection box = new Detection("laptop", 0.9f, 100f, 50f, 300f, 250f);
        static readonly Vector3 point = new Vector3(1f, 0.74f, 3f);

        [Test]
        public void PointThroughTheCentre_IsTheMiddleOfTheRoomBox()
        {
            Bounds roomBox = new PlacedDetection(box, point, 0.4f, 0.3f, BoxPointKind.Centre).RoomBox;

            Assert.AreEqual(point, roomBox.center);
            Assert.AreEqual(new Vector3(0.4f, 0.3f, 0.4f), roomBox.size);
        }

        [TestCase(BoxPointKind.BottomCentre)]
        [TestCase(BoxPointKind.BottomCentreSetBack)]
        public void PointThroughTheLowerEdge_IsUnderTheRoomBox(BoxPointKind kind)
        {
            Bounds roomBox = new PlacedDetection(box, point, 0.4f, 0.3f, kind).RoomBox;

            Assert.AreEqual(point.y, roomBox.min.y, 0.0001f, "the box stands on the point");
            Assert.AreEqual(point.x, roomBox.center.x, 0.0001f);
            Assert.AreEqual(point.z, roomBox.center.z, 0.0001f);
        }
    }
}
