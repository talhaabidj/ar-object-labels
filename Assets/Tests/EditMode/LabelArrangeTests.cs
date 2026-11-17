using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelArrangeTests
    {
        const float Margin = 0.01f;
        static readonly List<Rect> none = new List<Rect>();

        // An object 0.10 wide and high with its foot at the origin, and its label 0.12 x 0.05 a gap of 0.02 above it.
        static readonly Rect cube = new Rect(-0.05f, 0f, 0.10f, 0.10f);
        static readonly Rect label = new Rect(-0.06f, 0.12f, 0.12f, 0.05f);

        // Run under a time limit: the rule uses the overlap rule, which has a loop.
        static Vector2 Offset(List<Rect> otherObjects, List<Rect> placedLabels)
        {
            return TimeLimit.Run(() => LabelArrange.Offset(label, cube, otherObjects, placedLabels, Margin));
        }

        static Rect Moved(Vector2 offset)
        {
            return new Rect(label.x + offset.x, label.y + offset.y, label.width, label.height);
        }

        [Test]
        public void NothingAround_StaysAboveItsObject()
        {
            Assert.AreEqual(Vector2.zero, Offset(none, none));
        }

        [Test]
        public void ObjectsSideBySide_SecondLabelClearsTheFirst()
        {
            var neighbour = new Rect(0.07f, 0f, 0.10f, 0.10f);
            var neighbourLabel = new Rect(0.06f, 0.12f, 0.12f, 0.05f);
            Vector2 offset = Offset(new List<Rect> { neighbour }, new List<Rect> { neighbourLabel });

            Assert.AreNotEqual(Vector2.zero, offset);
            Assert.IsFalse(LabelArrange.Touches(Moved(offset), neighbourLabel, Margin - 0.0001f));
            Assert.IsFalse(LabelArrange.Touches(Moved(offset), neighbour, Margin - 0.0001f));
        }

        [Test]
        public void ObjectStackedOnTop_LabelGoesToTheSide()
        {
            var upper = new Rect(-0.05f, 0.10f, 0.10f, 0.10f);
            Vector2 offset = Offset(new List<Rect> { upper }, none);
            Rect moved = Moved(offset);

            Assert.IsFalse(LabelArrange.Touches(moved, upper, Margin - 0.0001f));
            Assert.Greater(moved.xMin, cube.xMax, "to the right of its own object");
            Assert.AreEqual(cube.center.y, moved.center.y, 0.0001f, "level with its own object");
        }

        [Test]
        public void StackedWithSomethingOnTheRight_LabelGoesToTheLeft()
        {
            var upper = new Rect(-0.05f, 0.10f, 0.10f, 0.10f);
            var onTheRight = new Rect(0.07f, 0f, 0.10f, 0.10f);
            Rect moved = Moved(Offset(new List<Rect> { upper, onTheRight }, none));

            Assert.Less(moved.xMax, cube.xMin);
            Assert.IsFalse(LabelArrange.Touches(moved, upper, Margin - 0.0001f));
            Assert.IsFalse(LabelArrange.Touches(moved, onTheRight, Margin - 0.0001f));
        }

        [Test]
        public void StackedWithSomethingOnBothSides_FindsAPlaceClearOfEveryObject()
        {
            var upper = new Rect(-0.05f, 0.10f, 0.10f, 0.10f);
            var onTheRight = new Rect(0.07f, 0f, 0.10f, 0.10f);
            var onTheLeft = new Rect(-0.17f, 0f, 0.10f, 0.10f);
            var objects = new List<Rect> { upper, onTheRight, onTheLeft };
            Rect moved = Moved(Offset(objects, none));

            foreach (Rect other in objects)
            {
                Assert.IsFalse(LabelArrange.Touches(moved, other, Margin - 0.0001f));
            }
        }

        [Test]
        public void EveryNearbyPlaceTaken_MovesStraightUpPastEverything()
        {
            // One wide, tall object covers every nearby place.
            var wall = new Rect(-0.50f, 0.105f, 1.00f, 0.40f);
            var beside = new Rect(-0.50f, -0.20f, 0.44f, 0.30f);
            var besideToo = new Rect(0.06f, -0.20f, 0.44f, 0.30f);
            var objects = new List<Rect> { wall, beside, besideToo };
            Rect moved = Moved(Offset(objects, none));

            foreach (Rect other in objects)
            {
                Assert.IsFalse(LabelArrange.Touches(moved, other, Margin - 0.0001f));
            }
            Assert.AreEqual(label.x, moved.x, 0.0001f, "straight up, not sideways");
        }

        [Test]
        public void LiftedPastALabel_DoesNotLandInAThirdObject()
        {
            // The neighbour's label sits where ours wants to be, and a tall object stands just above that.
            var neighbourLabel = new Rect(-0.02f, 0.12f, 0.12f, 0.05f);
            var tallObject = new Rect(-0.10f, 0.19f, 0.30f, 0.20f);
            Rect moved = Moved(Offset(new List<Rect> { tallObject }, new List<Rect> { neighbourLabel }));

            Assert.IsFalse(LabelArrange.Touches(moved, neighbourLabel, Margin - 0.0001f));
            Assert.IsFalse(LabelArrange.Touches(moved, tallObject, Margin - 0.0001f));
        }

        [Test]
        public void ObjectBehindOrUnderOurs_IsNotInTheWay()
        {
            var tableUnderTheCube = new Rect(-0.60f, -0.05f, 1.20f, 0.40f);
            var cubeNextToOurs = new Rect(0.07f, 0f, 0.10f, 0.10f);

            Assert.IsTrue(LabelArrange.HoldsTheObject(tableUnderTheCube, cube));
            Assert.IsFalse(LabelArrange.HoldsTheObject(cubeNextToOurs, cube));
        }

        [Test]
        public void StackedAndTheUpperLabelIsBeside_StillClearsThatLabel()
        {
            var upper = new Rect(-0.05f, 0.10f, 0.10f, 0.10f);
            var labelOnTheRight = new Rect(0.06f, 0.02f, 0.12f, 0.05f);
            Rect moved = Moved(Offset(new List<Rect> { upper }, new List<Rect> { labelOnTheRight }));

            Assert.IsFalse(LabelArrange.Touches(moved, labelOnTheRight, Margin - 0.0001f));
            Assert.IsFalse(LabelArrange.Touches(moved, upper, Margin - 0.0001f));
        }

        [Test]
        public void ManyLabelsInTheWay_ClearsEveryOne()
        {
            var placed = new List<Rect>();
            for (int row = 0; row < 12; row++)
            {
                placed.Add(new Rect(-0.06f, 0.12f + row * 0.06f, 0.12f, 0.05f));
            }
            Rect moved = Moved(Offset(none, placed));

            foreach (Rect other in placed)
            {
                Assert.IsFalse(LabelArrange.Touches(moved, other, Margin - 0.0001f));
            }
        }
    }
}
