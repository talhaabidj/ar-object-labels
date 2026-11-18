using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelChoiceTests
    {
        // Sizes on one step differ by less than a quarter.
        const float Share = 0.25f;

        [Test]
        public void FewerThanTheLimit_AllShown_BiggestFirst()
        {
            var sizes = new List<float> { 0.002f, 0.050f, 0.010f };
            var scores = new List<float> { 0.9f, 0.9f, 0.9f };

            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, LabelChoice.Pick(sizes, scores, 6, Share));
        }

        [Test]
        public void MoreThanTheLimit_TheSmallestAreLeftOut_WhateverTheirScore()
        {
            var sizes = new List<float> { 0.001f, 0.080f, 0.004f, 0.030f };
            var scores = new List<float> { 0.99f, 0.5f, 0.5f, 0.5f };

            CollectionAssert.AreEqual(new[] { 1, 3 }, LabelChoice.Pick(sizes, scores, 2, Share));
        }

        [Test]
        public void AboutEquallyBig_TheHigherScoreWins()
        {
            // 0.0100, 0.0104 and 0.0108 are on one step: the step runs from about 0.0092 to 0.0115.
            var sizes = new List<float> { 0.0108f, 0.0100f, 0.0104f };
            var scores = new List<float> { 0.55f, 0.90f, 0.70f };

            CollectionAssert.AreEqual(new[] { 1, 2 }, LabelChoice.Pick(sizes, scores, 2, Share));
        }

        [Test]
        public void ClearlyBigger_WinsOverAHigherScore()
        {
            var sizes = new List<float> { 0.010f, 0.020f };
            var scores = new List<float> { 0.95f, 0.40f };

            CollectionAssert.AreEqual(new[] { 1 }, LabelChoice.Pick(sizes, scores, 1, Share));
        }

        [Test]
        public void SameScoreAndStep_TheBiggerWins()
        {
            var sizes = new List<float> { 0.0100f, 0.0108f };
            var scores = new List<float> { 0.8f, 0.8f };

            CollectionAssert.AreEqual(new[] { 1 }, LabelChoice.Pick(sizes, scores, 1, Share));
        }

        [Test]
        public void SameSizeAndScore_KeepTheOrderTheyCameIn()
        {
            var sizes = new List<float> { 0.01f, 0.01f, 0.01f };
            var scores = new List<float> { 1f, 1f, 1f };

            CollectionAssert.AreEqual(new[] { 0, 1 }, LabelChoice.Pick(sizes, scores, 2, Share));
        }

        [Test]
        public void ObjectWithNoSizeInThePicture_ComesLast()
        {
            var sizes = new List<float> { 0f, 0.0001f };
            var scores = new List<float> { 1f, 0.3f };

            CollectionAssert.AreEqual(new[] { 1, 0 }, LabelChoice.Pick(sizes, scores, 6, Share));
        }

        [Test]
        public void LimitOfZero_NothingShown()
        {
            var sizes = new List<float> { 0.01f, 0.02f };
            var scores = new List<float> { 0.9f, 0.9f };

            CollectionAssert.IsEmpty(LabelChoice.Pick(sizes, scores, 0, Share));
        }

        [Test]
        public void NoLabels_NothingShown()
        {
            CollectionAssert.IsEmpty(LabelChoice.Pick(new List<float>(), new List<float>(), 6, Share));
        }
    }
}
