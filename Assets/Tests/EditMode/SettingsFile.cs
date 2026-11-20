using UnityEditor;

namespace ArObjectLabels.Tests
{
    /// <summary>The settings file the app's scene uses, so tests check the numbers the app really runs with.</summary>
    static class SettingsFile
    {
        public static AppSettings Load()
        {
            return AssetDatabase.LoadAssetAtPath<AppSettings>("Assets/Settings/AppSettings.asset");
        }
    }
}
