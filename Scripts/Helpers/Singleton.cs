using UnityEngine;

namespace CorePublic.Helpers
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        #region Fields

        /// <summary>
        ///     The instance.
        /// </summary>
        private static T _instance;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                // Never cache in edit mode: with domain reload disabled (Enter Play
                // Mode Options) the static survives into play mode still pointing at
                // the live scene object, so Awake would treat the real singleton as a
                // duplicate and destroy it.
                if (!Application.isPlaying) return FindObjectOfType<T>();
#endif
                if (_instance == null) _instance = FindObjectOfType<T>();

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
            // _instance == this happens when the static already points at this very
            // object (e.g. it was resolved before Awake ran, or survived a play-mode
            // transition without domain reload) — that is not a duplicate.
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                if (dontDestroyOnLoad) {
                    transform.SetParent(null);
                    DontDestroyOnLoad(gameObject);
                    Debug.Log($"DontDestroyOnLoad: {gameObject.name}");
                }
            }
            else
            {
                Debug.LogWarning($"There is already an instance of {typeof(T).Name} in the scene. Destroying the new instance.");
                if (destroyGameObjectOnDestroy) Destroy(gameObject);
                else Destroy(this);
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

        public bool IsInstace(){
            Debug.Log($"IsInstace: {_instance == this}");
            return _instance == this;
        }

        public void ReleaseInstance()
        {
            if(_instance == null){
                Debug.LogWarning($"Instance of {typeof(T).Name} is null");
                return;
            }
            if (_instance == this)
            {
                _instance = null;
            }else{
                Debug.LogWarning($"Trying to release instance of {typeof(T).Name} but it is not the instance");
            }
        }


        public void OnDestroy()
        {
            // Quiet release: destroyed duplicates must not warn, and the static must
            // always be cleared for the real instance so it can't leak across play
            // sessions when domain reload is disabled.
            if (_instance == this) _instance = null;
        }

        #endregion
    }
}