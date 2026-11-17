namespace ArObjectLabels.Core
{
    /// <summary>Tells a click from a drag by how far the pointer moved while the button was held.</summary>
    public static class ClickRule
    {
        /// <summary>
        /// A press counts as a click when the pointer never moved further from where it went down than a small
        /// share of the screen height. The share lets a tap that slips a few pixels still count as a click.
        /// </summary>
        public static bool IsClick(float farthestMovePixels, float screenHeightPixels, float maxMoveShareOfHeight)
        {
            return farthestMovePixels <= screenHeightPixels * maxMoveShareOfHeight;
        }
    }
}
