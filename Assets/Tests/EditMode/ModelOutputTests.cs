using System.Collections.Generic;
using ArObjectLabels.Core;
using NUnit.Framework;

namespace ArObjectLabels.Tests
{
    [Timeout(5000)]
    public class ModelOutputTests
    {
        static readonly string[] classNames = { "laptop", "vase" };
        const int Columns = 3;

        // A table of three columns and two classes, row after row: left, top, right, bottom, then one row of scores a class.
        static float[] Table(float[] laptopScores, float[] vaseScores)
        {
            var table = new List<float>();
            table.AddRange(new[] { 10f, 20f, 30f });
            table.AddRange(new[] { 11f, 21f, 31f });
            table.AddRange(new[] { 110f, 120f, 130f });
            table.AddRange(new[] { 111f, 121f, 131f });
            table.AddRange(laptopScores);
            table.AddRange(vaseScores);
            return table.ToArray();
        }

        [Test]
        public void Table_HasFourBoxRowsAndOneRowAClass()
        {
            Assert.AreEqual(84, ModelOutput.Rows(80));
        }

        [Test]
        public void ColumnWithAHighScore_GivesItsBoxAndClass()
        {
            var found = new List<Detection>();

            ModelOutput.AddBoxes(Table(new[] { 0.1f, 0.9f, 0.2f }, new[] { 0.0f, 0.1f, 0.3f }), Columns, classNames, 0.5f, found);

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("laptop", found[0].ClassName);
            Assert.AreEqual(0.9f, found[0].Score);
            Assert.AreEqual(20f, found[0].Left);
            Assert.AreEqual(21f, found[0].Top);
            Assert.AreEqual(120f, found[0].Right);
            Assert.AreEqual(121f, found[0].Bottom);
        }

        [Test]
        public void ScoreExactlyAtTheLimit_Counts()
        {
            var found = new List<Detection>();

            ModelOutput.AddBoxes(Table(new[] { 0.5f, 0.49f, 0f }, new[] { 0f, 0f, 0f }), Columns, classNames, 0.5f, found);

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(10f, found[0].Left);
        }

        [Test]
        public void BoxAtExactlyTheScoreGiven_IsKept_AndOneJustBelowItIsNot()
        {
            var found = new List<Detection>();

            ModelOutput.AddBoxes(Table(new[] { 0.40f, 0.39f, 0f }, new[] { 0f, 0f, 0f }), Columns, classNames,
                new ModelSettings().minScore, found);

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(0.40f, found[0].Score);
        }

        [Test]
        public void ColumnWithTwoHighScores_GivesABoxForEachClass()
        {
            var found = new List<Detection>();

            ModelOutput.AddBoxes(Table(new[] { 0f, 0f, 0.7f }, new[] { 0f, 0f, 0.6f }), Columns, classNames, 0.5f, found);

            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("laptop", found[0].ClassName);
            Assert.AreEqual("vase", found[1].ClassName);
            Assert.AreEqual(found[0].Left, found[1].Left);
        }

        [Test]
        public void NoHighScore_GivesNothing_AndKeepsWhatWasThere()
        {
            var found = new List<Detection> { new Detection("chair", 0.8f, 0f, 0f, 1f, 1f) };

            ModelOutput.AddBoxes(Table(new[] { 0.1f, 0.2f, 0.3f }, new[] { 0.4f, 0.1f, 0f }), Columns, classNames, 0.5f, found);

            Assert.AreEqual(1, found.Count);
        }

        [Test]
        public void TableOfTheRightSize_IsUsedAgain()
        {
            var table = new float[84 * 8400];
            Assert.AreSame(table, ModelOutput.TableFor(table, 84 * 8400));
        }

        [Test]
        public void TableOfAnotherSize_IsReplacedByOneOfTheSizeAsked()
        {
            var table = new float[10];
            float[] made = ModelOutput.TableFor(table, 12);
            Assert.AreNotSame(table, made);
            Assert.AreEqual(12, made.Length);
        }

        [Test]
        public void NoTableYet_OneIsMade()
        {
            Assert.AreEqual(5, ModelOutput.TableFor(null, 5).Length);
        }
    }
}
