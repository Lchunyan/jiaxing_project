using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System;
using Object = UnityEngine.Object;



namespace Ximmerse
{

    /// <summary>
    /// Generic collection utils .
    /// </summary>
    public static class GenericCollectionUtils
    {
        /// <summary>
        /// Adds an element to IDictionary[T1, List[T2]] .
        /// Will create a list object if the key doesn't exist.
        /// Return element count.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int AddElement<T1, T2>(this IDictionary<T1, List<T2>> dict, T1 key, T2 value) where T2 : class
        {
            bool contains = dict.TryGetValue(key, out List<T2> ValueList);
            if (contains)
            {
                if (ValueList == null)
                {
                    ValueList = new List<T2>();
                    dict[key] = ValueList;
                }
                ValueList.Add(value);
                return ValueList.Count;
            }
            else
            {
                ValueList = new List<T2>();
                ValueList.Add(value);
                dict.Add(key, ValueList);
                return 1;
            }
        }

        /// <summary>
        /// 查询列表型字典元素，返回最后一个元素。
        /// 如果 remove 为true， 则在返回之前从列表中删除。
        /// 此方法可高效的用于池字典。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="remove">如果remove为true，则在返回元素之前删除列表。</param>
        /// <returns></returns>
        public static T2 GetLastElement<T1, T2>(this IDictionary<T1, List<T2>> dict, T1 key, bool remove) where T2 : class
        {
            if (dict.TryGetValue(key, out List<T2> list) && list != null && list.Count > 0)
            {
                var element = list[list.Count - 1];
                if (remove)
                {
                    list.RemoveAt(list.Count - 1);
                }
                return element;
            }
            else
            {
                return default(T2);
            }
        }

        /// <summary>
        /// Get next element of the index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="index"></param>
        /// <param name="clamp">If clamp is true, return the last element if reaching max.</param>
        /// <returns></returns>
        public static T GetNextElement<T>(this T[] elements, int index, bool clamp = false)
        {
            if (clamp)
            {
                return (index >= elements.Length - 1) ? elements[elements.Length - 1] : elements[index + 1];
            }
            else
            {
                return (index >= elements.Length - 1) ? elements[0] : elements[index + 1];
            }
        }

        /// <summary>
        /// Get previous element of the index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="index"></param>
        /// <param name="clamp"></param>
        /// <returns></returns>
        public static T GetPreviousElement<T>(this T[] elements, int index, bool clamp = false)
        {
            if (clamp)
            {
                return (index <= 0) ? elements[0] : elements[index - 1];
            }
            else
            {
                return (index <= 0) ? elements[elements.Length - 1] : elements[index - 1];
            }
        }


        /// <summary>
        /// Remove element value from list retrieved by dictionary[key].
        /// Return element count.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="RemoveEmptyList">If true, remove the key from dictionary when the retrieved list is empty. </param>
        public static int RemoveElement<T1, T2>(this IDictionary<T1, List<T2>> dict, T1 key, T2 value, bool RemoveEmptyList = false) where T2 : class
        {
            bool contains = dict.TryGetValue(key, out List<T2> ValueList);
            if (contains && ValueList != null && ValueList.Count > 0)
            {
                ValueList.Remove(value);
                if (RemoveEmptyList && ValueList.Count == 0)
                {
                    dict.Remove(key);
                }
                return ValueList.Count;
            }
            return 0;
        }

        /// <summary>
        /// Add default elements to list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="addCount">The element count to be added.</param>
        /// <returns></returns>
        public static void AddElements<T>(this IList<T> list, int addCount, T value = default(T))
        {
            for (int i = 0; i < addCount; i++)
            {
                list.Add(value);
            }
        }

        /// <summary>
        /// Adds array start from offset , in size.
        /// if offset + size >= Array length, tries to add as many element as possible.
        /// Return added element count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="Array"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public static int AddRange<T>(this IList<T> list, T[] Array, int offset, int size)
        {
            if (offset >= Array.Length)
            {
                return 0;
            }
            int maxIndex = Mathf.Min(Array.Length - 1, offset + size - 1);
            for (int i = offset; i <= maxIndex; i++)
            {
                list.Add(Array[i]);
            }
            return maxIndex - offset + 1;
        }

