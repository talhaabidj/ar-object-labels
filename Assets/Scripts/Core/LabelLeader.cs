namespace ArObjectLabels.Core
{
    /// <summary>When the line from a moved label to its object is drawn.</summary>
    public static class LabelLeader
    {
        /// <summary>
        /// True for a line of some length that is no longer than <paramref name="maxPlates"/> times the plate's
        /// height. A longer line crosses other things and says little about which object the label names.
        /// </summary>
        public static bool IsDrawn(float length, float plateHeight, float maxPlates)
        {
            return length >= 0.0001f && length <= maxPlates * plateHeight;
        }
    }
}
