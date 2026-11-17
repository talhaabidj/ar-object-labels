using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelOverlapTests
    {
        const float Margin = 0.01f;
        static readonly Rect label = new Rect(0f, 0f, 0.20f, 0.04f);

        // Run under a time limit: this rule has a loop, and a loop that never ends would freeze the Editor.
        static float Lift(Rect rect, List<Rect> placedBefore)
        {
            return TimeLimit.Run(() => LabelOverlap.LiftToClear(rect, placedBefore, Margin));
        }

        [Test]
        public void NothingInTheWay_NoLift()
        {
            Assert.AreEqual(0f, Lift(label, new List<Rect>()));
        }

        [Test]
        public void AnotherLabelFarAway_NoLift()
        {
            var others = new List<Rect> { new Rect(1f, 0f, 0.20f, 0.04f) };

            Assert.AreEqual(0f, Lift(label, others));
        }

        [Test]
        public void SittingOnAnotherLabel_LiftsJustAboveIt()
        {
            var others = new List<Rect> { new Rect(0.05f, 0.01f, 0.20f, 0.04f) };
            float lift = Lift(label, others);

            Assert.AreEqual(0.01f + 0.04f + Margin, lift, 0.0001f);
        }

        [Test]
        public void TwoLabelsStackedInTheWay_LiftsAboveBoth()
        {
            var others = new List<Rect> { new Rect(0f, 0f, 0.20f, 0.04f), new Rect(0f, 0.05f, 0.20f, 0.04f) };
            float lift = Lift(label, others);

            Assert.AreEqual(0.05f + 0.04f + Margin, lift, 0.0001f);
        }

        [Test]
        public void LabelNotAtZero_StillEndsAndClears()
        {
            var raised = new Rect(0.3f, 0.7f, 0.20f, 0.04f);
            var others = new List<Rect> { new Rect(0.31f, 0.71f, 0.20f, 0.04f), new Rect(0.29f, 0.77f, 0.20f, 0.04f) };
            float lift = Lift(raised, others);

            Assert.AreEqual(0.77f + 0.04f + Margin - 0.7f, lift, 0.0001f);
        }

        [Test]
        public void ManyLabelsInAColumn_LiftsAboveAllOfThem()
        {
            var others = new List<Rect>();
            for (int row = 0; row < 20; row++) others.Add(new Rect(0f, row * 0.05f, 0.20f, 0.04f));
            float lift = Lift(label, others);

            Assert.AreEqual(19 * 0.05f + 0.04f + Margin, lift, 0.0001f);
        }

        [Test]
        public void CloserThanTheMargin_CountsAsInTheWay()
        {
            var others = new List<Rect> { new Rect(0.205f, 0f, 0.20f, 0.04f) };

            Assert.Greater(Lift(label, others), 0f);
        }
    }
}
