using System.Collections.Generic;
using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>
    /// Shows one label for each object the chosen source finds, at the place the placer gives it. A found
    /// object with no place in the room gets no label. With no source chosen, no label shows.
    /// </summary>
    public class DetectionLabels : MonoBehaviour
    {
        [Tooltip("The source used when the app starts. Leave empty to start with none.")]
        [SerializeField] MonoBehaviour source;
        [SerializeField] DetectionPlacer placer;
        [SerializeField] WorldLabel labelPrefab;
        [SerializeField] TextAsset displayNamesFile;

        readonly List<Detection> detections = new List<Detection>();
        readonly List<WorldLabel> labels = new List<WorldLabel>();
        Dictionary<string, string> displayNames;
        IDetectionSource chosen;

        /// <summary>Chooses where the boxes come from. Null means nowhere.</summary>
        public void Use(IDetectionSource newSource)
        {
            chosen = newSource;
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
            int shown = 0;
            if (chosen != null && chosen.TryRead(detections, out CameraFrame frame))
            {
                foreach (Detection detection in detections)
                {
                    if (placer.TryPlace(detection, frame, out PlacedDetection placed))
                    {
                        WorldLabel label = LabelNumber(shown);
                        label.SetText(ClassNames.LabelFor(detection.ClassName, displayNames));
                        label.PlaceAt(placed.RoomBox);
                        label.Score = detection.Score;
                        shown++;
                    }
                }
            }
            ShowOnly(shown);
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
