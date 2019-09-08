using UnityEngine;

namespace vFrame.Core.Singletons
{
    public class MonoSingleton<T> : MonoBehaviour where T: MonoBehaviour 
    {
        private static T _instance;
        private static bool _instanceCreated;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this as T)
            {
                Destroy(this);
                return;
            }

            _instance = this as T;
            _instanceCreated = true;
        }

        protected void OnDestroy()
        {
            if (_instance != this as T)
                return;
            
            _instance = null;
            _instanceCreated = false;
        }

        public static T Instance
        {
            get
            {
                if (_instanceCreated)
                    return _instance;
                
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                _instanceCreated = true;
                
                return _instance;
            }
        }
    }
}