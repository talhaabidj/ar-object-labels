using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class BoxPointTests
    {
        static readonly Detection box = new Detection("laptop", 0.93f, 100f, 50f, 300f, 250f);

        [Test]
        public void Box_KnowsItsSize()
        {
            Assert.AreEqual(200f, box.Width);
            Assert.AreEqual(200f, box.Height);
        }

        [Test]
        public void Centre_IsTheMiddleOfTheBox()
        {
            Assert.AreEqual(new Vector2(200f, 150f), BoxPoint.Pick(box, BoxPointKind.Centre));
        }

        [Test]
        public void BottomCentre_IsTheMiddleOfTheLowerEdge()
        {
            // y counts down from the top of the picture, so the lower edge has the larger y.
            Assert.AreEqual(new Vector2(200f, 250f), BoxPoint.Pick(box, BoxPointKind.BottomCentre));
        }

        [Test]
        public void BottomCentreSetBack_GoesThroughTheLowerEdgeToo()
        {
            Assert.AreEqual(BoxPoint.Pick(box, BoxPointKind.BottomCentre), BoxPoint.Pick(box, BoxPointKind.BottomCentreSetBack));
        }

        [Test]
        public void BoxClearOfTheBottomEdge_IsSetBackAsAsked()
        {
            Assert.AreEqual(BoxPointKind.BottomCentreSetBack, BoxPoint.KindFor(box, BoxPointKind.BottomCentreSetBack, 500, 0.01f));
        }

        [Test]
        public void BoxAtTheBottomEdge_IsNotSetBack()
        {
            var cutOff = new Detection("potted plant", 1f, 100f, 0f, 300f, 500f);

            Assert.AreEqual(BoxPointKind.BottomCentre, BoxPoint.KindFor(cutOff, BoxPointKind.BottomCentreSetBack, 500, 0.01f));
        }

        [Test]
        public void BoxWithinTheMarginOfTheBottomEdge_CountsAsTouchingIt()
        {
            // One hundredth of 500 pixels is 5 pixels.
            var threePixelsShort = new Detection("potted plant", 1f, 100f, 50f, 300f, 497f);
            var tenPixelsShort = new Detection("potted plant", 1f, 100f, 50f, 300f, 490f);

            Assert.IsTrue(BoxPoint.TouchesBottomEdge(threePixelsShort, 500, 0.01f));
            Assert.IsFalse(BoxPoint.TouchesBottomEdge(tenPixelsShort, 500, 0.01f));
        }

        [Test]
        public void BoxAtTheTopEdgeOnly_IsStillSetBack()
        {
            var cutAtTheTop = new Detection("potted plant", 1f, 100f, 0f, 300f, 250f);

            Assert.AreEqual(BoxPointKind.BottomCentreSetBack, BoxPoint.KindFor(cutAtTheTop, BoxPointKind.BottomCentreSetBack, 500, 0.01f));
        }

        [Test]
        public void OtherBoxPoints_StayAsAsked_AtTheBottomEdge()
        {
            var cutOff = new Detection("potted plant", 1f, 100f, 0f, 300f, 500f);

            Assert.AreEqual(BoxPointKind.Centre, BoxPoint.KindFor(cutOff, BoxPointKind.Centre, 500, 0.01f));
            Assert.AreEqual(BoxPointKind.BottomCentre, BoxPoint.KindFor(cutOff, BoxPointKind.BottomCentre, 500, 0.01f));
        }

        [Test]
        public void SetBack_MovesHalfTheDepthAwayFromTheCamera_AtTheSameHeight()
        {
            var cameraPosition = new Vector3(0f, 1.5f, 0f);
            var frontFoot = new Vector3(3f, 0.74f, 4f);

            Vector3 moved = BoxPoint.SetBack(frontFoot, cameraPosition, 0.5f);

            // On the floor plan the foot is 5 m from the camera, along (0.6, 0.8).
            Assert.AreEqual(3.15f, moved.x, 0.0001f);
            Assert.AreEqual(0.74f, moved.y, 0.0001f);
            Assert.AreEqual(4.2f, moved.z, 0.0001f);
        }

        [Test]
        public void SetBack_SeenFromStraightAbove_StaysWhereItIs()
        {
            var frontFoot = new Vector3(3f, 0.74f, 4f);

            Assert.AreEqual(frontFoot, BoxPoint.SetBack(frontFoot, new Vector3(3f, 2.5f, 4f), 0.5f));
        }
    }
}
