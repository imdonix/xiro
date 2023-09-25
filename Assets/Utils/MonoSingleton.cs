using UnityEngine;

namespace Utils
{
    public abstract class MonoSingleton<T> : MonoBehaviour
    {
        public static T Instance { private set; get; }

        #region UNITY

        private void Awake()
        {
            if (ReferenceEquals(Instance, null) || OverrideInstance())
            {
                MonoSingleton<T>.Instance = gameObject.GetComponent<T>();
                Init();

                if (!OverrideInstance())
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }


        #endregion

        protected virtual bool OverrideInstance()
        {
            return true;
        }

        protected abstract void Init();

    }
}
