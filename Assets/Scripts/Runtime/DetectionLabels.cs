using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Shows one label for each object the chosen source finds, at the place the placer gives it. A found
    /// object with no place in the room gets no label. With no source chosen, no label shows. A label stays at
    /// its object's place in the room: out of view it waits there, and in view but no longer found it fades
    /// out after a moment. What the model finds gets its label only after several runs in a row.
    /// </summary>
    public class DetectionLabels : MonoBehaviour
    {
        [Tooltip("The source used when the app starts. Leave empty to start with none.")]
        [SerializeField] MonoBehaviour source;
        [SerializeField] DetectionPlacer placer;
        [SerializeField] WorldLabel labelPrefab;
        [SerializeField] TextAsset displayNamesFile;
        [SerializeField] AppSettings settings;
        [SerializeField] Camera viewCamera;

        readonly List<Detection> detections = new List<Detection>();
        readonly List<WorldLabel> labels = new List<WorldLabel>();
        readonly List<PlacedDetection> placedNow = new List<PlacedDetection>();
        readonly ConfirmOverRuns confirm = new ConfirmOverRuns();
        readonly LabelKeeper keeper = new LabelKeeper();
        readonly List<string> runNames = new List<string>();
        readonly List<float> runScores = new List<float>();
        readonly List<Vector3> runPlaces = new List<Vector3>();
        readonly List<bool> mayShow = new List<bool>();
        readonly List<Bounds> runBoxes = new List<Bounds>();
        readonly List<Quaternion> runTurns = new List<Quaternion>();
        float lastReadingTime = float.NegativeInfinity;
        Dictionary<string, string> displayNames;
        IDetectionSource chosen;
        System.Func<Vector3, bool> isInView;

        /// <summary>Chooses where the boxes come from. Null means nowhere.</summary>
        public void Use(IDetectionSource newSource)
        {
            chosen = newSource;
            placedNow.Clear();
            confirm.Forget();
            keeper.Forget();
            lastReadingTime = float.NegativeInfinity;
        }

        void Awake()
        {
            chosen = source as IDetectionSource;
            displayNames = ClassNames.ParseDisplayNames(displayNamesFile != null ? displayNamesFile.text : "");
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
            isInView = IsInView;
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

            KeepInTheRoom();

            IReadOnlyList<KeptObject> kept = keeper.Kept;
            for (int i = 0; i < kept.Count; i++)
            {
                KeptObject one = kept[i];
                WorldLabel label = LabelNumber(i);
                label.SetText(ClassNames.LabelFor(one.Name, displayNames));
                label.PlaceAt(one.RoomBox, one.Turn);
                label.Score = one.Score;
                label.Fade = one.Fade;
                label.Memory = one.Memory;
            }
            ShowOnly(kept.Count);
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

        // Hands what is found now to the keeper, which keeps every label at its object's place in the room.
        void KeepInTheRoom()
        {
            runNames.Clear();
            runBoxes.Clear();
            runTurns.Clear();
            runScores.Clear();
            foreach (PlacedDetection placed in placedNow)
            {
                runNames.Add(placed.Detection.ClassName);
                runBoxes.Add(placed.RoomBox);
                runTurns.Add(placed.Turn);
                runScores.Add(placed.Detection.Score);
            }
            keeper.Take(runNames, runBoxes, runTurns, runScores, settings.detection.sameObjectDistance);
            LabelSettings rules = settings.label;
            keeper.Age(isInView, Time.deltaTime, rules.lostWaitSeconds, rules.lostFadeSeconds);
        }

        bool IsInView(Vector3 place)
        {
            if (viewCamera == null)
            {
                return false;
            }
            Vector3 seen = viewCamera.WorldToViewportPoint(place);
            return seen.z > 0f && seen.x >= 0f && seen.x <= 1f && seen.y >= 0f && seen.y <= 1f;
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
