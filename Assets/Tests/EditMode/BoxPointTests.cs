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
    }
}
