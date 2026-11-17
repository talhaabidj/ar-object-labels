using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelChoiceTests
    {
        const float Step = 0.5f;

        [Test]
        public void FewerThanTheLimit_AllShown_NearestFirst()
        {
            var distances = new List<float> { 2.2f, 0.7f, 1.4f };
            var scores = new List<float> { 0.9f, 0.9f, 0.9f };

            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, LabelChoice.Pick(distances, scores, 6, Step));
        }

        [Test]
        public void MoreThanTheLimit_TheFarthestAreLeftOut()
        {
            var distances = new List<float> { 3.1f, 0.6f, 2.2f, 1.2f };
            var scores = new List<float> { 0.99f, 0.5f, 0.5f, 0.5f };

            CollectionAssert.AreEqual(new[] { 1, 3 }, LabelChoice.Pick(distances, scores, 2, Step));
        }

        [Test]
        public void AboutEquallyNear_TheHigherScoreWins()
        {
            var distances = new List<float> { 1.10f, 1.30f, 1.20f };
            var scores = new List<float> { 0.55f, 0.90f, 0.70f };

            CollectionAssert.AreEqual(new[] { 1, 2 }, LabelChoice.Pick(distances, scores, 2, Step));
        }

        [Test]
        public void SameScoreAndBand_TheNearerWins()
        {
            var distances = new List<float> { 1.40f, 1.05f };
            var scores = new List<float> { 0.8f, 0.8f };

            CollectionAssert.AreEqual(new[] { 1 }, LabelChoice.Pick(distances, scores, 1, Step));
        }

        [Test]
        public void LimitOfZero_NothingShown()
        {
            var distances = new List<float> { 1f, 2f };
            var scores = new List<float> { 0.9f, 0.9f };

            CollectionAssert.IsEmpty(LabelChoice.Pick(distances, scores, 0, Step));
        }

        [Test]
        public void NoLabels_NothingShown()
        {
            CollectionAssert.IsEmpty(LabelChoice.Pick(new List<float>(), new List<float>(), 6, Step));
        }
    }
}
