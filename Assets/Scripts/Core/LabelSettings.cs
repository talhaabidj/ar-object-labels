using System;

namespace ArObjectLabels.Core
{
    /// <summary>The numbers a label's rules use. They live in the app's settings file.</summary>
    [Serializable]
    public class LabelSettings
    {
        /// <summary>Below this distance, in metres, the label stops shrinking.</summary>
        public float nearLimit = 0.5f;

        /// <summary>Beyond this distance, in metres, the label stops growing.</summary>
        public float farLimit = 2.5f;

        /// <summary>The label is never wider than this, in metres. 0 means no limit.</summary>
        public float maxWidth = 0f;

        /// <summary>From this distance, in metres, the label starts to fade out.</summary>
        public float fadeStart = 3.5f;

        /// <summary>At this distance, in metres, the label is gone.</summary>
        public float fadeEnd = 4.5f;

        /// <summary>Space between the object's top and the label's lower edge, in metres at 1 m distance.</summary>
        public float gap = 0.02f;

        /// <summary>Space kept between a label and a wall, in metres.</summary>
        public float wallMargin = 0.01f;

        /// <summary>Space kept between two labels, and between a label and another object, in metres at 1 m distance.</summary>
        public float labelMargin = 0.01f;

        /// <summary>How many labels are shown at once at most.</summary>
        public int maxLabels = 6;

        /// <summary>
        /// When labels are chosen, objects count as equally big in the picture if their sizes there differ by
        /// less than this share and fall on the same step of the rule. 0.25 means a quarter.
        /// </summary>
        public float sameSizeShare = 0.25f;

        /// <summary>
        /// A label that is shown counts as this much bigger when the labels to show are chosen. 0.5 means
        /// another object must be half as big again to take its place.
        /// </summary>
        public float stayBonus = 0.5f;

        /// <summary>A moved label goes back to its own place only after no move has been needed for this many seconds.</summary>
        public float holdSeconds = 0.5f;

        /// <summary>A label takes about this many seconds to glide to a new place. 0 makes it jump.</summary>
        public float glideSeconds = 0.2f;

        /// <summary>Thickness of the line from a moved label to its object, in metres at 1 m distance.</summary>
        public float leaderThickness = 0.004f;
    }
}
