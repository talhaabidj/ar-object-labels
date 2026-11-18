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

        static PlacedDetection Placed(BoxPointKind kind, Vector3 away)
        {
            return new PlacedDetection(box, point, 0.4f, 0.3f, 0.24f, away, kind);
        }

        [Test]
        public void PointThroughTheCentre_IsTheMiddleOfTheRoomBox()
        {
            Bounds roomBox = Placed(BoxPointKind.Centre, Vector3.forward).RoomBox;

            Assert.AreEqual(point, roomBox.center);
        }

        [TestCase(BoxPointKind.BottomCentre)]
        [TestCase(BoxPointKind.BottomCentreSetBack)]
        public void PointThroughTheLowerEdge_IsUnderTheRoomBox(BoxPointKind kind)
        {
            Bounds roomBox = Placed(kind, Vector3.forward).RoomBox;

            Assert.AreEqual(point.y, roomBox.min.y, 0.0001f, "the box stands on the point");
            Assert.AreEqual(point.x, roomBox.center.x, 0.0001f);
            Assert.AreEqual(point.z, roomBox.center.z, 0.0001f);
        }

        [Test]
        public void RoomBox_IsAsWideAsTheWidth_AndAsDeepAsTheDepth()
        {
            Assert.AreEqual(new Vector3(0.4f, 0.3f, 0.24f), Placed(BoxPointKind.BottomCentre, Vector3.forward).RoomBox.size);
        }

        [Test]
        public void RoomBox_IsTurnedSoThatItsDepthRunsAwayFromTheCamera()
        {
            // Found by a camera looking along the room's x axis and a little down.
            PlacedDetection placed = Placed(BoxPointKind.BottomCentre, new Vector3(2f, -0.6f, 0f));

            Vector3 depthInTheRoom = placed.Turn * Vector3.forward;
            Vector3 widthInTheRoom = placed.Turn * Vector3.right;

            Assert.AreEqual(1f, depthInTheRoom.x, 0.0001f);
            Assert.AreEqual(0f, depthInTheRoom.y, 0.0001f);
            Assert.AreEqual(1f, Mathf.Abs(widthInTheRoom.z), 0.0001f);
        }

        [Test]
        public void FoundFromStraightAbove_TheBoxIsNotTurned()
        {
            Assert.AreEqual(Quaternion.identity, Placed(BoxPointKind.BottomCentre, Vector3.down).Turn);
        }
    }
}
