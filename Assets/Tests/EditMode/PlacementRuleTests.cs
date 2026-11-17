using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    public class PlacementRuleTests
    {
        [Test]
        public void PressOnUpwardSurface_WithNoCube_PlacesCube()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, pressIsOnCube: false, hitSurface: true, surfaceFacesUp: true);

            Assert.AreEqual(PressOutcome.PlaceCube, outcome);
        }

        [Test]
        public void PressOnWall_WithNoCube_ShowsWallHint()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, pressIsOnCube: false, hitSurface: true, surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.ShowWallHint, outcome);
        }

        [Test]
        public void PressThatHitsNothing_IsIgnored()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: false, pressIsOnCube: false, hitSurface: false, surfaceFacesUp: false);

            Assert.AreEqual(PressOutcome.Ignore, outcome);
        }

        [Test]
        public void PressOverUi_IsIgnored_WhateverIsBehindIt(
            [Values] bool cubeExists, [Values] bool pressIsOnCube, [Values] bool hitSurface, [Values] bool surfaceFacesUp)
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: true, cubeExists, pressIsOnCube, hitSurface, surfaceFacesUp);

            Assert.AreEqual(PressOutcome.Ignore, outcome);
        }

        [Test]
        public void PressOnSurface_WhenCubeExists_NeverPlacesSecondCube([Values] bool surfaceFacesUp)
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, pressIsOnCube: false, hitSurface: true, surfaceFacesUp);

            Assert.AreEqual(PressOutcome.Deselect, outcome);
        }

        [Test]
        public void PressOnCube_SelectsIt()
        {
            PressOutcome outcome = PlacementRule.Decide(
                pointerOverUi: false, cubeExists: true, pressIsOnCube: true, hitSurface: true, surfaceFacesUp: true);

            Assert.AreEqual(PressOutcome.SelectCube, outcome);
        }
    }
}
