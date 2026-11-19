using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// The app's own panels and buttons count as more of the picture's edge: a label is kept out from under
    /// them as it is kept inside the picture. Rectangles are in the viewer's flat picture, y up.
    /// </summary>
    public static class PanelEdges
    {
        /// <summary>
        /// A rectangle in screen pixels (y up, from the bottom left) as a rectangle in the flat picture one
        /// metre in front of the camera, whose half width and half height are <paramref name="halfPicture"/>.
        /// </summary>
        public static Rect ToPicture(Rect onScreen, Vector2 screenSize, Vector2 halfPicture)
        {
            float xMin = (onScreen.xMin / screenSize.x * 2f - 1f) * halfPicture.x;
            float xMax = (onScreen.xMax / screenSize.x * 2f - 1f) * halfPicture.x;
            float yMin = (onScreen.yMin / screenSize.y * 2f - 1f) * halfPicture.y;
            float yMax = (onScreen.yMax / screenSize.y * 2f - 1f) * halfPicture.y;
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        /// <summary>
        /// The place moved out from under every panel it touches, away from the picture's edge that panel sits
        /// nearest to: down from a panel at the top, up from one at the bottom, and sideways from one at a side.
        /// </summary>
        public static Rect KeepOut(Rect place, IReadOnlyList<Rect> panels, float margin, Rect picture)
        {
            if (panels == null)
            {
                return place;
            }
            foreach (Rect panel in panels)
            {
                if (!LabelArrange.Touches(place, panel, margin))
                {
                    continue;
                }
                float toTop = picture.yMax - panel.yMax;
                float toBottom = panel.yMin - picture.yMin;
                float toLeft = panel.xMin - picture.xMin;
                float toRight = picture.xMax - panel.xMax;
                float nearest = Mathf.Min(Mathf.Min(toTop, toBottom), Mathf.Min(toLeft, toRight));
                if (nearest == toTop)
                {
                    place.y = panel.yMin - margin - place.height;
                }
                else if (nearest == toBottom)
                {
                    place.y = panel.yMax + margin;
                }
                else if (nearest == toLeft)
                {
                    place.x = panel.xMax + margin;
                }
                else
                {
                    place.x = panel.xMin - margin - place.width;
                }
            }
            return place;
        }

        /// <summary>True when the place touches none of the panels.</summary>
        public static bool IsClear(Rect place, IReadOnlyList<Rect> panels)
        {
            if (panels == null)
            {
                return true;
            }
            foreach (Rect panel in panels)
            {
                if (LabelArrange.Touches(place, panel, 0f))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
