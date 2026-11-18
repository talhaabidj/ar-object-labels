using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ObjectDepthTests
    {
        static readonly List<ClassDepth> table = new List<ClassDepth>
        {
            new ClassDepth { className = "keyboard", depth = 0.14f },
            new ClassDepth { className = "chair", depth = 0.50f }
        };

        [Test]
        public void ClassInTheTable_HasItsUsualDepth()
        {
            Assert.IsTrue(ObjectDepth.TryUsual("chair", table, out float depth));
            Assert.AreEqual(0.50f, depth);
        }

        [Test]
        public void ClassNotInTheTable_HasNone()
        {
            Assert.IsFalse(ObjectDepth.TryUsual("pen", table, out _));
        }

        [Test]
        public void DefaultTable_KnowsTheClassesOfTheTestRoom_AndNoPen()
        {
            List<ClassDepth> usual = new DetectionSettings().usualDepths;

            foreach (string className in new[] { "laptop", "keyboard", "mouse", "tv", "cup", "bottle", "vase", "potted plant", "chair", "clock" })
            {
                Assert.IsTrue(ObjectDepth.TryUsual(className, usual, out float depth), className);
                Assert.Greater(depth, 0f, className);
            }
            Assert.IsFalse(ObjectDepth.TryUsual("pen", usual, out _));
        }

        [Test]
        public void SeenFromTheSide_TheHeightIsAsSeen()
        {
            Assert.AreEqual(0.33f, ObjectDepth.Height(0.33f, 0.24f, 0f), 0.0001f);
        }

        [Test]
        public void FlatObjectSeenFromAbove_GetsItsRealHeightBack()
        {
            // A keyboard 3 cm high and 14 cm deep, looked down on at 30 degrees, shows about 9.6 cm high.
            float angle = 30f * Mathf.Deg2Rad;
            float asSeen = 0.03f * Mathf.Cos(angle) + 0.14f * Mathf.Sin(angle);

            Assert.AreEqual(0.03f, ObjectDepth.Height(asSeen, 0.14f, 30f), 0.0001f);
        }

        [Test]
        public void TallObjectSeenFromAbove_GetsItsRealHeightBack()
        {
            float angle = 20f * Mathf.Deg2Rad;
            float asSeen = 0.90f * Mathf.Cos(angle) + 0.50f * Mathf.Sin(angle);

            Assert.AreEqual(0.90f, ObjectDepth.Height(asSeen, 0.50f, 20f), 0.0001f);
        }

        [Test]
        public void LookedUpAt_TheHeightIsAsSeen()
        {
            Assert.AreEqual(0.30f, ObjectDepth.Height(0.30f, 0.05f, -25f), 0.0001f);
        }

        [Test]
        public void BoxLowerThanTheDepthAlone_GivesNoHeight_NotLessThanNone()
        {
            Assert.AreEqual(0f, ObjectDepth.Height(0.02f, 0.50f, 40f));
        }

        [Test]
        public void StraightDown_CountsAsTheSteepestAngleTheRuleTakes()
        {
            Assert.AreEqual(ObjectDepth.Height(0.5f, 0.1f, 75f), ObjectDepth.Height(0.5f, 0.1f, 90f), 0.0001f);
            Assert.IsFalse(float.IsInfinity(ObjectDepth.Height(0.5f, 0.1f, 90f)));
        }
    }
}
