using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class BoxInPictureTests
    {
        // A keyboard-like box, 0.4 wide and 0.1 deep, 2 m straight ahead of a camera at the origin.
        static readonly Bounds box = new Bounds(new Vector3(0f, 0f, 2f), new Vector3(0.4f, 0.02f, 0.1f));

        [Test]
        public void BoxSeenFromTheFront_IsAsWideAsItsWidth()
        {
            Rect seen = BoxInPicture.Around(box, Quaternion.identity, Vector3.zero, Quaternion.identity);

            // Its near face is 1.95 m away, so 0.4 m shows as 0.4 / 1.95.
            Assert.AreEqual(0.4f / 1.95f, seen.width, 0.0001f);
            Assert.AreEqual(0f, seen.center.x, 0.0001f);
        }

        [Test]
        public void SameBoxTurnedAQuarter_IsAsWideAsItsDepth()
        {
            Rect seen = BoxInPicture.Around(box, Quaternion.Euler(0f, 90f, 0f), Vector3.zero, Quaternion.identity);

            // Now the long side runs away from the camera; the near end is 1.8 m away.
            Assert.AreEqual(0.1f / 1.8f, seen.width, 0.0001f);
        }

        [Test]
        public void BoxToTheRightOfAStraightLook_IsRightOfTheMiddle()
        {
            var toTheRight = new Bounds(new Vector3(1f, 0f, 2f), Vector3.one * 0.1f);

            Rect seen = BoxInPicture.Around(toTheRight, Quaternion.identity, Vector3.zero, Quaternion.identity);

            Assert.Greater(seen.xMin, 0.4f);
        }

        [Test]
        public void CameraTurnedTowardsTheBox_SeesItInTheMiddle()
        {
            var toTheRight = new Bounds(new Vector3(2f, 0f, 0f), Vector3.one * 0.1f);

            Rect seen = BoxInPicture.Around(toTheRight, Quaternion.identity, Vector3.zero, Quaternion.Euler(0f, 90f, 0f));

            Assert.AreEqual(0f, seen.center.x, 0.0001f);
        }

        [Test]
        public void BoxBehindTheCamera_HasNoRectangle()
        {
            var behind = new Bounds(new Vector3(0f, 0f, -2f), Vector3.one * 0.1f);

            Rect seen = BoxInPicture.Around(behind, Quaternion.identity, Vector3.zero, Quaternion.identity);

            Assert.AreEqual(0f, seen.width * seen.height);
        }
    }
}
