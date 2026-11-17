using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class PlaneMathsTests
    {
        static readonly Vector3 tableTop = new Vector3(0f, 0.74f, 0f);

        [Test]
        public void RayDownOntoATable_MeetsItAtTheRightDistance()
        {
            var ray = new Ray(new Vector3(0f, 1.74f, 0f), Vector3.down);

            Assert.IsTrue(PlaneMaths.RayMeetsPlane(ray, tableTop, Vector3.up, out float distance));
            Assert.AreEqual(1f, distance, 0.0001f);
        }

        [Test]
        public void SlantedRay_MeetsTheTableWhereExpected()
        {
            var ray = new Ray(new Vector3(0f, 1.74f, 0f), new Vector3(0f, -1f, 1f).normalized);

            Assert.IsTrue(PlaneMaths.RayMeetsPlane(ray, tableTop, Vector3.up, out float distance));
            Vector3 point = ray.GetPoint(distance);
            Assert.AreEqual(0.74f, point.y, 0.0001f);
            Assert.AreEqual(1f, point.z, 0.0001f);
        }

        [Test]
        public void RayAlongTheTable_NeverMeetsIt()
        {
            var ray = new Ray(new Vector3(0f, 1f, 0f), Vector3.forward);

            Assert.IsFalse(PlaneMaths.RayMeetsPlane(ray, tableTop, Vector3.up, out _));
        }

        [Test]
        public void RayPointingAway_DoesNotMeetIt()
        {
            var ray = new Ray(new Vector3(0f, 1.74f, 0f), Vector3.up);

            Assert.IsFalse(PlaneMaths.RayMeetsPlane(ray, tableTop, Vector3.up, out _));
        }

        [Test]
        public void SurfaceSeenFromBelow_IsStillMet()
        {
            var ray = new Ray(Vector3.zero, Vector3.up);

            Assert.IsTrue(PlaneMaths.RayMeetsPlane(ray, tableTop, Vector3.up, out float distance));
            Assert.AreEqual(0.74f, distance, 0.0001f);
        }

        static readonly List<Vector2> square = new List<Vector2>
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f)
        };

        // An L shape: the square with its top-right quarter cut out.
        static readonly List<Vector2> corner = new List<Vector2>
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, 1f)
        };

        [TestCase(0.5f, 0.5f, true)]
        [TestCase(0.01f, 0.99f, true)]
        [TestCase(1.2f, 0.5f, false)]
        [TestCase(-0.1f, 0.5f, false)]
        [TestCase(0.5f, 1.5f, false)]
        public void PointAgainstASquare(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, PlaneMaths.IsInside(new Vector2(x, y), square));
        }

        [TestCase(0.25f, 0.75f, true)]
        [TestCase(0.75f, 0.25f, true)]
        [TestCase(0.75f, 0.75f, false)]
        public void PointAgainstAnLShape(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, PlaneMaths.IsInside(new Vector2(x, y), corner));
        }

        [Test]
        public void OutlineWithNoCorners_HoldsNothing()
        {
            Assert.IsFalse(PlaneMaths.IsInside(Vector2.zero, new List<Vector2>()));
        }
    }
}
