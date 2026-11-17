using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    public class HintRuleTests
    {
        static HintFacts Running()
        {
            return new HintFacts { SessionReady = true, Tracking = true, SecondsSinceStart = 10f };
        }

        [Test]
        public void BeforeSessionIsReady_ShowsStarting_ThenNotRunning()
        {
            var facts = new HintFacts { SessionReady = false, SecondsSinceStart = 1f };
            Assert.AreEqual(Hint.Starting, HintRule.Pick(facts));

            facts.SecondsSinceStart = HintRule.SecondsBeforeNotRunning + 1f;
            Assert.AreEqual(Hint.ArNotRunning, HintRule.Pick(facts));
        }

        [Test]
        public void TrackingLost_IsShown_OnlyAfterTheStart()
        {
            HintFacts facts = Running();
            facts.Tracking = false;
            Assert.AreEqual(Hint.TrackingLost, HintRule.Pick(facts));

            facts.SecondsSinceStart = 1f;
            Assert.AreEqual(Hint.Starting, HintRule.Pick(facts));
        }

        [Test]
        public void NoSurfaceYet_AsksToLookAround()
        {
            Assert.AreEqual(Hint.FindSurface, HintRule.Pick(Running()));
        }

        [Test]
        public void SurfaceFound_AsksToPlaceCube()
        {
            HintFacts facts = Running();
            facts.AnySurfaceFound = true;

            Assert.AreEqual(Hint.PlaceCube, HintRule.Pick(facts));
        }

        [Test]
        public void WallClick_WinsOverPlacingHints()
        {
            HintFacts facts = Running();
            facts.AnySurfaceFound = true;
            facts.WallWasClicked = true;

            Assert.AreEqual(Hint.UseFlatSurface, HintRule.Pick(facts));
        }

        [Test]
        public void CubePlaced_AsksToSelect_ThenToDrag()
        {
            HintFacts facts = Running();
            facts.AnySurfaceFound = true;
            facts.CubeExists = true;
            Assert.AreEqual(Hint.SelectCube, HintRule.Pick(facts));

            facts.CubeSelected = true;
            Assert.AreEqual(Hint.DragCube, HintRule.Pick(facts));
        }

        [Test]
        public void CubeCarried_SaysHowToPutItDown()
        {
            HintFacts facts = Running();
            facts.AnySurfaceFound = true;
            facts.CubeExists = true;
            facts.CubeSelected = true;
            facts.CubeCarried = true;

            Assert.AreEqual(Hint.CarryCube, HintRule.Pick(facts));
        }

        [Test]
        public void WallClick_WhileCarrying_WinsUntilItsTimeIsUp()
        {
            HintFacts facts = Running();
            facts.AnySurfaceFound = true;
            facts.CubeExists = true;
            facts.CubeCarried = true;
            facts.WallWasClicked = true;
            Assert.AreEqual(Hint.UseFlatSurface, HintRule.Pick(facts));

            facts.WallWasClicked = false;
            Assert.AreEqual(Hint.CarryCube, HintRule.Pick(facts));
        }
    }
}
