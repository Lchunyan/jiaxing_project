using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse
{
    /// <summary>
    /// Singleton monobehavior.
    /// </summary>
    public class SingletonMonobehaviour<T> : MonoBehaviour, I_Singleton
        where T : SingletonMonobehaviour<T>
    {
        private const string kMsg = "SingletonMonobehaviour: {0} is NULL !";
        static T instance;
        static System.Object instanceReference = null;

        /// <summary>
        /// 判断 单例对象是否有调用成功。
        /// </summary>
        /// <returns><c>true</c> if has instance; otherwise, <c>false</c>.</returns>
        public static bool HasInstance()
        {
            return instanceReference != null;
        }

        /// <summary>
        /// 获取一个singleton behavior 的单例对象引用。 
        /// 注意: 这个方法会调用 FindObjectsOfType, 如果只是为了得知 instance 是否有创建成功，请调用 HasInstance()
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                //Editor 下和 Runtime 有所区别，主要是对Null的检查:
                if(Application.isEditor)
                {
                    if (!instance)
                    {
                        instance = FindObjectOfType<T>();
                        instanceReference = instance;
                        if (instanceReference == null)
                        {
                            Debug.LogErrorFormat(kMsg, typeof(T).FullName);
                        }
                    }
                }
                else
                {
                    //runtime 下用 c# object 判断null:
                    if(instanceReference == null)
                    {
                        instance = FindObjectOfType<T>();
                        instanceReference = instance;
                        if (instanceReference == null)
                        {
                            Debug.LogErrorFormat(kMsg, typeof(T).FullName);
                        }
                    }
                }
               
                return instance;
            }
        }

        #region I_Singleton implementation

        /// <summary>
        /// Sets the singleton.
        /// </summary>
        public virtual void SetSingleton()
        {
            bool isPlaying = Application.isPlaying;
#if UNITY_EDITOR
            isPlaying = UnityEditor.EditorApplication.isPlaying ||
                UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#endif
            instance = (T)this;
            instanceReference = instance;
            var interfaces = typeof(T).GetInterfaces();
            var iSingletonInterfaceType = typeof(I_Singleton);
            for (int i = 0; i < interfaces.Length; i++)
            {
                System.Type intf = interfaces[i];
                if (iSingletonInterfaceType != intf && iSingletonInterfaceType.IsAssignableFrom(intf))
                {
                    SingletonFactory.RegisterSingleton(this, intf);
                }
            }
        }

        #endregion

        protected virtual void Awake()
        {
            SetSingleton();
        }

        protected virtual void OnDestroy()
        {
            instance = null;
            instanceReference = null;
            var interfaces = typeof(T).GetInterfaces();
            var iSingletonInterfaceType = typeof(I_Singleton);
            for (int i = 0; i < interfaces.Length; i++)
            {
                System.Type intf = interfaces[i];
                if (iSingletonInterfaceType != intf && iSingletonInterfaceType.IsAssignableFrom(intf))
                {
                    SingletonFactory.UnregisterSingleton(this, intf);
                }
            }
        }
    }
}