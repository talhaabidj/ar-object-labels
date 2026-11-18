using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class BoxOverlapTests
    {
        static Detection Box(string className, float score, float left, float top, float right, float bottom)
        {
            return new Detection(className, score, left, top, right, bottom);
        }

        [Test]
        public void SameBox_SharesEverything()
        {
            Detection box = Box("laptop", 0.9f, 10f, 10f, 110f, 60f);

            Assert.AreEqual(1f, BoxOverlap.Share(box, box), 0.0001f);
        }

        [Test]
        public void BoxesApart_ShareNothing()
        {
            Assert.AreEqual(0f, BoxOverlap.Share(Box("a", 1f, 0f, 0f, 10f, 10f), Box("a", 1f, 20f, 0f, 30f, 10f)));
        }

        [Test]
        public void BoxesThatOnlyTouch_ShareNothing()
        {
            Assert.AreEqual(0f, BoxOverlap.Share(Box("a", 1f, 0f, 0f, 10f, 10f), Box("a", 1f, 10f, 0f, 20f, 10f)));
        }

        [Test]
        public void HalfOverlap_IsAThirdOfTheJointArea()
        {
            // Two 10 x 10 boxes, shifted by 5: they share 50 of a joint area of 150.
            float share = BoxOverlap.Share(Box("a", 1f, 0f, 0f, 10f, 10f), Box("a", 1f, 5f, 0f, 15f, 10f));

            Assert.AreEqual(1f / 3f, share, 0.0001f);
        }

        [Test]
        public void TwoBoxesOnOneObject_OnlyTheSurerStays()
        {
            var boxes = new List<Detection>
            {
                Box("laptop", 0.70f, 12f, 11f, 111f, 62f),
                Box("laptop", 0.92f, 10f, 10f, 110f, 60f)
            };

            BoxOverlap.KeepTheBest(boxes, 0.45f);

            Assert.AreEqual(1, boxes.Count);
            Assert.AreEqual(0.92f, boxes[0].Score);
        }

        [Test]
        public void TwoObjectsOfOneClass_BothStay_HighestScoreFirst()
        {
            var boxes = new List<Detection>
            {
                Box("chair", 0.6f, 300f, 10f, 400f, 200f),
                Box("chair", 0.8f, 10f, 10f, 110f, 200f)
            };

            BoxOverlap.KeepTheBest(boxes, 0.45f);

            Assert.AreEqual(2, boxes.Count);
            Assert.AreEqual(0.8f, boxes[0].Score);
        }

        [Test]
        public void SamePlaceButAnotherClass_BothStay()
        {
            var boxes = new List<Detection>
            {
                Box("vase", 0.8f, 10f, 10f, 110f, 200f),
                Box("potted plant", 0.7f, 10f, 10f, 110f, 200f)
            };

            BoxOverlap.KeepTheBest(boxes, 0.45f);

            Assert.AreEqual(2, boxes.Count);
        }

        [Test]
        public void BoxThatSharesOnlyWithARemovedBox_Stays()
        {
            // The middle box is removed by the first. The third shares with the middle one only.
            var boxes = new List<Detection>
            {
                Box("cup", 0.9f, 0f, 0f, 10f, 10f),
                Box("cup", 0.8f, 4f, 0f, 14f, 10f),
                Box("cup", 0.7f, 8f, 0f, 18f, 10f)
            };

            BoxOverlap.KeepTheBest(boxes, 0.4f);

            Assert.AreEqual(2, boxes.Count);
            Assert.AreEqual(0.9f, boxes[0].Score);
            Assert.AreEqual(0.7f, boxes[1].Score);
        }

        [Test]
        public void NoBoxes_NothingHappens()
        {
            var boxes = new List<Detection>();

            BoxOverlap.KeepTheBest(boxes, 0.45f);

            CollectionAssert.IsEmpty(boxes);
        }
    }
}
