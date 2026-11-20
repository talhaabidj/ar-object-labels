using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>The app's tunable numbers, in one file: labels, detection, the model, and the cube and hints.</summary>
    [CreateAssetMenu(menuName = "AR Object Labels/App Settings", fileName = "AppSettings")]
    public class AppSettings : ScriptableObject
    {
        public LabelSettings label = new LabelSettings();
        public DetectionSettings detection = new DetectionSettings();
        public ModelSettings model = new ModelSettings();
        public InteractionSettings interaction = new InteractionSettings();
    }
}
