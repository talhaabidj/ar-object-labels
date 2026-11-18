using System.Collections.Generic;
using ArObjectLabels.Core;

namespace ArObjectLabels
{
    /// <summary>Something that finds objects in the camera's picture.</summary>
    public interface IDetectionSource
    {
        /// <summary>The short name the source switch shows.</summary>
        string Name { get; }

        /// <summary>
        /// True for a source that can be wrong for a single reading: what it finds gets a label only after
        /// it has been found several times in a row.
        /// </summary>
        bool NeedsConfirming { get; }

        /// <summary>
        /// Fills the list with what is found now and gives the camera's pose that belongs to it. False when
        /// the source has nothing to give.
        /// </summary>
        bool TryRead(List<Detection> into, out CameraFrame frame);
    }
}
