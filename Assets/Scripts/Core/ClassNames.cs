using System.Collections.Generic;

namespace ArObjectLabels.Core
{
    /// <summary>Turns a model's class number into the short text a label shows.</summary>
    public static class ClassNames
    {
        /// <summary>One raw class name per line. The first name is class 0. Empty lines are skipped.</summary>
        public static List<string> ParseClassList(string text)
        {
            var classes = new List<string>();
            foreach (string line in text.Split('\n'))
            {
                string name = line.Trim();
                if (name.Length > 0)
                {
                    classes.Add(name);
                }
            }

            return classes;
        }

        /// <summary>Lines of "raw class name = label text". Empty lines and # lines are skipped.</summary>
        public static Dictionary<string, string> ParseDisplayNames(string text)
        {
            var displayNames = new Dictionary<string, string>();
            foreach (string line in text.Split('\n'))
            {
                string trimmed = line.Trim();
                int equals = trimmed.IndexOf('=');
                if (trimmed.Length == 0 || trimmed[0] == '#' || equals < 0)
                {
                    continue;
                }

                displayNames[trimmed.Substring(0, equals).Trim()] = trimmed.Substring(equals + 1).Trim();
            }

            return displayNames;
        }

        /// <summary>The label text for a class name. A class with no display name shows its raw name with a capital.</summary>
        public static string LabelFor(string raw, IReadOnlyDictionary<string, string> displayNames)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return "Object";
            }
            if (displayNames.TryGetValue(raw, out string label) && label.Length > 0)
            {
                return label;
            }

            return char.ToUpperInvariant(raw[0]) + raw.Substring(1);
        }
    }
}
