using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ModelPacingTests
    {
        [Test]
        public void TenRunsASecond_ARunIsDueATenthOfASecondAfterTheLast()
        {
            Assert.IsFalse(ModelPacing.RunIsDue(5.09f, 5.00f, 10f));
            Assert.IsTrue(ModelPacing.RunIsDue(5.11f, 5.00f, 10f));
        }

        [Test]
        public void FiveRunsASecond_WaitsTwiceAsLong()
        {
            Assert.IsFalse(ModelPacing.RunIsDue(5.11f, 5.00f, 5f));
            Assert.IsTrue(ModelPacing.RunIsDue(5.21f, 5.00f, 5f));
        }

        [Test]
        public void NoRunsWanted_NoRunIsEverDue()
        {
            Assert.IsFalse(ModelPacing.RunIsDue(1000f, 0f, 0f));
        }

        [Test]
        public void AskedInTheFrameBefore_WasNotLeftAlone()
        {
            Assert.IsFalse(ModelPacing.WasLeftAlone(100, 101));
        }

        [Test]
        public void AskedTwiceInOneFrame_WasNotLeftAlone()
        {
            Assert.IsFalse(ModelPacing.WasLeftAlone(100, 100));
        }

        [Test]
        public void OneFrameMissed_WasLeftAlone()
        {
            Assert.IsTrue(ModelPacing.WasLeftAlone(100, 102));
        }

        [Test]
        public void NeverAskedBefore_CountsAsLeftAlone_EvenInTheFirstFrame()
        {
            Assert.IsTrue(ModelPacing.WasLeftAlone(ModelPacing.NeverAsked, 0));
            Assert.IsTrue(ModelPacing.WasLeftAlone(ModelPacing.NeverAsked, 1));
        }

        [Test]
        public void TheSettingsFile_SaysTenRunsASecond_FortyLayersAFrame()
        {
            ModelSettings file = SettingsFile.Load().model;

            Assert.AreEqual(10f, file.runsPerSecond);
            Assert.AreEqual(40, file.layersPerFrame);
        }
    }
}
