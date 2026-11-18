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
        public void WithTheSettingsAsTheyCome_TenRunsASecond()
        {
            Assert.AreEqual(10f, new ModelSettings().runsPerSecond);
        }
    }
}
