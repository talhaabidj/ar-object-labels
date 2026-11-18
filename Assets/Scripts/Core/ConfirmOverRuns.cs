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
        readonly List<int> runs = new List<int>();
        readonly List<int> continues = new List<int>();

        /// <summary>Forgets everything seen so far.</summary>
        public void Forget()
        {
            earlierNames.Clear();
            earlierPlaces.Clear();
            earlierRuns.Clear();
            earlierShown.Clear();
        }

        /// <summary>
        /// Takes what one run of the model found, with each object's place in the room, and fills
        /// <paramref name="mayShow"/> with true for every object that may have a label now. An object that
        /// is missing from a run is forgotten and has to be found several times again.
        /// </summary>
        public void TakeRun(IReadOnlyList<string> names, IReadOnlyList<float> scores, IReadOnlyList<Vector3> places,
            ModelSettings rules, float sameObjectDistance, List<bool> mayShow)
        {
            TrackMatch.Match(earlierNames, earlierPlaces, names, places, sameObjectDistance, continues);
            mayShow.Clear();
            runs.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                bool wasShown = continues[i] >= 0 && earlierShown[continues[i]];
                int runsBefore = continues[i] >= 0 ? earlierRuns[continues[i]] : 0;
                // Only a run with the score to appear counts towards the runs in a row.
                runs.Add(scores[i] >= rules.minScore ? runsBefore + 1 : 0);
                bool staying = wasShown && scores[i] >= rules.stayScore;
                mayShow.Add(staying || runs[i] >= rules.runsToAppear);
            }

            Forget();
            for (int i = 0; i < names.Count; i++)
            {
                earlierNames.Add(names[i]);
                earlierPlaces.Add(places[i]);
                earlierRuns.Add(runs[i]);
                earlierShown.Add(mayShow[i]);
            }
        }
    }
}
