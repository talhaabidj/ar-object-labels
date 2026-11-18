using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Shows one label for each object the chosen source finds, at the place the placer gives it. A found
    /// object with no place in the room gets no label. With no source chosen, no label shows. An object
    /// found again near where it was keeps its label's memory, so the label stays steady. What the model
    /// finds gets its label only after several runs in a row.
    /// </summary>
    public class DetectionLabels : MonoBehaviour
    {
        [Tooltip("The source used when the app starts. Leave empty to start with none.")]
        [SerializeField] MonoBehaviour source;
        [SerializeField] DetectionPlacer placer;
        [SerializeField] WorldLabel labelPrefab;
        [SerializeField] TextAsset displayNamesFile;
        [SerializeField] AppSettings settings;

        readonly List<Detection> detections = new List<Detection>();
        readonly List<WorldLabel> labels = new List<WorldLabel>();
        readonly List<string> names = new List<string>();
        readonly List<Vector3> places = new List<Vector3>();
        readonly List<LabelMemory> memories = new List<LabelMemory>();
        readonly List<string> earlierNames = new List<string>();
        readonly List<Vector3> earlierPlaces = new List<Vector3>();
        readonly List<LabelMemory> earlierMemories = new List<LabelMemory>();
        readonly List<int> continues = new List<int>();
        readonly List<PlacedDetection> placedNow = new List<PlacedDetection>();
        readonly ConfirmOverRuns confirm = new ConfirmOverRuns();
        readonly List<string> runNames = new List<string>();
        readonly List<float> runScores = new List<float>();
        readonly List<Vector3> runPlaces = new List<Vector3>();
        readonly List<bool> mayShow = new List<bool>();
        float lastReadingTime = float.NegativeInfinity;
        Dictionary<string, string> displayNames;
        IDetectionSource chosen;

        /// <summary>Chooses where the boxes come from. Null means nowhere.</summary>
        public void Use(IDetectionSource newSource)
        {
            chosen = newSource;
            placedNow.Clear();
            confirm.Forget();
            lastReadingTime = float.NegativeInfinity;
        }

        void Awake()
        {
            chosen = source as IDetectionSource;
            displayNames = ClassNames.ParseDisplayNames(displayNamesFile != null ? displayNamesFile.text : "");
        }

        void OnDisable()
        {
            ShowOnly(0);
        }

        void Update()
        {
            if (chosen != null && chosen.TryRead(detections, out CameraFrame frame))
            {
                // A source that needs confirming gives the same reading until its next one is ready. Each
                // reading is placed and judged once.
                if (!chosen.NeedsConfirming)
                {
                    PlaceAll(frame);
                }
                else if (frame.Time != lastReadingTime)
                {
                    PlaceAll(frame);
                    KeepTheConfirmed();
                    lastReadingTime = frame.Time;
                }
            }
            else
            {
                placedNow.Clear();
            }

            names.Clear();
            places.Clear();
            for (int i = 0; i < placedNow.Count; i++)
            {
                PlacedDetection placed = placedNow[i];
                WorldLabel label = LabelNumber(i);
                string text = ClassNames.LabelFor(placed.Detection.ClassName, displayNames);
                label.SetText(text);
                label.PlaceAt(placed.RoomBox, placed.Turn);
                label.Score = placed.Detection.Score;
                names.Add(text);
                places.Add(placed.RoomBox.center);
            }
            HandOnMemories();
            ShowOnly(placedNow.Count);
        }

        void PlaceAll(CameraFrame frame)
        {
            placedNow.Clear();
            foreach (Detection detection in detections)
            {
                if (placer.TryPlace(detection, frame, out PlacedDetection placed))
                {
                    placedNow.Add(placed);
                }
            }
        }

        // Leaves only the objects that have been found often enough in a row to have a label.
        void KeepTheConfirmed()
        {
            runNames.Clear();
            runScores.Clear();
            runPlaces.Clear();
            foreach (PlacedDetection placed in placedNow)
            {
                runNames.Add(placed.Detection.ClassName);
                runScores.Add(placed.Detection.Score);
                runPlaces.Add(placed.RoomBox.center);
            }
            confirm.TakeRun(runNames, runScores, runPlaces, settings.model, settings.detection.sameObjectDistance, mayShow);
            for (int i = placedNow.Count - 1; i >= 0; i--)
            {
                if (!mayShow[i])
                {
                    placedNow.RemoveAt(i);
                }
            }
        }

        // Labels are handed out in the order objects are found, which changes from frame to frame. What a
        // label remembers belongs to its object, so each object found again gets its memory back.
        void HandOnMemories()
        {
            TrackMatch.Match(earlierNames, earlierPlaces, names, places, settings.detection.sameObjectDistance, continues);
            memories.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                memories.Add(continues[i] >= 0 ? earlierMemories[continues[i]] : new LabelMemory());
                labels[i].Memory = memories[i];
            }

            earlierNames.Clear();
            earlierNames.AddRange(names);
            earlierPlaces.Clear();
            earlierPlaces.AddRange(places);
            earlierMemories.Clear();
            earlierMemories.AddRange(memories);
        }

        WorldLabel LabelNumber(int index)
        {
            while (labels.Count <= index)
            {
                labels.Add(Instantiate(labelPrefab, transform));
            }
            labels[index].gameObject.SetActive(true);
            return labels[index];
        }

        void ShowOnly(int count)
        {
            for (int i = count; i < labels.Count; i++)
            {
                if (labels[i] != null)
                {
                    labels[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
