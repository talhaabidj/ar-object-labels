using System.Collections.Generic;
using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>
    /// A model is sometimes wrong for a single run. This lets a found object have a label only after it has
    /// been found in several runs in a row, in about the same place, with a score high enough to appear.
    /// Once it has a label it keeps it while it is still found with the lower score for staying, so a score
    /// that wavers around one limit does not make the label blink.
    /// </summary>
    public class ConfirmOverRuns
    {
        readonly List<string> earlierNames = new List<string>();
        readonly List<Vector3> earlierPlaces = new List<Vector3>();
        readonly List<int> earlierRuns = new List<int>();
        readonly List<bool> earlierShown = new List<bool>();
        readonly List<bool> earlierHadClearRun = new List<bool>();
        readonly List<int> runs = new List<int>();
        readonly List<bool> hadClearRun = new List<bool>();
        readonly List<int> continues = new List<int>();
        readonly TrackMatch matcher = new TrackMatch();

        /// <summary>Forgets everything seen so far.</summary>
        public void Forget()
        {
            earlierNames.Clear();
            earlierPlaces.Clear();
            earlierRuns.Clear();
            earlierShown.Clear();
            earlierHadClearRun.Clear();
        }

        /// <summary>
        /// Takes what one run of the model found, with each object's place in the room, and fills
        /// <paramref name="mayShow"/> with true for every object that may have a label now. An object that
        /// is missing from a run is forgotten and has to be found several times again. An object whose box
        /// was cut by the picture's edge in every one of those runs gets no label yet, because a model often
        /// takes a cut-off object for something else. <paramref name="clearOfEdge"/> says, for each box,
        /// whether it was clear of the edge; left out, every box counts as clear.
        /// </summary>
        public void TakeRun(IReadOnlyList<string> names, IReadOnlyList<float> scores, IReadOnlyList<Vector3> places,
            ModelSettings rules, float sameObjectDistance, List<bool> mayShow, IReadOnlyList<bool> clearOfEdge = null)
        {
            matcher.Match(earlierNames, earlierPlaces, names, places, sameObjectDistance, continues);
            mayShow.Clear();
            runs.Clear();
            hadClearRun.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                bool wasShown = continues[i] >= 0 && earlierShown[continues[i]];
                int runsBefore = continues[i] >= 0 ? earlierRuns[continues[i]] : 0;
                bool clearBefore = runsBefore > 0 && earlierHadClearRun[continues[i]];
                // Only a run with the score to appear counts towards the runs in a row.
                bool counts = scores[i] >= rules.minScore;
                runs.Add(counts ? runsBefore + 1 : 0);
                hadClearRun.Add(counts && (clearBefore || clearOfEdge == null || clearOfEdge[i]));
                bool staying = wasShown && scores[i] >= rules.stayScore;
                mayShow.Add(staying || (runs[i] >= rules.runsToAppear && hadClearRun[i]));
            }

            Forget();
            for (int i = 0; i < names.Count; i++)
            {
                earlierNames.Add(names[i]);
                earlierPlaces.Add(places[i]);
                earlierRuns.Add(runs[i]);
                earlierShown.Add(mayShow[i]);
                earlierHadClearRun.Add(hadClearRun[i]);
            }
        }

        /// <summary>
        /// True when no side of the box lies within <paramref name="margin"/> of the picture's edge. The margin
        /// is a share of the picture's width or height; the box is in picture pixels.
        /// </summary>
        public static bool IsClearOfEdge(Detection box, float pictureWidth, float pictureHeight, float margin)
        {
            float side = margin * pictureWidth;
            float topAndBottom = margin * pictureHeight;
            return box.Left > side && box.Right < pictureWidth - side
                && box.Top > topAndBottom && box.Bottom < pictureHeight - topAndBottom;
        }
    }
}
