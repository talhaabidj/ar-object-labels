using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class CameraStillTests
    {
        const float Frame = 0.1f;
        const float MoveLimit = 0.05f;
        const float TurnLimit = 5f;

        [Test]
        public void BeforeAnyMove_CountsAsStillLongEnough()
        {
            var still = new CameraStill();
            still.Take(Vector3.zero, Quaternion.identity, Frame, MoveLimit, TurnLimit);

            Assert.IsTrue(float.IsPositiveInfinity(still.SecondsStill));
        }

        [Test]
        public void StepFasterThanTheLimit_StartsTheCountAgain()
        {
            var still = new CameraStill();
            still.Take(Vector3.zero, Quaternion.identity, Frame, MoveLimit, TurnLimit);

            // 2 cm in a tenth of a second is 0.2 m a second.
            still.Take(new Vector3(0.02f, 0f, 0f), Quaternion.identity, Frame, MoveLimit, TurnLimit);

            Assert.AreEqual(0f, still.SecondsStill);
        }

        [Test]
        public void TurnFasterThanTheLimit_StartsTheCountAgain()
        {
            var still = new CameraStill();
            still.Take(Vector3.zero, Quaternion.identity, Frame, MoveLimit, TurnLimit);

            // 1 degree in a tenth of a second is 10 degrees a second.
            still.Take(Vector3.zero, Quaternion.Euler(0f, 1f, 0f), Frame, MoveLimit, TurnLimit);

            Assert.AreEqual(0f, still.SecondsStill);
        }

        [Test]
        public void SlowDriftUnderTheLimits_CountsAsStill_AndAddsUp()
        {
            var still = new CameraStill();
            still.Take(Vector3.zero, Quaternion.identity, Frame, MoveLimit, TurnLimit);
            still.Take(new Vector3(0.02f, 0f, 0f), Quaternion.identity, Frame, MoveLimit, TurnLimit);

            for (int frame = 1; frame <= 5; frame++)
            {
                // 3 mm and 0.3 degrees a frame: 0.03 m and 3 degrees a second.
                still.Take(new Vector3(0.02f + 0.003f * frame, 0f, 0f), Quaternion.Euler(0f, 0.3f * frame, 0f), Frame, MoveLimit, TurnLimit);
            }

            Assert.AreEqual(0.5f, still.SecondsStill, 0.0001f);
        }

        [Test]
        public void CameraThatHasNotLeftItsStart_HasNotMoved()
        {
            Assert.IsFalse(CameraStill.HasLeft(Vector3.zero, Quaternion.identity, new Vector3(0.05f, 0f, 0f), Quaternion.Euler(0f, 3f, 0f), 0.1f, 5f));
            Assert.IsTrue(CameraStill.HasLeft(Vector3.zero, Quaternion.identity, new Vector3(0.2f, 0f, 0f), Quaternion.identity, 0.1f, 5f));
            Assert.IsTrue(CameraStill.HasLeft(Vector3.zero, Quaternion.identity, Vector3.zero, Quaternion.Euler(0f, 8f, 0f), 0.1f, 5f));
        }
    }
}
