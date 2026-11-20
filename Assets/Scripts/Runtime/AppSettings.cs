using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>The tunable numbers of the labels, the detection and the model, in one file.</summary>
    [CreateAssetMenu(menuName = "AR Object Labels/App Settings", fileName = "AppSettings")]
    public class AppSettings : ScriptableObject
    {
        public LabelSettings label = new LabelSettings();
        public DetectionSettings detection = new DetectionSettings();
        public ModelSettings model = new ModelSettings();
    }
}
