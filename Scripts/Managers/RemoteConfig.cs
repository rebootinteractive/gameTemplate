using System.Threading.Tasks;
using CorePublic.Helpers;
using CorePublic.Interfaces;
using CorePublic.ScriptableObjects;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace CorePublic.Managers
{
    public class RemoteConfig : BaseManager<RemoteConfig>, IStats, IPrerequisite
    {
        private struct UserAttributes {
            public bool expansionFlag;
        }

        private struct AppAttributes {
            public int level;
            public int score;
            public string appVersion;
        }
    
        [SerializeField] protected bool showDebugLogs;
    
        protected bool IsReady;

        protected void Start()
        {
            _ = InitializeRemoteService();
        }

        protected virtual async Task InitializeRemoteService()
        {
            if (Utilities.CheckForInternetConnection()) 
            {
                await InitializeRemoteConfigAsync();
            }
        
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;

            var rebootSettings = RebootSettings.Instance;

            string environmentID = "";
            
            if (rebootSettings)
            {
                if (Debug.isDebugBuild)
                {
                    environmentID = rebootSettings.remoteConfigEnvironmentIDDebug;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    environmentID = rebootSettings.remoteConfigEnvironmentIDAndroid;    
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    environmentID = rebootSettings.remoteConfigEnvironmentIDIOS;
                }
            }
            else
            {
                Debug.LogError("RebootSettings not found!");
                return;
            }
        
            RemoteConfigService.Instance.SetEnvironmentID(environmentID);
            RemoteConfigService.Instance.FetchConfigs(new UserAttributes(), new AppAttributes());
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
    
        async Task InitializeRemoteConfigAsync()
        {
            await UnityServices.InitializeAsync();
        
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
    

        private void ApplyRemoteConfig(ConfigResponse configResponse)
        {
            switch (configResponse.requestOrigin) {
                case ConfigOrigin.Default:
                    Debug.Log("No settings loaded this session; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    Debug.Log("No settings loaded this session; using cached values from a previous session.");
                    break;
                case ConfigOrigin.Remote:
                    Debug.Log("New settings loaded this session; update values accordingly.");
                    break;
            }

            IsReady = true;
        
        }
    
        public virtual float GetFloat(string key, float defaultValue = -1)
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
            return RemoteConfigService.Instance.appConfig.GetFloat(key, defaultValue);
        }
    
        public virtual int GetInt(string key, int defaultValue = -1)
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
            return RemoteConfigService.Instance.appConfig.GetInt(key, defaultValue);
        }
    
        public virtual string GetString(string key, string defaultValue = "")
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
            return RemoteConfigService.Instance.appConfig.GetString(key, defaultValue);
        }
    
        public virtual bool GetBool(string key, bool defaultValue = false)
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
            return RemoteConfigService.Instance.appConfig.GetBool(key, defaultValue);
        }
    
        public virtual Vector3 GetVector3(string key, Vector3 defaultValue)
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
        
            var json = RemoteConfigService.Instance.appConfig.GetJson(key, "");
            try
            {
                return JsonUtility.FromJson<Vector3>(json);
            }
            catch
            {
                Debug.LogError("Vector data is not valid: " + key);
            }

            return defaultValue;
        }
    
    
        public virtual bool HasKey(string key)
        {
            return RemoteConfigService.Instance.appConfig.HasKey(key);
        }
    
        public virtual string GetJson(string key, string defaultValue = "")
        {
            if (!HasKey(key))
            {
                Debug.LogWarning("Key not found: " + key);
                return defaultValue;
            }
            return RemoteConfigService.Instance.appConfig.GetJson(key, defaultValue);
        }


        public virtual string GetStats()
        {
            if (!showDebugLogs) return null;
        
            if (!IsReady)
            {
                return "<color=#FF0000> Remote config is not ready! </color>";
            }
        
            var stats = "";
        
            var properties = RemoteConfigService.Instance.appConfig.GetKeys();
            foreach (var key in properties)
            {
                stats += key + ": " + RemoteConfigService.Instance.appConfig.config.GetValue(key) + "\n";
            }
        
            return stats;
        }

        protected virtual void OnApplicationQuit()
        {
            RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteConfig;
        }

        public bool IsMet()
        {
            return IsReady;
        }
    }
}

