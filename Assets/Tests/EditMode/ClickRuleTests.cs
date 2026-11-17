using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ClickRuleTests
    {
        const float GameViewHeight = 473f;
        const float Share = 0.025f;   // 11.8 pixels at this height

        [TestCase(0f)]
        [TestCase(3f)]
        [TestCase(5f)]
        [TestCase(11f)]
        public void TapThatSlipsAFewPixels_IsStillAClick(float movedPixels)
        {
            Assert.IsTrue(ClickRule.IsClick(movedPixels, GameViewHeight, Share));
        }

        [TestCase(12f)]
        [TestCase(40f)]
        [TestCase(300f)]
        public void PressThatMovesFurther_IsADrag(float movedPixels)
        {
            Assert.IsFalse(ClickRule.IsClick(movedPixels, GameViewHeight, Share));
        }

        [Test]
        public void TheLimitGrowsWithTheScreen()
        {
            Assert.IsFalse(ClickRule.IsClick(20f, GameViewHeight, Share));
            Assert.IsTrue(ClickRule.IsClick(20f, 1080f, Share));
        }
    }
}
