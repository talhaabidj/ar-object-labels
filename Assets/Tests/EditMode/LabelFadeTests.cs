using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelFadeTests
    {
        const float Start = 3.5f;
        const float End = 4.5f;

        [TestCase(0.3f)]
        [TestCase(2f)]
        [TestCase(3.5f)]
        public void UpToTheStart_FullyDrawn(float distance)
        {
            Assert.AreEqual(1f, LabelFade.Opacity(distance, Start, End), 0.0001f);
        }

        [Test]
        public void HalfWayThroughTheFade_HalfDrawn()
        {
            Assert.AreEqual(0.5f, LabelFade.Opacity(4f, Start, End), 0.0001f);
        }

        [TestCase(4.5f)]
        [TestCase(9f)]
        public void FromTheEnd_Gone(float distance)
        {
            Assert.AreEqual(0f, LabelFade.Opacity(distance, Start, End), 0.0001f);
        }

        [Test]
        public void FurtherAway_NeverClearer()
        {
            float before = 1f;
            for (int step = 0; step <= 60; step++)
            {
                float now = LabelFade.Opacity(step * 0.1f, Start, End);
                Assert.LessOrEqual(now, before);
                before = now;
            }
        }

        [Test]
        public void EndNotAfterStart_SwitchesOffAtTheStart()
        {
            Assert.AreEqual(1f, LabelFade.Opacity(2.9f, 3f, 3f));
            Assert.AreEqual(0f, LabelFade.Opacity(3f, 3f, 3f));
        }
    }
}
