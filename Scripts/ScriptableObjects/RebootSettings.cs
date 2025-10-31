using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
using UnityEngine;

namespace CorePublic.ScriptableObjects
{
    public class RebootSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/Settings/RebootSettings.asset";
        private const string SettingsDirectory = "Assets/Settings";
        
        private static RebootSettings _instance;
        
        public static RebootSettings Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    _instance = AssetDatabase.LoadAssetAtPath<RebootSettings>(SettingsPath);
                    if (_instance == null)
                    {
                        _instance = CreateAndSaveInstance();
                    }
#else
                    // For runtime, we'll use the preloaded asset approach
                    _instance = FindSettingsInPreloadedAssets();
                    if (_instance == null)
                    {
                        Debug.LogError("RebootSettings not found in preloaded assets! Make sure to add it to Player Settings > Preloaded Assets.");
                    }
#endif
                }
                return _instance;
            }
        }

#if UNITY_EDITOR
        private static RebootSettings CreateAndSaveInstance()
        {
            var settings = CreateInstance<RebootSettings>();
            
            if (!AssetDatabase.IsValidFolder(SettingsDirectory))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created RebootSettings at {SettingsPath}");
            return settings;
        }
        
        public static void EnsureInPreloadedAssets()
        {
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            var settingsAsset = Instance;
            
            bool alreadyPreloaded = false;
            foreach (var asset in preloadedAssets)
            {
                if (asset == settingsAsset)
                {
                    alreadyPreloaded = true;
                    break;
                }
            }
            
            if (!alreadyPreloaded)
            {
                var newPreloadedAssets = new Object[preloadedAssets.Length + 1];
                for (int i = 0; i < preloadedAssets.Length; i++)
                {
                    newPreloadedAssets[i] = preloadedAssets[i];
                }
                newPreloadedAssets[preloadedAssets.Length] = settingsAsset;
                PlayerSettings.SetPreloadedAssets(newPreloadedAssets);
                Debug.Log("Added RebootSettings to preloaded assets");
            }
        }
#endif
        
        private static RebootSettings FindSettingsInPreloadedAssets()
        {
#if UNITY_EDITOR
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            foreach (var asset in preloadedAssets)
            {
                if (asset is RebootSettings rebootSettings)
                {
                    return rebootSettings;
                }
            }
            return null;
#else
            // In runtime builds, preloaded assets are automatically loaded at startup
            // We can find them using FindObjectsOfTypeAll since they're already loaded
            var allSettings = Resources.FindObjectsOfTypeAll<RebootSettings>();
            return allSettings.Length > 0 ? allSettings[0] : null;
#endif
        }

        public string gameName;
        public Texture2D gameIcon;
        public string bundleID;
        public string bundleVersion = "1.0.0";
        public int iosBuildNumber = 1;
        public int androidVersionCode = 1;
        public string remoteConfigEnvironmentIDAndroid = "xxx_android_xxx";
        public string remoteConfigEnvironmentIDIOS = "xxx_ios_xxx";
        public string remoteConfigEnvironmentIDDebug = "xxx_dev_xxx";

#if UNITY_EDITOR
        public void UpdateProjectSettings()
        {
            PlayerSettings.companyName = "Reboot Interactive";
            PlayerSettings.productName = gameName;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleID);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleID);
            PlayerSettings.bundleVersion = bundleVersion;
            PlayerSettings.Android.bundleVersionCode = androidVersionCode;            
            PlayerSettings.iOS.buildNumber = iosBuildNumber.ToString();
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { gameIcon }, IconKind.Any);

            // Ensure this settings asset is in preloaded assets for runtime access
            EnsureInPreloadedAssets();

            Debug.Log("Project settings updated");
        }

        public bool ReadyToBuild(out string error)
        {
            error = "";
            bool fieldsFull = gameName != "" && bundleID != "" && iosBuildNumber >= 0 &&
                              remoteConfigEnvironmentIDAndroid != "" && remoteConfigEnvironmentIDIOS != "";
            if (!fieldsFull)
            {
                error = "Reboot settings fields are not fulfilled";
                return false;
            }

            if (PlayerSettings.companyName != "Reboot Interactive" ||
                PlayerSettings.productName != gameName ||
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS) != bundleID ||
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) != bundleID ||
                PlayerSettings.Android.bundleVersionCode != androidVersionCode ||
                PlayerSettings.iOS.buildNumber != iosBuildNumber.ToString() ||
                PlayerSettings.bundleVersion != bundleVersion ||
                PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown)[0] != gameIcon)
            {
                error = "Reboot settings fields are not synced with player settings!";
                return false;
            }

            return true;
        }
#endif
    }
}