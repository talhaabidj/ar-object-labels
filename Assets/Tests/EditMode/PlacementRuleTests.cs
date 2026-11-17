using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class PlacementRuleTests
    {
        [Test]
        public void PressOnUpwardSurface_WithNoCube_PlacesCube()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, cubeIsCarried: false, pressIsOnCube: false,
                hitSurface: true, surfaceFacesUp: true);

            Assert.AreEqual(PressOutcome.PlaceCube, outcome);
        }

        [Test]
        public void PressOnWall_WithNoCube_ShowsWallHint()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, cubeIsCarried: false, pressIsOnCube: false,
                hitSurface: true, surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.ShowWallHint, outcome);
        }

        [Test]
        public void PressThatHitsNothing_IsIgnored()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, cubeIsCarried: false, pressIsOnCube: false,
                hitSurface: false, surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.Ignore, outcome);
        }

        [Test]
        public void PressOverUi_IsIgnored_WhateverIsBehindIt(
            [Values] bool cubeExists, [Values] bool cubeIsCarried, [Values] bool pressIsOnCube,
            [Values] bool hitSurface, [Values] bool surfaceFacesUp)
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: true, cubeExists, cubeIsCarried, pressIsOnCube, hitSurface, surfaceFacesUp);

            Assert.AreEqual(PressOutcome.Ignore, outcome);
        }

        [Test]
        public void PressOnSurface_WhenCubeExists_NeverPlacesSecondCube([Values] bool surfaceFacesUp)
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, cubeIsCarried: false, pressIsOnCube: false, hitSurface: true,
                surfaceFacesUp);

            Assert.AreEqual(PressOutcome.Deselect, outcome);
        }

        [Test]
        public void PressOnCube_SelectsIt()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, cubeIsCarried: false, pressIsOnCube: true, hitSurface: true,
                surfaceFacesUp: true);

            Assert.AreEqual(PressOutcome.SelectCube, outcome);
        }

        [Test]
        public void ClickOnUpwardSurface_WhileCarrying_PutsCubeDown([Values] bool pressIsOnCube)
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, cubeIsCarried: true, pressIsOnCube, hitSurface: true,
                surfaceFacesUp: true);

            Assert.AreEqual(PressOutcome.PutDownCube, outcome);
        }

        [Test]
        public void ClickOnWall_WhileCarrying_ShowsWallHint()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, cubeIsCarried: true, pressIsOnCube: true, hitSurface: true,
                surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.ShowWallHint, outcome);
        }

        [Test]
        public void ClickThatHitsNothing_WhileCarrying_IsIgnored()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, cubeIsCarried: true, pressIsOnCube: true, hitSurface: false,
                surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.Ignore, outcome);
        }
    }
}
