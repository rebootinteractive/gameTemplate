using System;
using System.Collections.Generic;
using UnityEngine;

namespace CorePublic.Helpers
{
    /// <summary>
    ///     Clears every Singleton&lt;T&gt; static instance at the start of each play session.
    ///     Needed because [RuntimeInitializeOnLoadMethod] is never invoked on open generic
    ///     types, and because statics survive between play sessions when
    ///     "Enter Play Mode Options > Reload Domain" is disabled.
    /// </summary>
    internal static class SingletonStaticReset
    {
        private static readonly List<Action> Resetters = new List<Action>();

        internal static void Register(Action resetter)
        {
            Resetters.Add(resetter);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAll()
        {
            for (var i = 0; i < Resetters.Count; i++) Resetters[i].Invoke();
        }
    }

    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        #region Fields

        /// <summary>
        ///     The instance.
        /// </summary>
        private static T _instance;

        /// <summary>
        ///     Set when this component lost the race in Awake and is being destroyed as a
        ///     duplicate. A duplicate must never clear the surviving static instance.
        /// </summary>
        private bool _isDuplicate;

        #endregion

        #region Properties

        /// <summary>
        ///     Registers this closed generic type's reset hook exactly once per domain.
        ///     Declaring an explicit static constructor also removes beforefieldinit, so this
        ///     runs before the first access to _instance.
        /// </summary>
        static Singleton()
        {
            SingletonStaticReset.Register(() => _instance = null);
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var found = FindObjectOfType<T>();

#if UNITY_EDITOR
                // Never cache outside play mode. An editor tool or custom inspector touching
                // Instance would otherwise pin the static to an edit-mode object, which then
                // survives into play mode when Reload Domain is disabled and makes Awake
                // destroy the real manager as a "duplicate".
                if (!Application.isPlaying) return found;
#endif

                _instance = found;
                return _instance;
            }
        }

        public bool dontDestroyOnLoad = true;
        public bool destroyGameObjectOnDestroy = true;

        #endregion

        #region Methods

        /// <summary>
        ///     Use this for initialization.
        /// </summary>
        protected virtual void Awake()
        {
            // _instance == this is legitimate re-entry: with "Reload Scene" disabled the very
            // same GameObject is reused across play sessions, so the static may already point
            // at us. Only a different, still-alive object is a real duplicate.
            if (_instance != null && _instance != this)
            {
                _isDuplicate = true;
                Debug.LogWarning(
                    $"There is already an instance of {typeof(T).Name} in the scene. Destroying the new instance.");
                if (destroyGameObjectOnDestroy) Destroy(gameObject);
                else Destroy(this);
                return;
            }

            _instance = this as T;

            if (dontDestroyOnLoad)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                Debug.Log($"DontDestroyOnLoad: {gameObject.name}");
            }
        }

        public static T Request()
        {
            if (!_instance) Debug.LogWarning("There is no instance of " + typeof(T).Name + " in the scene");

            return _instance;
        }

        public static T ForceRequest()
        {
            if (_instance == null)
            {
                var ownerObject = new GameObject();
                _instance = ownerObject.AddComponent<T>();
                ownerObject.name = typeof(T).ToString();
            }

            return _instance;
        }

        public bool IsInstace()
        {
            Debug.Log($"IsInstace: {_instance == this}");
            return _instance == this;
        }

        public void ReleaseInstance()
        {
            // A duplicate never owned the static, so it has nothing to release. Returning
            // silently removes the "but it is not the instance" spam on every scene load.
            if (_isDuplicate) return;

            if (_instance == null)
            {
                Debug.LogWarning($"Instance of {typeof(T).Name} is null");
                return;
            }

            if (_instance == this)
            {
                _instance = null;
            }
            else
            {
                Debug.LogWarning($"Trying to release instance of {typeof(T).Name} but it is not the instance");
            }
        }

        public void OnDestroy()
        {
            ReleaseInstance();
        }

        #endregion
    }
}
