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
        public void TheSettingsAsTheyCome_AreThreeRuns_040ToAppear_030ToStay()
        {
            Assert.AreEqual(3, rules.runsToAppear);
            Assert.AreEqual(0.40f, rules.minScore);
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

            Assert.IsFalse(Run("laptop", 0.35f, onTheTable));
            Assert.IsFalse(Run("laptop", 0.9f, onTheTable));
        }

        [Test]
        public void ScoreBetweenTheTwoLimits_NeverBringsALabel()
        {
            for (int run = 0; run < 6; run++)
            {
                Assert.IsFalse(Run("vase", 0.35f, onTheTable));
            }
        }

        [Test]
        public void ObjectWithALabel_KeepsItWhileItsScoreIs030OrMore()
        {
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);
            Run("laptop", 0.9f, onTheTable);

            Assert.IsTrue(Run("laptop", 0.35f, onTheTable));
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

        // One run that finds one object, with its box cut by the picture's edge or clear of it.
        bool RunAtTheEdge(string name, float score, Vector3 place, bool clearOfEdge)
        {
            var mayShow = new List<bool>();
            confirm.TakeRun(new List<string> { name }, new List<float> { score }, new List<Vector3> { place }, rules, SameObject,
                mayShow, new List<bool> { clearOfEdge });
            return mayShow[0];
        }

        [Test]
        public void BoxCutByTheEdgeInEveryRun_GetsNoLabel()
        {
            for (int run = 0; run < 6; run++)
            {
                Assert.IsFalse(RunAtTheEdge("bicycle", 0.6f, onTheTable, false), "run " + run);
            }
        }

        [Test]
        public void BoxClearOfTheEdgeInOneOfTheThreeRuns_GetsItsLabel()
        {
            RunAtTheEdge("clock", 0.6f, onTheTable, false);
            RunAtTheEdge("clock", 0.6f, onTheTable, true);

            Assert.IsTrue(RunAtTheEdge("clock", 0.6f, onTheTable, false));
        }

        [Test]
        public void ClearRunBeforeTheCountStartedAgain_DoesNotCount()
        {
            RunAtTheEdge("clock", 0.6f, onTheTable, true);
            RunAtTheEdge("clock", 0.2f, onTheTable, true);
            RunAtTheEdge("clock", 0.6f, onTheTable, false);
            RunAtTheEdge("clock", 0.6f, onTheTable, false);

            Assert.IsFalse(RunAtTheEdge("clock", 0.6f, onTheTable, false));
        }

        [Test]
        public void ObjectWithALabel_KeepsIt_WhenItsBoxComesToTheEdge()
        {
            RunAtTheEdge("laptop", 0.9f, onTheTable, true);
            RunAtTheEdge("laptop", 0.9f, onTheTable, true);
            RunAtTheEdge("laptop", 0.9f, onTheTable, true);

            Assert.IsTrue(RunAtTheEdge("laptop", 0.9f, onTheTable, false));
        }

        [Test]
        public void BoxTouchingASideOfThePicture_IsNotClearOfTheEdge()
        {
            // A 1000 x 500 picture with a 1 % margin: 10 pixels at the sides, 5 at the top and bottom.
            Assert.IsTrue(ConfirmOverRuns.IsClearOfEdge(new Detection("clock", 0.6f, 11f, 6f, 989f, 494f), 1000f, 500f, 0.01f));
            Assert.IsFalse(ConfirmOverRuns.IsClearOfEdge(new Detection("clock", 0.6f, 10f, 100f, 200f, 200f), 1000f, 500f, 0.01f), "left");
            Assert.IsFalse(ConfirmOverRuns.IsClearOfEdge(new Detection("clock", 0.6f, 100f, 100f, 990f, 200f), 1000f, 500f, 0.01f), "right");
            Assert.IsFalse(ConfirmOverRuns.IsClearOfEdge(new Detection("clock", 0.6f, 100f, 5f, 200f, 200f), 1000f, 500f, 0.01f), "top");
            Assert.IsFalse(ConfirmOverRuns.IsClearOfEdge(new Detection("clock", 0.6f, 100f, 100f, 200f, 495f), 1000f, 500f, 0.01f), "bottom");
        }
    }
}
