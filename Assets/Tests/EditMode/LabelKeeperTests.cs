using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class LabelKeeperTests
    {
        const float Same = 0.5f;
        const float Wait = 1f;
        const float FadeTime = 0.3f;
        const float Frame = 0.1f;
        static readonly Bounds laptopBox = new Bounds(new Vector3(0f, 0.8f, 2f), new Vector3(0.3f, 0.2f, 0.24f));

        static void Find(LabelKeeper keeper, params (string name, Bounds box)[] found)
        {
            var names = new List<string>();
            var boxes = new List<Bounds>();
            var turns = new List<Quaternion>();
            var scores = new List<float>();
            foreach ((string name, Bounds box) in found)
            {
                names.Add(name);
                boxes.Add(box);
                turns.Add(Quaternion.identity);
                scores.Add(0.9f);
            }
            keeper.Take(names, boxes, turns, scores, Same);
        }

        static readonly System.Func<Vector3, bool> everythingInView = place => true;
        static readonly System.Func<Vector3, bool> nothingInView = place => false;

        [Test]
        public void FoundObject_GetsAKeptLabel_AtItsPlace()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));

            Assert.AreEqual(1, keeper.Kept.Count);
            Assert.AreEqual(laptopBox.center, keeper.Kept[0].RoomBox.center);
            Assert.AreEqual(1f, keeper.Kept[0].Fade);
        }

        [Test]
        public void ObjectOutOfView_KeepsItsLabel_However_LongItIsAway()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));

            for (int frame = 0; frame < 100; frame++)
            {
                Find(keeper);
                keeper.Age(nothingInView, Frame, Wait, FadeTime);
            }

            Assert.AreEqual(1, keeper.Kept.Count);
            Assert.AreEqual(1f, keeper.Kept[0].Fade);
        }

        [Test]
        public void ObjectInViewButNotFound_StaysOneSecond_ThenFadesOut_ThenIsDropped()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));
            KeptObject laptop = keeper.Kept[0];

            for (int frame = 0; frame < 10; frame++)
            {
                Find(keeper);
                keeper.Age(everythingInView, Frame, Wait, FadeTime);
            }
            Assert.AreEqual(1f, laptop.Fade, 0.0001f, "still fully drawn after one second");

            Find(keeper);
            keeper.Age(everythingInView, Frame, Wait, FadeTime);
            Assert.AreEqual(2f / 3f, laptop.Fade, 0.001f, "a third of the way out");

            Find(keeper);
            keeper.Age(everythingInView, Frame, Wait, FadeTime);
            Find(keeper);
            keeper.Age(everythingInView, Frame, Wait, FadeTime);
            CollectionAssert.IsEmpty(keeper.Kept);
        }

        [Test]
        public void ObjectFoundAgainWhileFading_IsDrawnFullyAgain_AndKeepsItsMemory()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));
            KeptObject laptop = keeper.Kept[0];
            laptop.Memory.WasShown = true;
            for (int frame = 0; frame < 11; frame++)
            {
                Find(keeper);
                keeper.Age(everythingInView, Frame, Wait, FadeTime);
            }

            Find(keeper, ("laptop", laptopBox));
            keeper.Age(everythingInView, Frame, Wait, FadeTime);

            Assert.AreSame(laptop, keeper.Kept[0]);
            Assert.AreEqual(1f, laptop.Fade);
            Assert.AreEqual(0f, laptop.UnseenInView);
            Assert.IsTrue(laptop.Memory.WasShown);
        }

        [Test]
        public void ObjectFoundAFewCentimetresAway_IsTheSameObject_AndMovesThere()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));
            var nudged = new Bounds(laptopBox.center + new Vector3(0.05f, 0f, 0f), laptopBox.size);

            Find(keeper, ("laptop", nudged));

            Assert.AreEqual(1, keeper.Kept.Count);
            Assert.AreEqual(nudged.center, keeper.Kept[0].RoomBox.center);
        }

        [Test]
        public void OtherObjectWithTheSameName_FarAway_GetsItsOwnLabel()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("chair", laptopBox));
            var farChair = new Bounds(laptopBox.center + new Vector3(2f, 0f, 0f), laptopBox.size);

            Find(keeper, ("chair", farChair));

            Assert.AreEqual(2, keeper.Kept.Count);
        }

        [Test]
        public void FoundNow_IsTrueOnlyInAFrameThatFindsTheObject()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));
            Assert.IsTrue(keeper.Kept[0].FoundNow);

            Find(keeper);
            Assert.IsFalse(keeper.Kept[0].FoundNow);
        }

        // The keyboard as the sweep saw it: from far away its room point was put on the chair in front of the
        // desk, 74 cm from where a nearer view puts it. The two boxes do not overlap.
        static readonly Bounds keyboardFromAfar = new Bounds(new Vector3(0.9f, 0.55f, 1.26f), new Vector3(0.45f, 0.05f, 0.14f));
        static readonly Bounds keyboardFromNear = new Bounds(new Vector3(0.9f, 0.76f, 2.0f), new Vector3(0.45f, 0.05f, 0.14f));

        static void FindWithScreenCheck(LabelKeeper keeper, string name, Bounds box, bool keptPointSeenInsideBox)
        {
            keeper.Take(new List<string> { name }, new List<Bounds> { box }, new List<Quaternion> { Quaternion.identity },
                new List<float> { 0.9f }, Same, (found, point) => keptPointSeenInsideBox);
        }

        [Test]
        public void KeyboardWhosePlaceMoved74Centimetres_KeepsOneLabel_WhenItsOldPointIsInsideTheNewBox()
        {
            var keeper = new LabelKeeper();
            FindWithScreenCheck(keeper, "keyboard", keyboardFromAfar, false);
            KeptObject keyboard = keeper.Kept[0];

            FindWithScreenCheck(keeper, "keyboard", keyboardFromNear, true);

            Assert.AreEqual(1, keeper.Kept.Count, "no second keyboard label");
            Assert.AreSame(keyboard, keeper.Kept[0]);
            Assert.AreEqual(keyboardFromNear.center, keyboard.RoomBox.center, "moved to where it is found now");
        }

        [Test]
        public void KeyboardWhosePlaceMoved74Centimetres_GetsASecondLabel_WhenNeitherRuleHolds()
        {
            var keeper = new LabelKeeper();
            FindWithScreenCheck(keeper, "keyboard", keyboardFromAfar, false);

            FindWithScreenCheck(keeper, "keyboard", keyboardFromNear, false);

            Assert.AreEqual(2, keeper.Kept.Count);
        }

        [Test]
        public void ObjectWhoseRoomBoxOverlapsTheKeptOne_IsTheSameObject()
        {
            var keeper = new LabelKeeper();
            var bigPlant = new Bounds(new Vector3(3.5f, 0.6f, 3.5f), new Vector3(1.0f, 1.2f, 0.3f));
            var plantNearby = new Bounds(new Vector3(3.5f, 0.6f, 2.95f), new Vector3(1.0f, 1.2f, 1.0f));
            FindWithScreenCheck(keeper, "potted plant", bigPlant, false);

            FindWithScreenCheck(keeper, "potted plant", plantNearby, false);

            Assert.AreEqual(1, keeper.Kept.Count);
        }

        [Test]
        public void TwoObjectsOfOneKind_FoundTogether_KeepTwoLabels_EvenWhenOneIsInsideTheOthersBox()
        {
            var keeper = new LabelKeeper();
            var penA = new Bounds(new Vector3(2.0f, 0.75f, 3.5f), new Vector3(0.14f, 0.01f, 0.01f));
            var penB = new Bounds(new Vector3(2.08f, 0.75f, 3.52f), new Vector3(0.14f, 0.01f, 0.01f));
            var names = new List<string> { "pen", "pen" };
            var boxes = new List<Bounds> { penA, penB };
            var turns = new List<Quaternion> { Quaternion.identity, Quaternion.identity };
            var scores = new List<float> { 0.9f, 0.9f };
            keeper.Take(names, boxes, turns, scores, Same, (found, point) => true);

            keeper.Take(names, boxes, turns, scores, Same, (found, point) => true);

            Assert.AreEqual(2, keeper.Kept.Count);
        }

        [Test]
        public void ObjectOfAnotherName_InsideTheNewBox_IsNotTakenOver()
        {
            var keeper = new LabelKeeper();
            FindWithScreenCheck(keeper, "monitor", keyboardFromAfar, false);

            FindWithScreenCheck(keeper, "keyboard", keyboardFromNear, true);

            Assert.AreEqual(2, keeper.Kept.Count);
        }

        [Test]
        public void RoomPointSeenInsideABoxOnTheScreen()
        {
            // A camera at the origin looking along z, 60 degrees high, picture 1000 x 500.
            var frame = new CameraFrame(Vector3.zero, Quaternion.identity, 60f, 1000, 500, 0f);
            var middleBox = new Detection("keyboard", 0.9f, 400f, 200f, 600f, 300f);

            Assert.IsTrue(LabelKeeper.SeenInside(middleBox, frame, new Vector3(0f, 0f, 2f)), "straight ahead");
            Assert.IsFalse(LabelKeeper.SeenInside(middleBox, frame, new Vector3(1.5f, 0f, 2f)), "to the right");
            Assert.IsFalse(LabelKeeper.SeenInside(middleBox, frame, new Vector3(0f, 0f, -2f)), "behind the camera");
        }

        [Test]
        public void Forget_DropsEveryLabel()
        {
            var keeper = new LabelKeeper();
            Find(keeper, ("laptop", laptopBox));

            keeper.Forget();

            CollectionAssert.IsEmpty(keeper.Kept);
        }
    }
}
