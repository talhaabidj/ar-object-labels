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

        // A picture so large that its edge plays no part.
        static readonly Rect everywhere = new Rect(-100f, -100f, 200f, 200f);
        // A picture of a camera that sees 60 degrees up and down: 1.06 wide and 0.58 high, one metre away.
        static readonly Rect picture = new Rect(-0.53f, -0.29f, 1.06f, 0.58f);

        // An object 0.10 wide and high with its foot at the origin, and its label 0.12 x 0.05 a gap of 0.02 above it.
        static readonly Rect cube = new Rect(-0.05f, 0f, 0.10f, 0.10f);
        static readonly Rect label = new Rect(-0.06f, 0.12f, 0.12f, 0.05f);

        // Run under a time limit: the rule uses the overlap rule, which has a loop.
        static Vector2 Offset(List<Rect> otherObjects, List<Rect> placedLabels)
        {
            return TimeLimit.Run(() => LabelArrange.Offset(label, cube, otherObjects, placedLabels, Margin, everywhere));
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

        static Rect At(Rect rect, float x, float y)
        {
            return new Rect(x, y, rect.width, rect.height);
        }

        static Rect MovedFrom(Rect from, Vector2 offset)
        {
            return new Rect(from.x + offset.x, from.y + offset.y, from.width, from.height);
        }

        [Test]
        public void LabelInsideThePicture_DoesNotMove()
        {
            Assert.AreEqual(Vector2.zero, LabelArrange.Offset(label, cube, none, none, Margin, picture));
        }

        [Test]
        public void LabelCutByTheRightEdge_MovesJustInside()
        {
            // The object stands at the right edge; its label would reach 0.04 past it.
            Rect objectAtTheEdge = At(cube, 0.46f, 0f);
            Rect ownLabel = At(label, 0.45f, 0.12f);

            Vector2 offset = LabelArrange.Offset(ownLabel, objectAtTheEdge, none, none, Margin, picture);

            Assert.AreEqual(picture.xMax, MovedFrom(ownLabel, offset).xMax, 0.0001f);
            Assert.AreEqual(0f, offset.y, 0.0001f, "only as far as needed");
        }

        [Test]
        public void LabelAboveTheTopEdge_MovesDownIntoThePicture()
        {
            // A clock near the ceiling: its label's own place is above the picture.
            Rect clock = At(cube, -0.05f, 0.17f);
            Rect ownLabel = At(label, -0.06f, 0.30f);

            Vector2 offset = LabelArrange.Offset(ownLabel, clock, none, none, Margin, picture);

            Rect moved = MovedFrom(ownLabel, offset);
            Assert.LessOrEqual(moved.yMax, picture.yMax + 0.0001f);
            Assert.GreaterOrEqual(moved.xMin, picture.xMin);
            Assert.LessOrEqual(moved.xMax, picture.xMax);
        }

        [Test]
        public void PulledInsideOntoAnotherLabel_TakesAnotherPlaceInside()
        {
            Rect objectAtTheEdge = At(cube, 0.46f, 0f);
            Rect ownLabel = At(label, 0.45f, 0.12f);
            // Another label already sits where ours would be pulled to.
            var taken = new List<Rect> { new Rect(0.41f, 0.12f, 0.12f, 0.05f) };

            Vector2 offset = TimeLimit.Run(() => LabelArrange.Offset(ownLabel, objectAtTheEdge, none, taken, Margin, picture));

            Rect moved = MovedFrom(ownLabel, offset);
            Assert.IsFalse(LabelArrange.Touches(moved, taken[0], Margin), "clear of the other label");
            Assert.LessOrEqual(moved.xMax, picture.xMax + 0.0001f);
            Assert.LessOrEqual(moved.yMax, picture.yMax + 0.0001f);
        }

        [Test]
        public void ObjectWithItsMiddleOutsideThePicture_ItsLabelIsNotPulledIn()
        {
            // The object reaches 0.03 into the picture, but its middle is outside it.
            Rect mostlyOut = At(cube, 0.50f, 0f);
            Rect ownLabel = At(label, 0.49f, 0.12f);

            Assert.AreEqual(Vector2.zero, LabelArrange.Offset(ownLabel, mostlyOut, none, none, Margin, picture));
        }

        [Test]
        public void ObjectFoundNow_WithItsMiddleOutsideThePicture_ItsLabelIsPulledIn()
        {
            Rect mostlyOut = At(cube, 0.50f, 0f);
            Rect ownLabel = At(label, 0.49f, 0.12f);

            Vector2 offset = LabelArrange.Offset(ownLabel, mostlyOut, none, none, Margin, picture, null, objectFound: true);

            Assert.LessOrEqual(MovedFrom(ownLabel, offset).xMax, picture.xMax + 0.0001f);
        }

        [Test]
        public void LabelThatWouldOnlySitOffThePicture_TakesNoPlace()
        {
            // The object just reaches into the picture, its middle is outside, it is not found now, and its label is off the picture.
            Rect mostlyOut = At(cube, 0.50f, 0f);
            Rect offPicture = At(label, 0.60f, 0.12f);

            Assert.IsFalse(LabelArrange.TakesAPlace(mostlyOut, offPicture, false, picture));
            Assert.IsTrue(LabelArrange.TakesAPlace(mostlyOut, offPicture, true, picture), "found now");
            Assert.IsTrue(LabelArrange.TakesAPlace(mostlyOut, At(label, 0.49f, 0.12f), false, picture), "label partly in the picture");
            Assert.IsTrue(LabelArrange.TakesAPlace(cube, offPicture, false, picture), "object's middle in the picture");
            Assert.IsFalse(LabelArrange.TakesAPlace(At(cube, 0.80f, 0f), label, true, picture), "object wholly outside");
            Assert.IsFalse(LabelArrange.TakesAPlace(cube, label, true, picture, labelInFront: false), "label's own place behind the camera");
        }

        [Test]
        public void ObjectFoundButMostlyOutOfThePicture_TakesNoPlace()
        {
            // Seen in true-step 22-29: the school chair cut by the left edge got a new place in every frame.
            Rect thirtyPercentIn = At(cube, 0.50f, 0f);
            Rect halfIn = At(cube, 0.48f, 0f);

            Assert.IsFalse(LabelArrange.TakesAPlace(thirtyPercentIn, label, true, picture, true, 0.4f));
            Assert.IsTrue(LabelArrange.TakesAPlace(halfIn, label, true, picture, true, 0.4f));
        }

        [Test]
        public void ShareInPicture_CountsHowMuchOfTheObjectIsIn()
        {
            Assert.AreEqual(1f, LabelArrange.ShareInPicture(cube, picture), 0.0001f, "wholly in");
            Assert.AreEqual(0.3f, LabelArrange.ShareInPicture(At(cube, 0.50f, 0f), picture), 0.0001f, "cut on the right");
            Assert.AreEqual(0.3f * 0.5f, LabelArrange.ShareInPicture(At(cube, 0.50f, 0.24f), picture), 0.0001f, "cut in a corner");
            Assert.AreEqual(0f, LabelArrange.ShareInPicture(At(cube, 0.80f, 0f), picture), "wholly out");
        }

        [Test]
        public void ObjectBiggerThanThePicture_ThatFillsIt_CountsAsWhollyIn()
        {
            // A table close to the camera reaches past both side edges of the picture.
            var table = new Rect(-2f, -0.25f, 4f, 0.3f);

            Assert.AreEqual(1f, LabelArrange.ShareInPicture(table, picture), 0.0001f);
            Assert.IsTrue(LabelArrange.TakesAPlace(table, label, true, picture, true, 0.4f));
        }

        [Test]
        public void LabelNotPulledIn_IsNotMovedAside_EvenWhenAnotherLabelIsInItsPlace()
        {
            // Seen in S11, S18 and S20: the object is mostly out of the picture and not found, its own label place
            // touches the picture, and another label is there. Moving it aside put it wholly off the picture.
            Rect mostlyOut = At(cube, 0.50f, 0f);
            Rect ownLabel = At(label, 0.49f, 0.12f);
            var taken = new List<Rect> { ownLabel };

            Vector2 offset = TimeLimit.Run(() => LabelArrange.Offset(ownLabel, mostlyOut, none, taken, Margin, picture));

            Assert.AreEqual(Vector2.zero, offset);
        }

        [Test]
        public void ObjectOutsideThePicture_ItsLabelIsNotPulledIn()
        {
            Rect farToTheRight = At(cube, 0.80f, 0f);
            Rect ownLabel = At(label, 0.79f, 0.12f);

            Assert.AreEqual(Vector2.zero, LabelArrange.Offset(ownLabel, farToTheRight, none, none, Margin, picture));
        }

        [Test]
        public void RectangleWiderThanThePicture_IsPutInItsMiddle()
        {
            Rect kept = LabelArrange.KeepInside(new Rect(0.3f, 0f, 2f, 0.05f), picture);

            Assert.AreEqual(picture.center.x, kept.center.x, 0.0001f);
        }

        [Test]
        public void HeldPlace_CanBeKept_WhileItIsFreeAndInsideThePicture()
        {
            Rect held = At(label, 0.10f, 0.12f);

            Assert.IsTrue(LabelArrange.CanStay(held, none, none, Margin, picture));
            Assert.IsFalse(LabelArrange.CanStay(held, none, new List<Rect> { At(label, 0.12f, 0.13f) }, Margin, picture), "another label is there now");
            Assert.IsFalse(LabelArrange.CanStay(At(label, 0.50f, 0.12f), none, none, Margin, picture), "it would be cut by the edge");
        }

        [Test]
        public void PlaceOnThePicturesEdge_CountsAsInside()
        {
            Rect atTheEdge = At(label, picture.xMax - label.width, 0.12f);

            Assert.IsTrue(LabelArrange.CanStay(atTheEdge, none, none, Margin, picture));
        }
    }
}