        /// <summary>
        /// 群体添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="Other"></param>
        /// <returns>添加个数</returns>
        public static int AddMany<T>(this IList<T> list, IList<T> Other)
        {
            for (int i = 0, imax = Other.Count; i < imax; i++)
            {
                T e = Other[i];
                list.Add(e);
            }
            return Other.Count;
        }

        /// <summary>
        /// 群体添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="Other"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int AddMany<T>(this IList<T> list, IList<T> Other, int offset, int size)
        {
            if (offset >= Other.Count)
            {
                return 0;
            }
            int maxIndex = Mathf.Min(Other.Count - 1, offset + size - 1);
            for (int i = offset; i <= maxIndex; i++)
            {
                list.Add(Other[i]);
            }
            return maxIndex - offset + 1;
        }

        /// <summary>
        /// Add the elements to the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void Add<T>(this IList<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                T e = elements[i];
                list.Add(e);
            }
        }
        /// <summary>
        /// Add the elements to the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="elements"></param>
        public static void AddUnduplicate<T>(this IList<T> list, params T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                T e = elements[i];
                list.AddUnduplicate(e);
            }
        }

        /// <summary>
        /// 给字典内容赋值.如果key不存在则添加key，否则直接赋值。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrSetDictionary<T1, T2>(this IDictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
        /// <summary>
        /// Adds a key and checks existence before adding. A safe function never hits KeyAlreadyExists exception.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddSafey<T1, T2>(this IDictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// 从 list 中按照正向排序， 返回前N个对象组成的数组。
        /// 如果 Start + Count 超出了list的长度， 则最多返回  List.Length - Start - 1 个对象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static T[] GetArray<T>(this IList<T> list, int count, int start = 0)
        {
            int n = Mathf.Min(list.Count - start, count);
            T[] array = new T[n];
            for (int i = start; i < start + n; i++)
            {
                array[i] = list[i];
            }
            return array;
        }

        /// <summary>
        /// 和 GetArray 一样但是顺序是反过来， 从后往前获取。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static T[] GetArrayFlip<T>(this IList<T> list, int count)
        {
            int n = Mathf.Min(list.Count, count);
            T[] array = new T[n];
            for (int i = list.Count - 1; i >= 0; i--)
            {
                array[i] = list[i];
            }
            return array;
        }


        /// <summary>
        /// Given a dictonary, gets or add an value at the key.
        /// The value is instantiated with default new() constructor.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T2 GetOrAddDictionary<T1, T2>(this IDictionary<T1, T2> dict, T1 key) where T2 : class, new()
        {
            bool contains = dict.TryGetValue(key, out T2 value);
            if (contains)
            {
                return value;
            }
            else
            {
                T2 newVal = new T2();
                dict.Add(key, newVal);
                return newVal;
            }
        }

        /// <summary>
        /// Given a dictonary, gets by key if key exists, or return default value.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T2 GetOrDefault<T1, T2>(this IDictionary<T1, T2> dict, T1 key)
        {
            bool contains = dict.TryGetValue(key, out T2 value);
            if (contains)
            {
                return value;
            }
            else
            {
                return default(T2);
            }
        }

        /// <summary>
        /// Given a dictonary, gets or add a element at the key, if the ket did not exists, adds the default value.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T2 GetOrAddDictionary<T1, T2>(this IDictionary<T1, T2> dict, T1 key, T2 defaultVal)
        {
            bool contains = dict.TryGetValue(key, out T2 value);
            if (contains)
            {
                return value;
            }
            else
            {
                dict.Add(key, defaultVal);
                return defaultVal;
            }
        }


        /// <summary>
        /// 对于列表元素类型, 获取或者创建一个 list 对象。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<T2> GetOrAddDictionaryListElement<T1, T2>(this IDictionary<T1, List<T2>> dict, T1 key)
        {
            bool contains = dict.TryGetValue(key, out List<T2> value);
            if (contains)
            {
                if (value != null)
                    return value;
                else
                {
                    value = new List<T2>();
                    dict[key] = value;
                    return value;
                }
            }
            else
            {
                List<T2> list = new List<T2>();
                dict.Add(key, list);
                return list;
            }
        }


        /// <summary>
        /// Clears null element from list.
        /// Returns the element count which is removed.
        /// Call this method for UnityEngine object list.
        /// </summary>
        /// <param name="list"></param>
        public static int ClearNull<T>(this List<T> list) where T : Object
        {
            if (list != null && list.Count > 0)
            {
                int removed = 0;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i])
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }
                return removed;
            }
            return 0;
        }

        /// <summary>
        /// Clears null element from list.
        /// Returns the element count which is removed.
        /// For unity object list, call [ClearNull], call this method for non-unity object list.
        /// </summary>
        /// <param name="list"></param>
        public static int ClearNullElement<T>(this List<T> list) where T : class
        {
            if (list != null && list.Count > 0)
            {
                int removed = 0;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (object.ReferenceEquals(list[i], null))
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }
                return removed;
            }
            return 0;
        }

        /// <summary>
        /// Clear all list values inside the [T1, List[T2]] dictionary.
        /// This method do not clear the dictionary entry. So the key-value persist.
        /// 此方法不会销毁字典的键值索引记录。
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        public static void ClearDictionaryListValues<T1, T2>(this IDictionary<T1, List<T2>> dict)
        {
            foreach (var key in dict.Keys)
            {
                if (dict[key] != null && dict[key].Count > 0)
                {
                    dict[key].Clear();
                }
            }
        }

        /// <summary>
        /// Clear and all list values inside the [T1, List[T2]] dictionary, and destroy all unity objects inside those lists.
        /// This method do not clear the dictionary entry. So the key-value persist.
        /// 此方法不会销毁字典的键值索引记录
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        public static void ClearAndDestroyDictionaryListValues<T1, T2>(this IDictionary<T1, List<T2>> dict, float delayed = 0, bool allowDestroyAsset = true) where T2 : UnityEngine.Object
        {
            foreach (var key in dict.Keys)
            {
                if (dict[key] != null && dict[key].Count > 0)
                {
                    dict[key].DestroyAll(delayed, allowDestroyAsset);
                }
            }
        }

        /// <summary>
        /// Collect material from renderers.
        /// </summary>
        /// <param name="renderers"></param>
        /// <param name="materials"></param>
        public static void CollectionMaterials(this IEnumerable<Renderer> renderers, List<Material> materials)
        {
            foreach (var renderer in renderers)
            {
                var sharedMats = renderer.sharedMaterials;
                for (int i = 0, sharedMatsLength = sharedMats.Length; i < sharedMatsLength; i++)
                {
                    var sharedMat = sharedMats[i];
                    if (!materials.Contains(sharedMat))
                    {
                        materials.Add(sharedMat);
                    }
                }
            }
        }

        /// <summary>
        /// Is the list null or empty ?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count <= 0;
        }

        /// <summary>
        /// Is the list null or empty ?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this T[] list)
        {
            return list == null || list.Length == 0;
        }


        /// <summary>
        /// Destroy and clear the unity object list.
        /// 删除并且清空list中的所有的unity object。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DestroyAll<T>(this IList<T> list, float delayed = 0, bool allowDestroyAsset = true) where T : Object
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Destroy(delayed, allowDestroyAsset);
            }
            list.Clear();
        }

        /// <summary>
        /// Destroy and clear the unity object list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DestroyAll<T>(this T[] array, float delayed = 0, bool allowDestroyAsset = true) where T : Object
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Destroy(delayed, allowDestroyAsset);
            }
        }

        /// <summary>
        /// Destroy and clear the unity game object list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DestroyAllGameObject<T>(this IList<T> list, float delayed = 0, bool allowDestroyAsset = true) where T : Component
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].gameObject.Destroy(delayed, allowDestroyAsset);
            }
            list.Clear();
        }

        /// <summary>
        /// Destroy and clear the unity game object list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DestroyAllGameObject<T>(this T[] array, float delayed = 0, bool allowDestroyAsset = true) where T : Component
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i].gameObject.Destroy(delayed, allowDestroyAsset);
            }
        }

        /// <summary>
        /// Destroy the list[Index] and remove from the list.
        /// 删除并销毁 list 中 element index 所指向的 Unity object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void DestroyAndRemoveAt<T>(this IList<T> list, int index, float delayed = 0, bool allowDestroyAsset = true) where T : Object
        {
            if (index >= 0 && list.Count > index)
            {
                if (list[index] != null)
                {
                    list[index].Destroy(delayed, allowDestroyAsset);
                }
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Destroy the list[Index] and remove from the list, this method also destroy the game object of the component.
        /// 删除并销毁 list 中 element index 所指向的 Unity component  和 它的 game object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void DestroyGameObjectAndRemoveAt<T>(this IList<T> list, int index, float delayed = 0, bool allowDestroyAsset = true) where T : Component
        {
            if (index >= 0 && list.Count > index)
            {
                if (list[index] != null)
                {
                    list[index].gameObject.Destroy(delayed, allowDestroyAsset);
                }
                list.RemoveAt(index);
            }
        }


        /// <summary>
        /// Destroy and clear the components game object.
        /// 删除并且清空list中的所有的unity object。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void DestroyAllGameObjects<T>(this IList<T> list, float delayed = 0, bool allowDestroyAsset = true) where T : Component
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    list[i].gameObject.Destroy(delayed, allowDestroyAsset);
                }
            }
            list.Clear();
        }

        /// <summary>
        /// Delete the first element from the list.
        /// If you want to delete and destroy the unity object list element, call DestroyFirst() instead.
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DeleteFirst<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                list.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete the last element from the list.
        /// If you want to delete and destroy the unity object list element, call DestroyLast() instead.
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DeleteLast<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Delete and destroy the first element from the unity object list.
        /// If you want to destroy the game object too, call DestroyFirstWithGameObject()
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DestroyFirst<T>(this IList<T> list, float delayed = 0, bool destroyAsset = true) where T : UnityEngine.Object
        {
            if (list.Count > 0)
            {
                if (list[0] != null)
                    list[0].Destroy(delayed, destroyAsset);
                list.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete and destroy the last element from the unity object list.
        /// If you want to destroy the game object too, call DestroyGameObjectLast()
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DestroyLast<T>(this IList<T> list, float delayed = 0, bool destroyAsset = true) where T : UnityEngine.Object
        {
            if (list.Count > 0)
            {
                if (list[list.Count - 1] != null)
                    list[list.Count - 1].Destroy(delayed, destroyAsset);
                list.RemoveAt(list.Count - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete and destroy the first element and its game object from the unity object list.
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DestroyFirstWithGameObject<T>(this IList<T> list, float delayed = 0, bool destroyAsset = true) where T : UnityEngine.Component
        {
            if (list.Count > 0)
            {
                if (list[0] != null)
                {
                    list[0].gameObject.Destroy(delayed, destroyAsset);
                }
                list.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete and destroy the last element and its game object from the unity object list.
        /// Return true if deleted, else return false - if the list are null, or empty.
        /// </summary>
        /// <param name="list"></param>
        public static bool DestroyLastWithGameObject<T>(this IList<T> list, float delayed = 0, bool destroyAsset = true) where T : UnityEngine.Component
        {
            if (list.Count > 0)
            {
                if (list[list.Count - 1] != null)
                    list[list.Count - 1].gameObject.Destroy(delayed, destroyAsset);
                list.RemoveAt(list.Count - 1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// sets the monobehaviours enable / disable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="enabled"></param>
        public static void SetEnable<T>(this T[] array, bool enabled) where T : MonoBehaviour
        {
            for (int i = 0; i < array.Length; i++)
            {
                T monob = array[i];
                monob.enabled = enabled;
            }
        }

        /// <summary>
        /// sets the monobehaviours enable / disable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="enabled"></param>
        public static void SetEnable<T>(this IList<T> list, bool enabled) where T : MonoBehaviour
        {
            for (int i = 0; i < list.Count; i++)
            {
                T monob = list[i];
                monob.enabled = enabled;
            }
        }


        /// <summary>
        /// Loops on each element on a postive direction (starts from zero);
        /// With a return value.
        /// </summary>
        /// <param name="array">Generic list.</param>
        /// <param name="func">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        public static IEnumerable<TResult> For<T, TResult>(this T[] array, System.Func<T, int, TResult> func)
        {
            List<TResult> ret = new List<TResult>();
            for (int i = 0; i < array.Length; i++)
            {
                ret.Add(func(array[i], i));
            }
            return ret;
        }

        /// <summary>
        /// Loops on each element on a negative direction (starts from last);
        /// With a return value.
        /// </summary>
        /// <param name="array">Generic list.</param>
        /// <param name="func">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        public static IEnumerable<TResult> ForReverse2<T, TResult>(this T[] array, System.Func<T, int, TResult> func)
        {
            List<TResult> ret = new List<TResult>();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                ret.Add(func(array[i], i));
            }
            return ret;
        }


        /// <summary>
        /// Find index of the first element that returns true at func.
        /// Return -1 if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static int FindIndexOf<T>(this IList<T> list, Func<T, bool> func)
        {
            for (int i = 0, imax = list.Count; i < imax; i++)
            {
                bool ret = func(list[i]);
                if (ret)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find all indices of the elements that returns true at func.
        /// Return 0 if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <param name="foundIndices">输出找到的索引</param>
        /// <returns>Found matched elements count.</returns>
        public static int FindIndicesOfAll<T>(this IList<T> list, Func<T, bool> func, IList<int> foundIndices)
        {
            int found = 0;
            for (int i = 0, imax = list.Count; i < imax; i++)
            {
                bool ret = func(list[i]);
                if (ret)
                {
                    found++;
                }
            }
            return found;
        }

        /// <summary>
        /// Loops on each element on a postive direction (starts from zero);
        /// With a return value.
        /// </summary>
        /// <param name="list">Generic list.</param>
        /// <param name="func">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="TResult"></typeparam>
        public static IEnumerable<TResult> For2<T, TResult>(this IList<T> list, System.Func<T, int, TResult> func)
        {
            List<TResult> ret = new List<TResult>();
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(func(list[i], i));
            }
            return ret;
        }

        /// <summary>
        /// Loops on each element on a negative direction (starts from last);
        /// </summary>
        /// <param name="list">Generic list.</param>
        /// <param name="func">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static IEnumerable<TResult> ForReverse<T, TResult>(this IList<T> list, System.Func<T, int, TResult> func)
        {
            List<TResult> ret = new List<TResult>();
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ret.Add(func(list[i], i));
            }
            return ret;
        }


        /// <summary>
        /// Return the last element from the list, return default(T) if list is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static T Last<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                var t = list[list.Count - 1];
                return t;
            }
            else return default(T);
        }

        /// <summary>
        /// Remove and return the first element from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static T RemoveFirst<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                var t = list[0];
                list.RemoveAt(0);
                return t;
            }
            else return default(T);
        }

        /// <summary>
        /// Remove and return the last element from the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static T RemoveLast<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                var t = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return t;
            }
            else return default(T);
        }

        /// <summary>
        /// 从尾部开始删除[Count]个元素。
        /// 如果[Count]大于list的元素个数，则会清空 list.
        /// 返回删除的元素个数.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="Count"></param>
        public static int RemoveFromTail<T>(this IList<T> list, int Count)
        {
            if (Count >= list.Count)
            {
                int c = list.Count;
                list.Clear();
                return c;
            }
            else
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    list.RemoveAt(i);
                }
                return Count;
            }
        }

        /// <summary>
        /// 从 list 中删除 otherList 中的元素.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="otherList"></param>
        /// <returns></returns>
        public static int RemoveFromList<T>(this IList<T> list, IList<T> otherList)
        {
            int ret = 0;
            for (int i = 0; i < otherList.Count; i++)
            {
                T element = otherList[i];
                if (list.Remove(element))
                {
                    ret++;
                }
            }
            return ret;
        }

        /// <summary>
        /// 从 list 中删除 others 中的元素.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static int RemoveFromList<T>(this IList<T> list, T[] others)
        {
            int ret = 0;
            for (int i = 0; i < others.Length; i++)
            {
                T element = others[i];
                if (list.Remove(element))
                {
                    ret++;
                }
            }
            return ret;
        }

        /// <summary>
        /// 从头部开始删除[Count]个元素。
        /// 如果[Count]大于list的元素个数，则会清空 list.
        /// 返回删除的元素个数.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="Count"></param>
        public static int RemoveFromHead<T>(this IList<T> list, int Count)
        {
            if (Count >= list.Count)
            {
                int c = list.Count;
                list.Clear();
                return c;
            }
            else
            {
                int rCount = 0;
                while (rCount <= Count)
                {
                    list.RemoveAt(0);
                    rCount++;
                }
                return Count;
            }
        }


        /// <summary>
        /// Shuffle a list.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> ShuffleList<T>(this IList<T> list, int? randomSeed = null)
        {
            var random = ReferenceEquals(randomSeed, null) ? new System.Random() : new System.Random(randomSeed.Value);
            for (var i = 0; i < list.Count - 1; i++)
            {
                var newIndex = random.Next(i, list.Count - 1);
                var tempValue = list[i];
                list[i] = list[newIndex];
                list[newIndex] = tempValue;
            }
            return list;
        }

        /// <summary>
        /// 此方法自动处理数组越界问题， 将index限定在 array 数组范围内。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T SafeGet<T>(this T[] array, int index)
        {
            if (array == null || array.Length == 0)
            {
                return default(T);
            }
            else if (index < 0)//下界
            {
                return array[0];
            }
            else if (index >= array.Length)//上界
            {
                return array[array.Length - 1];
            }
            else
            {
                return array[index];
            }
        }

        /// <summary>
        /// 此方法自动处理数组越界问题， 将index限定在 array 数组范围内。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T SafeGet<T>(this IList<T> list, int index)
        {
            if (list == null || list.Count == 0)
            {
                return default(T);
            }
            else if (index < 0)//下界
            {
                return list[0];
            }
            else if (index >= list.Count)//上界
            {
                return list[list.Count - 1];
            }
            else
            {
                return list[index];
            }
        }

        /// <summary>
        /// 在 List 上设置第 Index 位置的元素 = element.
        /// 如果List的capacity 不够，会自动扩容。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <param name="Index"></param>
        /// <returns>是否对list执行了扩容操作。</returns>
        public static bool SetElementAtIndex<T>(this IList<T> list, T element, int Index)
        {
            //不需要给List扩容:
            if (list.Count >= (Index + 1))
            {
                list[Index] = element;
                return false;
            }
            else
            {
                //list 容量不够了， 需要扩容:
                int capcityIncreased = Index + 1 - list.Count;
                for (int i = 0; i < capcityIncreased; i++)
                {
                    list.Add(default(T));//自动扩容
                }
                list[Index] = element;
                return true;
            }
        }

        /// <summary>
        /// Sets a single demension array's every element to the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void SetSingleDemensionValue<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Sets value of a 2D array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr2D"></param>
        /// <param name="value"></param>
        public static void Set2DArrayValue<T>(this T[,] arr2D, T value)
        {
            for (int x = 0, xMax = arr2D.GetLength(0); x < xMax; x++)
            {
                for (int y = 0, yMax = arr2D.GetLength(1); y < yMax; y++)
                {
                    arr2D[x, y] = value;
                }
            }
        }

        /// <summary>
        /// Sets value of a 3D array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr3D"></param>
        /// <param name="value"></param>
        public static void Set3DArrayValue<T>(this T[,,] arr3D, T value)
        {
            for (int x = 0, xMax = arr3D.GetLength(0); x < xMax; x++)
            {
                for (int y = 0, yMax = arr3D.GetLength(1); y < yMax; y++)
                {
                    for (int z = 0, zMax = arr3D.GetLength(2); z < zMax; z++)
                    {
                        arr3D[x, y, z] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Sets value of a 4D array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr4D"></param>
        /// <param name="value"></param>
        public static void Set4DArrayValue<T>(this T[,,,] arr4D, T value)
        {
            for (int x = 0, xMax = arr4D.GetLength(0); x < xMax; x++)
            {
                for (int y = 0, yMax = arr4D.GetLength(1); y < yMax; y++)
                {
                    for (int z = 0, zMax = arr4D.GetLength(2); z < zMax; z++)
                    {
                        for (int w = 0, wMax = arr4D.GetLength(3); w < wMax; w++)
                        {
                            arr4D[x, y, z, w] = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets value of a 5D array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr5D"></param>
        /// <param name="value"></param>
        public static void Set5DArrayValue<T>(this T[,,,,] arr5D, T value)
        {
            for (int x = 0, xMax = arr5D.GetLength(0); x < xMax; x++)
            {
                for (int y = 0, yMax = arr5D.GetLength(1); y < yMax; y++)
                {
                    for (int z = 0, zMax = arr5D.GetLength(2); z < zMax; z++)
                    {
                        for (int w = 0, wMax = arr5D.GetLength(3); w < wMax; w++)
                        {
                            for (int a = 0, aMax = arr5D.GetLength(4); a < aMax; a++)
                            {
                                arr5D[x, y, z, w, a] = value;
                            }
                        }
                    }
                }
            }
        }
    }
}