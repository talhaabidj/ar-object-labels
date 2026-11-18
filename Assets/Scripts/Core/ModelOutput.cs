using System.Collections.Generic;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// Reads the output of YOLOv9-T. It is a table with one column for each place the model looks at and 84
    /// rows: the first four are a box's left, top, right and bottom in the model's own square picture, the
    /// other 80 are one score for each class.
    /// </summary>
    public static class ModelOutput
    {
        const int BoxRows = 4;

        /// <summary>How many numbers the output holds for this many columns and classes.</summary>
        public static int Length(int columns, int classCount)
        {
            return (BoxRows + classCount) * columns;
        }

        /// <summary>
        /// Adds a box for every column and class whose score is at least <paramref name="minScore"/>. The
        /// table comes row after row. The boxes are in the model's square picture.
        /// </summary>
        public static void AddBoxes(float[] table, int columns, IReadOnlyList<string> classNames,
            float minScore, List<Detection> into)
        {
            for (int classNumber = 0; classNumber < classNames.Count; classNumber++)
            {
                int scoreRow = (BoxRows + classNumber) * columns;
                for (int column = 0; column < columns; column++)
                {
                    if (table[scoreRow + column] >= minScore)
                    {
                        into.Add(new Detection(classNames[classNumber], table[scoreRow + column], table[column],
                            table[columns + column], table[2 * columns + column], table[3 * columns + column]));
                    }
                }
            }
        }
    }
}
