using ArObjectLabels.Core;
using UnityEngine;

namespace ArObjectLabels
{
    /// <summary>The app's tunable numbers, in one file.</summary>
    [CreateAssetMenu(menuName = "AR Object Labels/App Settings", fileName = "AppSettings")]
    public class AppSettings : ScriptableObject
    {
        public LabelSettings label = new LabelSettings();
    }
}
