using System.Collections;
using CorePublic.Managers;
using UnityEngine;  
using UnityEngine.Events;

namespace CorePublic.Helpers
{
    public class RemoteConfigBoolEvent : MonoBehaviour
    {
        public string key;
        public UnityEvent<bool> RemoteBoolEvent;
        public UnityEvent RemoteBoolTrueEvent;
        public UnityEvent RemoteBoolFalseEvent;

        public enum InvokeType
        {
            OnStart,
            LateStart,
            OnEnable,
            OnDisable,
            OnDestroy,
            Manual
        }

        [SerializeField] private InvokeType invokeType = InvokeType.OnStart;

        private void Start()
        {
            if(invokeType == InvokeType.OnStart)
            {
                RetrieveRemoteConfig();
            }
            else if(invokeType == InvokeType.LateStart)
            {
                StartCoroutine(LateStart());
            }
        }    

        private IEnumerator LateStart()
        {
            yield return null;
            if(invokeType == InvokeType.LateStart)
            {
                RetrieveRemoteConfig();
            }
        }

        private void OnEnable()
        {
            if(invokeType == InvokeType.OnEnable)
            {
                RetrieveRemoteConfig();
            }
        }

        private void OnDisable()
        {
            if(invokeType == InvokeType.OnDisable)
            {
                RetrieveRemoteConfig();
            }
        }   

        private void OnDestroy()
        {
            if(invokeType == InvokeType.OnDestroy)
            {
                RetrieveRemoteConfig();
            }
        }

        public void RetrieveRemoteConfig()
        {
            if(RemoteConfig.Instance && RemoteConfig.Instance.HasKey(key))
            {
                bool value = RemoteConfig.Instance.GetBool(key, false);
                if(value)
                {
                    RemoteBoolTrueEvent.Invoke();
                }
                else
                {
                    RemoteBoolFalseEvent.Invoke();
                }
                RemoteBoolEvent.Invoke(value);
            }
        }
    }
}