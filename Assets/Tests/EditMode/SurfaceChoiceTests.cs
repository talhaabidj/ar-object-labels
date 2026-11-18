using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class SurfaceChoiceTests
    {
        static readonly DetectionSettings rules = new DetectionSettings();

        // A low ray towards a laptop: the table's front edge at 2.50 m, the table top 8 cm further on.
        static readonly SurfaceHit tableEdge = new SurfaceHit(new Vector3(0f, 0.78f, 3.30f), 2.50f, false, true, 0.8f);
        static readonly SurfaceHit tableTop = new SurfaceHit(new Vector3(0f, 0.74f, 3.38f), 2.58f, true, false, 1.2f);
        static readonly SurfaceHit backWall = new SurfaceHit(new Vector3(0f, 0.60f, 4.00f), 3.25f, false, true, 4.0f);

        [Test]
        public void OnlyOneSurface_IsUsed()
        {
            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { tableTop }, rules));
        }

        [Test]
        public void TableEdgeThenTableTop_TheTopIsUsed()
        {
            Assert.AreEqual(1, SurfaceChoice.Pick(new List<SurfaceHit> { tableEdge, tableTop, backWall }, rules));
        }

        [Test]
        public void TableTopFirst_StaysTheTableTop()
        {
            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { tableTop, backWall }, rules));
        }

        [Test]
        public void ClockOnAWall_StaysOnItsWall()
        {
            // A wall is not small, whatever lies behind it.
            var wall = new SurfaceHit(new Vector3(0f, 1.9f, 4.0f), 3.0f, false, true, 4.0f);
            var shelfBehind = new SurfaceHit(new Vector3(0f, 1.9f, 4.1f), 3.1f, true, false, 1.0f);

            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { wall, shelfBehind }, rules));
        }

        [Test]
        public void SmallUprightSurfaceWithNothingBehind_IsUsed()
        {
            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { tableEdge }, rules));
        }

        [Test]
        public void UpwardSurfaceTooFarBehind_IsNotUsed()
        {
            // The back of a chair, and a desk top 60 cm behind it: the object may be the chair itself.
            var chairBack = new SurfaceHit(new Vector3(0f, 0.9f, 2.0f), 2.0f, false, true, 1.1f);
            var deskTop = new SurfaceHit(new Vector3(0f, 0.86f, 2.6f), 2.6f, true, false, 1.6f);

            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { chairBack, deskTop }, rules));
        }

        [Test]
        public void UpwardSurfaceAtAnotherHeight_IsNotUsed()
        {
            // A steep ray through a table's edge to the floor under it.
            var floor = new SurfaceHit(new Vector3(0f, 0f, 3.5f), 2.75f, true, false, 4.0f);

            Assert.AreEqual(0, SurfaceChoice.Pick(new List<SurfaceHit> { tableEdge, floor }, rules));
        }

        [Test]
        public void ObjectOnATableTop_MayBeSetBack()
        {
            Assert.IsTrue(SurfaceChoice.AllowsSetBack(tableTop));
        }

        [Test]
        public void ObjectOnAWall_IsNotSetBack()
        {
            Assert.IsFalse(SurfaceChoice.AllowsSetBack(backWall));
        }

        [Test]
        public void RayThatEndsOnASmallUprightSurface_IsNotSetBackEither()
        {
            Assert.IsFalse(SurfaceChoice.AllowsSetBack(tableEdge));
        }

        [Test]
        public void SecondSmallUprightSurfaceInBetween_DoesNotStopTheSearch()
        {
            var secondEdge = new SurfaceHit(new Vector3(0f, 0.77f, 3.33f), 2.53f, false, true, 0.3f);

            Assert.AreEqual(2, SurfaceChoice.Pick(new List<SurfaceHit> { tableEdge, secondEdge, tableTop }, rules));
        }
    }
}
