using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ConfirmOverRunsTests
    {
        const float SameObject = 0.5f;
        static readonly ModelSettings rules = new ModelSettings();
        static readonly Vector3 onTheTable = new Vector3(1.7f, 0.9f, 3.7f);
        static readonly Vector3 onTheDesk = new Vector3(0.4f, 0.8f, 2.0f);

        ConfirmOverRuns confirm;

        [SetUp]
        public void StartWithNothingSeen()
        {
            confirm = new ConfirmOverRuns();
        }

        // One run that finds one object. Returns whether it may have a label.
        bool Run(string name, float score, Vector3 place)
        {
            var mayShow = new List<bool>();
            confirm.TakeRun(new List<string> { name }, new List<float> { score }, new List<Vector3> { place }, rules, SameObject, mayShow);
            return mayShow[0];
        }

        void RunThatFindsNothing()
        {
            confirm.TakeRun(new List<string>(), new List<float>(), new List<Vector3>(), rules, SameObject, new List<bool>());
        }

        [Test]
        public void TheSettingsAsTheyCome_AreThreeRuns_045ToAppear_030ToStay()
        {
            Assert.AreEqual(3, rules.runsToAppear);
            Assert.AreEqual(0.45f, rules.minScore);
            Assert.AreEqual(0.30f, rules.stayScore);
        }

        [Test]
        public void ObjectFoundThreeRunsInARow_GetsItsLabelOnTheThird()
        {
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsTrue(Run("laptop", 0.9f, onTheTable));
        }

        [Test]
        public void PlaceThatWandersALittle_IsStillTheSameObject()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable + new Vector3(0.1f, 0f, -0.1f));

            Assert.IsTrue(Run("laptop", 0.9f, onTheTable + new Vector3(-0.05f, 0f, 0.1f)));
        }

        [Test]
        public void RunWithoutIt_StartsTheCountAgain()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);
            RunThatFindsNothing();

            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsTrue(Run("laptop", 0.9f, onTheTable));
        }

        [Test]
        public void RunWithTooLowAScore_StartsTheCountAgain()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            Assert.IsFalse(Run("laptop", 0.40f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
        }

        [Test]
        public void ScoreBetweenTheTwoLimits_NeverBringsALabel()
        {
            for (int run = 0; run < 6; run++)
            {
                Assert.IsFalse(Run("vase", 0.40f, onTheTable));
            }
        }

        [Test]
        public void ObjectWithALabel_KeepsItWhileItsScoreIs030OrMore()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            Assert.IsTrue(Run("laptop", 0.40f, onTheTable));
            Assert.IsTrue(Run("laptop", 0.30f, onTheTable));
        }

        [Test]
        public void ObjectWithALabel_LosesItUnder030_AndMustBeFoundThreeTimesAgain()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            Assert.IsFalse(Run("laptop", 0.29f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
            Assert.IsTrue(Run("laptop", 0.9f, onTheTable));
        }

        [Test]
        public void SameClassSomewhereElse_IsAnotherObject_AndCountsFromOne()
        {
            Run("keyboard", 0.9f, onTheTable);
            Run("keyboard", 0.9f, onTheTable);

            Assert.IsFalse(Run("keyboard", 0.9f, onTheDesk));
        }

        [Test]
        public void AnotherClassInTheSamePlace_CountsOnItsOwn()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            Assert.IsFalse(Run("keyboard", 0.9f, onTheTable));
        }

        [Test]
        public void TwoObjectsInOneRun_AreCountedSeparately()
        {
            var mayShow = new List<bool>();
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            confirm.TakeRun(new List<string> { "tv", "laptop" }, new List<float> { 0.9f, 0.9f },
                new List<Vector3> { onTheDesk, onTheTable }, rules, SameObject, mayShow);

            CollectionAssert.AreEqual(new[] { false, true }, mayShow);
        }

        [Test]
        public void AfterForget_EverythingStartsAgain()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);
            confirm.Forget();

            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
        }
    }
}
