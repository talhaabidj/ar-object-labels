using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelPlacementTests
    {
        static readonly Vector3 top = new Vector3(0f, 1f, 0f);
        const float Radius = 0.05f;
        const float HalfHeight = 0.02f;
        const float Gap = 0.02f;

        [Test]
        public void FromTheSide_LowerEdgeIsOneGapAboveTheTop()
        {
            var camera = new Vector3(0f, 1f, -2f);
            Vector3 centre = LabelPlacement.Centre(top, Radius, HalfHeight, Gap, camera, Vector3.up);

            Assert.AreEqual(top.x, centre.x, 0.0001f);
            Assert.AreEqual(top.z, centre.z, 0.0001f);
            Assert.AreEqual(top.y + Gap, centre.y - HalfHeight, 0.0001f);
        }

        [Test]
        public void FromBelow_IsPlacedAsFromTheSide()
        {
            var camera = new Vector3(0f, 0.2f, -1f);
            Vector3 centre = LabelPlacement.Centre(top, Radius, HalfHeight, Gap, camera, Vector3.up);

            Assert.AreEqual(top.y + Gap + HalfHeight, centre.y, 0.0001f);
        }

        [Test]
        public void FromStraightAbove_ClearsTheObjectInThePicture()
        {
            var camera = new Vector3(0f, 3f, 0f);
            Vector3 topOfPicture = Vector3.forward;
            Vector3 centre = LabelPlacement.Centre(top, Radius, HalfHeight, Gap, camera, topOfPicture);

            // In the picture the label's lower edge is a gap beyond the object's far edge.
            float lowerEdge = Vector3.Dot(centre - top, topOfPicture) - HalfHeight;
            Assert.AreEqual(Radius + Gap, lowerEdge, 0.0001f);
        }

        [TestCase(0f)]
        [TestCase(20f)]
        [TestCase(45f)]
        [TestCase(60f)]
        [TestCase(70f)]
        [TestCase(85f)]
        [TestCase(90f)]
        public void FromAnyHeight_StaysAGapClearOfTheObjectInThePicture(float pitch)
        {
            Quaternion look = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 upInPicture = look * Vector3.up;
            Vector3 centre = LabelPlacement.Centre(top, Radius, HalfHeight, Gap, top - look * Vector3.forward * 2f, upInPicture);

            float labelLowerEdge = Vector3.Dot(centre - top, upInPicture) - HalfHeight;
            float objectFarEdge = Radius * Mathf.Sin(pitch * Mathf.Deg2Rad);
            Assert.GreaterOrEqual(labelLowerEdge - objectFarEdge, Gap - 0.0001f);
        }

        // A wall at x = 2 whose room side is towards smaller x; a label 0.20 m wide and 0.04 m high facing along z.
        static readonly Vector3 wallPoint = new Vector3(2f, 0f, 0f);
        static readonly Vector3 intoRoom = Vector3.left;

        [Test]
        public void LabelReachingIntoAWall_IsMovedOutByExactlyWhatIsMissing()
        {
            var centre = new Vector3(1.95f, 1f, 0f);
            Vector3 moved = LabelPlacement.KeepOutOfWall(centre, Vector3.right, Vector3.up, 0.10f, 0.02f, wallPoint, intoRoom, 0.01f);

            Assert.AreEqual(2f - 0.10f - 0.01f, moved.x, 0.0001f);
            Assert.AreEqual(centre.y, moved.y, 0.0001f);
            Assert.AreEqual(centre.z, moved.z, 0.0001f);
        }

        [Test]
        public void LabelClearOfTheWall_IsNotMoved()
        {
            var centre = new Vector3(1.5f, 1f, 0f);
            Vector3 moved = LabelPlacement.KeepOutOfWall(centre, Vector3.right, Vector3.up, 0.10f, 0.02f, wallPoint, intoRoom, 0.01f);

            Assert.AreEqual(centre, moved);
        }

        [Test]
        public void LabelEdgeOnToTheWall_NeedsOnlyTheMargin()
        {
            // Turned so its flat face is parallel to the wall: it has no width towards the wall.
            var centre = new Vector3(1.995f, 1f, 0f);
            Vector3 moved = LabelPlacement.KeepOutOfWall(centre, Vector3.forward, Vector3.up, 0.10f, 0.02f, wallPoint, intoRoom, 0.01f);

            Assert.AreEqual(2f - 0.01f, moved.x, 0.0001f);
        }

        [Test]
        public void LabelBehindTheWall_IsBroughtBackIntoTheRoom()
        {
            var centre = new Vector3(2.05f, 1f, 0f);
            Vector3 moved = LabelPlacement.KeepOutOfWall(centre, Vector3.right, Vector3.up, 0.10f, 0.02f, wallPoint, intoRoom, 0.01f);

            Assert.AreEqual(2f - 0.10f - 0.01f, moved.x, 0.0001f);
        }

        [Test]
        public void LabelWhollyBehindTheWall_IsNotMoved()
        {
            var centre = new Vector3(2.5f, 1f, 0f);
            Vector3 moved = LabelPlacement.KeepOutOfWall(centre, Vector3.right, Vector3.up, 0.10f, 0.02f, wallPoint, intoRoom, 0.01f);

            Assert.AreEqual(centre, moved);
        }
    }
}
