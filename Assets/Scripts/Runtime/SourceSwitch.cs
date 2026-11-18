using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArObjectLabels
{
    /// <summary>
    /// The on-screen button that chooses where detections come from. Each click goes on to the next source;
    /// after the last one, detection is off again. The app starts with detection off.
    /// </summary>
    public class SourceSwitch : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text buttonText;
        [SerializeField] DetectionLabels labels;
        [Tooltip("In the order the button steps through them. Each must be a detection source.")]
        [SerializeField] MonoBehaviour[] sources;

        // The number of the chosen source; the number after the last one stands for "off".
        int chosen;

        void Awake()
        {
            button.onClick.AddListener(ChooseNext);
        }

        void Start()
        {
            chosen = sources.Length;
            Show();
        }

        public void ChooseNext()
        {
            chosen = (chosen + 1) % (sources.Length + 1);
            Show();
        }

        void Show()
        {
            IDetectionSource source = chosen < sources.Length ? sources[chosen] as IDetectionSource : null;
            labels.Use(source);
            buttonText.text = Messages.ForSource(source != null ? source.Name : Messages.NoSource);
        }
    }
}
