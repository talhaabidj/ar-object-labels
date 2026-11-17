using System;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(10000)]
    public class TimeLimitTests
    {
        [Test]
        public void RuleThatEnds_GivesItsResult()
        {
            Assert.AreEqual(3, TimeLimit.Run(() => 1 + 2));
        }

        [Test]
        public void RuleThatFails_PassesItsErrorOn()
        {
            Assert.Throws<InvalidOperationException>(() => TimeLimit.Run<int>(() => throw new InvalidOperationException()));
        }

        [Test]
        public void RuleThatNeverEnds_FailsInsteadOfFreezing()
        {
            var error = Assert.Throws<TimeoutException>(() => TimeLimit.Run<int>(() =>
            {
                while (true)
                {
                }
            }));

            StringAssert.DoesNotContain("still running", error.Message);
        }
    }
}
