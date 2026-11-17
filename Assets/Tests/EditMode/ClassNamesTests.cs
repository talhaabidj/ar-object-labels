using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArObjectLabels.Core;
using NUnit.Framework;
using UnityEngine;

namespace ArObjectLabels.Tests
{
    public class ClassNamesTests
    {
        const string ClassListFile = "Models/yolox_classes.txt";
        const string DisplayNamesFile = "Models/yolox_display_names.txt";
        const int ClassCount = 80;

        static List<string> Classes()
        {
            return ClassNames.ParseClassList(File.ReadAllText(Path.Combine(Application.dataPath, ClassListFile)));
        }

        static Dictionary<string, string> DisplayNames()
        {
            return ClassNames.ParseDisplayNames(
                File.ReadAllText(Path.Combine(Application.dataPath, DisplayNamesFile)));
        }

        [Test]
        public void ClassList_HasEveryClassOnce()
        {
            List<string> classes = Classes();

            Assert.AreEqual(ClassCount, classes.Count);
            Assert.AreEqual(ClassCount, classes.Distinct().Count());
        }

        [Test]
        public void EveryClass_HasADisplayName_ThatIsNotEmpty()
        {
            Dictionary<string, string> displayNames = DisplayNames();
            string[] missing = Classes()
                .Where(c => !displayNames.TryGetValue(c, out string label) || label.Length == 0).ToArray();

            Assert.IsEmpty(missing);
        }

        [Test]
        public void NoDisplayName_IsLongerThanTheLimit()
        {
            string[] tooLong = DisplayNames().Values.Where(v => v.Length > ClassNames.MaxLabelLength).ToArray();

            Assert.IsEmpty(tooLong);
        }

        [Test]
        public void EveryDisplayName_StartsWithACapitalLetter()
        {
            string[] wrong = DisplayNames().Values.Where(v => v.Length > 0 && !char.IsUpper(v[0])).ToArray();

            Assert.IsEmpty(wrong);
        }

        [Test]
        public void NoDisplayName_IsForAClassTheModelDoesNotHave()
        {
            List<string> classes = Classes();
            string[] unknown = DisplayNames().Keys.Where(k => !classes.Contains(k)).ToArray();

            Assert.IsEmpty(unknown);
        }

        [TestCase("cup", "Cup")]
        [TestCase("potted plant", "Potted plant")]
        [TestCase("tv", "TV")]
        [TestCase("cell phone", "Phone")]
        [TestCase("dining table", "Table")]
        [TestCase("scissors", "Scissors")]
        public void Label_IsTheNameAlone(string rawName, string expected)
        {
            List<string> classes = Classes();

            Assert.AreEqual(expected, ClassNames.LabelFor(classes.IndexOf(rawName), classes, DisplayNames()));
        }

        [Test]
        public void ClassWithNoDisplayName_ShowsItsRawNameWithACapital()
        {
            var classes = new List<string> { "teddy bear" };

            Assert.AreEqual("Teddy bear", ClassNames.LabelFor(0, classes, new Dictionary<string, string>()));
        }

        [TestCase(-1, "Object -1")]
        [TestCase(17, "Object 17")]
        public void NumberOutsideTheList_ShowsAsObjectN(int classNumber, string expected)
        {
            var classes = new List<string> { "cup" };

            Assert.AreEqual(expected, ClassNames.LabelFor(classNumber, classes, new Dictionary<string, string>()));
        }

        [Test]
        public void DisplayNames_SkipCommentsAndEmptyLines_AndTrimSpaces()
        {
            Dictionary<string, string> names =
                ClassNames.ParseDisplayNames("# a comment\n\n  tv  =  TV \r\ncup = Cup\n");

            Assert.AreEqual(2, names.Count);
            Assert.AreEqual("TV", names["tv"]);
            Assert.AreEqual("Cup", names["cup"]);
        }
    }
}
