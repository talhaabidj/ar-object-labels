using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelSizingTests
    {
        const float Near = 0.5f;
        const float Far = 3f;

        [TestCase(0.5f)]
        [TestCase(1f)]
        [TestCase(2f)]
        [TestCase(3f)]
        public void BetweenTheLimits_ScaleFollowsDistance(float distance)
        {
            Assert.AreEqual(distance, LabelSizing.Scale(distance, Near, Far), 0.0001f);
        }

        [TestCase(0.1f)]
        [TestCase(0.49f)]
        public void NearerThanTheNearLimit_StopsShrinking(float distance)
        {
            Assert.AreEqual(Near, LabelSizing.Scale(distance, Near, Far), 0.0001f);
        }

        [TestCase(3.01f)]
        [TestCase(10f)]
        public void FartherThanTheFarLimit_StopsGrowing(float distance)
        {
            Assert.AreEqual(Far, LabelSizing.Scale(distance, Near, Far), 0.0001f);
        }

        [Test]
        public void BetweenTheLimits_TextLooksTheSameSize()
        {
            const float heightAtOneMetre = 0.0122f;
            float atOne = LabelSizing.ViewingAngle(heightAtOneMetre * LabelSizing.Scale(1f, Near, Far), 1f);
            float atThree = LabelSizing.ViewingAngle(heightAtOneMetre * LabelSizing.Scale(3f, Near, Far), 3f);

            Assert.AreEqual(atOne, atThree, 0.001f);
            Assert.AreEqual(0.7f, atOne, 0.01f);
        }

        [Test]
        public void BeyondTheFarLimit_TextLooksSmaller()
        {
            const float heightAtOneMetre = 0.0122f;
            float atThree = LabelSizing.ViewingAngle(heightAtOneMetre * LabelSizing.Scale(3f, Near, Far), 3f);
            float atSix = LabelSizing.ViewingAngle(heightAtOneMetre * LabelSizing.Scale(6f, Near, Far), 6f);

            Assert.AreEqual(atThree / 2f, atSix, 0.001f);
        }

        [Test]
        public void NarrowerThanTheWidthLimit_SizeIsKept()
        {
            Assert.AreEqual(2f, LabelSizing.CapToWidth(2f, 0.10f, 0.30f), 0.0001f);
        }

        [Test]
        public void WiderThanTheWidthLimit_SizeIsHeldBackToTheLimit()
        {
            float scale = LabelSizing.CapToWidth(3.5f, 0.10f, 0.30f);

            Assert.AreEqual(0.30f, 0.10f * scale, 0.0001f);
        }

        [Test]
        public void NoWidthLimit_SizeIsKept()
        {
            Assert.AreEqual(3.5f, LabelSizing.CapToWidth(3.5f, 0.10f, 0f), 0.0001f);
        }
    }
}
