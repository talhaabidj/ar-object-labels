using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class PanelEdgesTests
    {
        // A picture 2 wide and 1 high, y up.
        static readonly Rect picture = Rect.MinMaxRect(-1f, -0.5f, 1f, 0.5f);
        static readonly Rect topLeftPanel = Rect.MinMaxRect(-1f, 0.4f, -0.3f, 0.5f);
        static readonly Rect bottomBar = Rect.MinMaxRect(-0.6f, -0.5f, 0.6f, -0.4f);
        const float Margin = 0.01f;

        [Test]
        public void ScreenRectangle_BecomesAPictureRectangle()
        {
            // The left half of a 1000 x 500 screen, top fifth.
            Rect onScreen = Rect.MinMaxRect(0f, 400f, 500f, 500f);

            Rect inPicture = PanelEdges.ToPicture(onScreen, new Vector2(1000f, 500f), new Vector2(1f, 0.5f));

            Assert.AreEqual(-1f, inPicture.xMin, 0.0001f);
            Assert.AreEqual(0f, inPicture.xMax, 0.0001f);
            Assert.AreEqual(0.3f, inPicture.yMin, 0.0001f);
            Assert.AreEqual(0.5f, inPicture.yMax, 0.0001f);
        }

        [Test]
        public void LabelUnderAPanelAtTheTop_GoesDownBelowIt()
        {
            var label = new Rect(-0.8f, 0.42f, 0.2f, 0.06f);

            Rect kept = PanelEdges.KeepOut(label, new List<Rect> { topLeftPanel }, Margin, picture);

            Assert.AreEqual(label.x, kept.x, 0.0001f);
            Assert.AreEqual(0.4f - Margin, kept.yMax, 0.0001f);
        }

        [Test]
        public void LabelOnTheHintBar_GoesUpAboveIt()
        {
            var label = new Rect(0f, -0.48f, 0.2f, 0.06f);

            Rect kept = PanelEdges.KeepOut(label, new List<Rect> { bottomBar }, Margin, picture);

            Assert.AreEqual(-0.4f + Margin, kept.yMin, 0.0001f);
        }

        [Test]
        public void LabelClearOfEveryPanel_StaysWhereItIs()
        {
            var label = new Rect(0f, 0f, 0.2f, 0.06f);

            Assert.AreEqual(label, PanelEdges.KeepOut(label, new List<Rect> { topLeftPanel, bottomBar }, Margin, picture));
            Assert.IsTrue(PanelEdges.IsClear(label, new List<Rect> { topLeftPanel, bottomBar }));
        }

        [Test]
        public void LabelForAnObjectUnderAPanel_IsPlacedClearOfIt()
        {
            // The object sits just under the top-left panel, so its own place is in the panel.
            Rect ownObject = Rect.MinMaxRect(-0.75f, 0.2f, -0.55f, 0.38f);
            var label = new Rect(-0.75f, 0.39f, 0.2f, 0.06f);

            Vector2 offset = LabelArrange.Offset(label, ownObject, new List<Rect>(), new List<Rect>(), Margin, picture,
                new List<Rect> { topLeftPanel });
            var placed = new Rect(label.x + offset.x, label.y + offset.y, label.width, label.height);

            Assert.IsTrue(PanelEdges.IsClear(placed, new List<Rect> { topLeftPanel }), "placed at " + placed);
        }

        [Test]
        public void PlaceUnderAPanel_CannotBeKept()
        {
            var label = new Rect(-0.8f, 0.42f, 0.2f, 0.06f);

            Assert.IsFalse(LabelArrange.CanStay(label, new List<Rect>(), new List<Rect>(), Margin, picture, new List<Rect> { topLeftPanel }));
        }
    }
}
