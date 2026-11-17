using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    public class DragRuleTests
    {
        static readonly Vector3 lastGood = new Vector3(1f, 0f, 2f);
        static readonly Vector3 hitPoint = new Vector3(3f, 0.5f, 4f);

        [Test]
        public void PointerOverUpwardSurface_MovesCubeToHitPoint()
        {
            Vector3 next = DragRule.NextPosition(lastGood, hitSurface: true, surfaceFacesUp: true, hitPoint);

            Assert.AreEqual(hitPoint, next);
        }

        [Test]
        public void PointerOverWall_KeepsLastGoodPosition()
        {
            Vector3 next = DragRule.NextPosition(lastGood, hitSurface: true, surfaceFacesUp: false, hitPoint);

            Assert.AreEqual(lastGood, next);
        }

        [Test]
        public void PointerOverNothing_KeepsLastGoodPosition()
        {
            Vector3 next = DragRule.NextPosition(lastGood, hitSurface: false, surfaceFacesUp: false, hitPoint);

            Assert.AreEqual(lastGood, next);
        }
    }
}
