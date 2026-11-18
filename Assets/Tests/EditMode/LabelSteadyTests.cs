using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelSteadyTests
    {
        const float Hold = 0.5f;
        // An eighth of a second: four of them are exactly the hold time.
        const float Frame = 0.125f;
        static readonly Vector2 up = new Vector2(0f, 0.06f);
        static readonly Vector2 right = new Vector2(0.11f, 0f);

        [Test]
        public void ShownLabel_CountsAsBiggerWhenLabelsAreChosen()
        {
            Assert.AreEqual(0.015f, LabelSteady.SizeForChoice(0.010f, true, 0.5f), 0.00001f);
            Assert.AreEqual(0.010f, LabelSteady.SizeForChoice(0.010f, false, 0.5f), 0.00001f);
        }

        [Test]
        public void LabelAtItsOwnPlace_MovesAsSoonAsThereIsAReason()
        {
            var memory = new LabelMemory();

            Assert.AreEqual(up, LabelSteady.Move(memory, true, up, Frame, Hold));
        }

        [Test]
        public void LabelAtItsOwnPlace_WithNoReason_Stays()
        {
            var memory = new LabelMemory();

            Assert.AreEqual(Vector2.zero, LabelSteady.Move(memory, true, Vector2.zero, Frame, Hold));
        }

        [Test]
        public void MovedLabel_WhoseReasonHasGone_WaitsBeforeGoingBack()
        {
            var memory = new LabelMemory { HeldMove = up };

            for (int frame = 0; frame < 3; frame++)
            {
                Assert.AreEqual(up, LabelSteady.Move(memory, true, Vector2.zero, Frame, Hold), "after frame " + frame);
            }
            Assert.AreEqual(Vector2.zero, LabelSteady.Move(memory, true, Vector2.zero, Frame, Hold), "after half a second");
        }

        [Test]
        public void ReasonThatComesBack_StartsTheWaitAgain()
        {
            var memory = new LabelMemory { HeldMove = up };
            LabelSteady.Move(memory, true, Vector2.zero, 0.4f, Hold);

            LabelSteady.Move(memory, true, up, Frame, Hold);

            Assert.AreEqual(0f, memory.CalmSeconds);
            Assert.AreEqual(up, LabelSteady.Move(memory, true, Vector2.zero, 0.4f, Hold));
        }

        [Test]
        public void MovedLabel_KeepsItsPlace_WhenAnotherPlaceWouldAlsoDo()
        {
            var memory = new LabelMemory { HeldMove = right };

            Assert.AreEqual(right, LabelSteady.Move(memory, true, up, Frame, Hold));
        }

        [Test]
        public void MovedLabel_WhosePlaceIsTaken_MovesAtOnce()
        {
            var memory = new LabelMemory { HeldMove = right };

            Assert.AreEqual(up, LabelSteady.Move(memory, false, up, Frame, Hold));
        }

        [Test]
        public void Glide_GoesPartOfTheWayInOneFrame_AndMostOfItInTheGlideTime()
        {
            Vector2 afterOneFrame = LabelSteady.Glide(Vector2.zero, up, 0.01f, 0.2f);
            Vector2 afterTheGlideTime = LabelSteady.Glide(Vector2.zero, up, 0.2f, 0.2f);

            Assert.Greater(afterOneFrame.y, 0f);
            Assert.Less(afterOneFrame.y, up.y * 0.2f);
            Assert.Greater(afterTheGlideTime.y, up.y * 0.9f);
        }

        [Test]
        public void Glide_ArrivesExactly()
        {
            Vector2 drawn = Vector2.zero;
            for (int frame = 0; frame < 200; frame++)
            {
                drawn = LabelSteady.Glide(drawn, up, 0.01f, 0.2f);
            }

            Assert.AreEqual(up, drawn);
        }

        [Test]
        public void NoGlideTime_IsThereAtOnce()
        {
            Assert.AreEqual(up, LabelSteady.Glide(Vector2.zero, up, 0.01f, 0f));
        }

        [Test]
        public void Forget_ClearsEverything()
        {
            var memory = new LabelMemory { WasShown = true, HeldMove = up, CalmSeconds = 0.3f, DrawnMove = up, HasBeenDrawn = true };

            memory.Forget();

            Assert.IsFalse(memory.WasShown);
            Assert.AreEqual(Vector2.zero, memory.HeldMove);
            Assert.IsFalse(memory.HasBeenDrawn);
        }
    }
}
