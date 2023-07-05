using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Ximmerse
{
    public static class SingletonFactory
    {
        static Dictionary<System.Type, object> singltonRegistery = new Dictionary<System.Type, object>();
        const string kWarning01 = "Singleton instance: {0} is registered but it's null!";
        const string kWarning02 = "Singleton instance: {0} is registered but it doesn't match the type, actual type is{1}";
        const string kWarning03 = "Singleton instance: {0} has not been registered!";
        const string kWarning04 = "RegisterSingleton method2 parameter error: {0} is not I_Singleton !";
        const string kWarning05 = "Singleton type : {0} has already been registered as object: {1}, it'll be replaced by the new one: {2}";
#if UNITY_EDITOR
        /// <summary>
        /// Gets the singlton registery editor only.
        /// 用于代码检查， EditorOnly
        /// </summary>
        /// <value>The singlton registery editor only.</value>
        public static Dictionary<System.Type, object> SingltonRegistery_EditorOnly
        {
            get
            {
                return singltonRegistery;
            }
        }
#endif

        /// <summary>
        /// Registers the singleton.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void RegisterSingleton<T>(T singleton) where T : class, Ximmerse.I_Singleton
        {
            System.Type singletonType = typeof(T);
            if (singltonRegistery.ContainsKey(singletonType))
            {
                if (singltonRegistery[singletonType] != null && singltonRegistery[singletonType] != singleton)
                {
                    Debug.LogWarningFormat(kWarning05, singletonType.ToString(), singltonRegistery[singletonType].ToString(), singleton.ToString());
                    singltonRegistery[singletonType] = singleton;
                }
                else if (singltonRegistery[singletonType] == null)
                {
                    singltonRegistery[singletonType] = singleton;
                }
            }
            else
            {
                singltonRegistery.Add(singletonType, singleton);
            }
        }

        /// <summary>
        /// Registers a singleton interface implemenation object
        /// </summary>
        /// <param name="singleton">Singleton.</param>
        public static void RegisterSingleton(I_Singleton singleton, System.Type singletonType)
        {
            var iSingletonType = typeof(I_Singleton);
            if (!(iSingletonType.IsAssignableFrom(singletonType)))
            {
                Debug.LogErrorFormat(kWarning04, singletonType.Name);
                return;
            }
            if (singltonRegistery.ContainsKey(singletonType))
            {
                if (singltonRegistery[singletonType] != null && singltonRegistery[singletonType] != singleton)
                {
                    Debug.LogWarningFormat(kWarning05, singletonType.ToString(), singltonRegistery[singletonType].ToString(), singleton.ToString());
                    singltonRegistery[singletonType] = singleton;
                }
                else if (singltonRegistery[singletonType] == null)
                {
                    singltonRegistery[singletonType] = singleton;
                }
            }
            else
            {
                singltonRegistery.Add(singletonType, singleton);
            }

            //注册类型的 I_Singleton 接口实现类.
            var interfacesTypes = singletonType.GetInterfaces();
            for (int i = 0; i < interfacesTypes.Length; i++)
            {
                Type interfaceType = interfacesTypes[i];
                if (iSingletonType.IsAssignableFrom(interfaceType))
                {
                    singltonRegistery.AddOrSetDictionary(interfaceType, singleton);
                }
            }
        }

        /// <summary>
        /// Unregisters the singleton.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void UnregisterSingleton(I_Singleton singleton, System.Type singletonType)
        {
            if (singltonRegistery.ContainsKey(singletonType) && singltonRegistery[singletonType] == singleton)
            {
                singltonRegistery.Remove(singletonType);
            }

            var iSingletonType = typeof(I_Singleton);
            //反向注册类型的 I_Singleton 接口实现类.
            var interfacesTypes = singletonType.GetInterfaces();
            for (int i = 0; i < interfacesTypes.Length; i++)
            {
                Type interfaceType = interfacesTypes[i];
                if (iSingletonType.IsAssignableFrom(interfaceType))
                {
                    singltonRegistery.Remove(interfaceType);
                }
            }
        }

        /// <summary>
        /// Unregisters the singleton.
        /// </summary>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void UnregisterSingleton<T>(T singleton) where T : class, Ximmerse.I_Singleton
        {
            System.Type singletonType = typeof(T);
            if (singltonRegistery.ContainsKey(singletonType))
            {
                singltonRegistery.Remove(singletonType);
            }

            var iSingletonType = typeof(I_Singleton);
            //反向注册类型的 I_Singleton 接口实现类.
            var interfacesTypes = singletonType.GetInterfaces();
            for (int i = 0; i < interfacesTypes.Length; i++)
            {
                Type interfaceType = interfacesTypes[i];
                if (iSingletonType.IsAssignableFrom(interfaceType))
                {
                    singltonRegistery.Remove(interfaceType);
                }
            }
        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        /// <returns>The singleton.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetSingleton<T>() where T : class, I_Singleton
        {
            System.Type singletonType = typeof(T);
            if (singltonRegistery.ContainsKey(singletonType))
            {
                if (singltonRegistery[singletonType] == null)
                {
                    Debug.LogErrorFormat(kWarning01, typeof(T).Name);
                }
                else if (!(singltonRegistery[singletonType] is T))
                {
                    Debug.LogErrorFormat(kWarning02, typeof(T).Name, singltonRegistery[singletonType].GetType().Name);
                }
                return (T)singltonRegistery[singletonType];
            }
            Debug.LogWarningFormat(kWarning03, typeof(T).Name);
            return default(T);
        }

        /// <summary>
        /// Determines if has singleton.
        /// </summary>
        /// <returns><c>true</c> if has singleton; otherwise, <c>false</c>.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool HasSingleton<T>() where T : class, Ximmerse.I_Singleton
        {
            return singltonRegistery.ContainsKey(typeof(T)) && singltonRegistery[typeof(T)] != default(T);
        }
    }
}