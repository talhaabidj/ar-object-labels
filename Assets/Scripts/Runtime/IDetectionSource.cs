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
        /// Fills the list with what is found now and gives the camera's pose that belongs to it. False when
        /// the source has nothing to give.
        /// </summary>
        bool TryRead(List<Detection> into, out CameraFrame frame);
    }
}
