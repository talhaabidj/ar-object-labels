using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelSizingTests
    {
        const float Near = 0.1f;

        [TestCase(0.1f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        [TestCase(3f)]
        [TestCase(4.5f)]
        public void ScaleFollowsDepth_AtEveryDistance(float depth)
        {
            Assert.AreEqual(depth, LabelSizing.Scale(depth, Near), 0.0001f);
        }

        [TestCase(0.05f)]
        [TestCase(-1f)]
        public void NearerThanTheNearLimit_StopsShrinking(float depth)
        {
            Assert.AreEqual(Near, LabelSizing.Scale(depth, Near), 0.0001f);
        }

        [Test]
        public void Depth_IsMeasuredAlongTheWayTheCameraLooks_NotStraightToThePoint()
        {
            // 2 m ahead and 1 m to the side: 2.24 m away, but 2 m deep.
            Assert.AreEqual(2f, LabelSizing.Depth(new Vector3(1f, 0f, 2f), Vector3.zero, Vector3.forward), 0.0001f);
        }

        [Test]
        public void Depth_OfAPointBehindTheCamera_IsNegative()
        {
            Assert.Less(LabelSizing.Depth(new Vector3(0f, 0f, -1f), Vector3.zero, Vector3.forward), 0f);
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
