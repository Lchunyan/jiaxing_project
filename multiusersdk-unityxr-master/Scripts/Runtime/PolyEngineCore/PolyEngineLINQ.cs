using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Ximmerse.XR
{
    public static class PolyEngineLinq
    {
        /// <summary>
        /// Adds item if the item not exists in the list.
        /// </summary>
        /// <returns>The if not exists.</returns>
        /// <param name="genericList">Generic list.</param>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool AddUnduplicate<T>(this IList<T> genericList, T item)
        {
            if (!genericList.Contains(item))
            {
                genericList.Add(item);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Adds item if the item not exists in the hashset.
        /// </summary>
        /// <returns>The if not exists.</returns>
        /// <param name="genericList">Generic list.</param>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool AddUnduplicate<T>(this HashSet<T> genericList, T item)
        {
            if (!genericList.Contains(item))
            {
                genericList.Add(item);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Add array to hash set.
        /// Return failed (already exists) count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int AddRange<T>(this HashSet<T> hashSet, T[] items)
        {
            int failedCount = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (!hashSet.Add(items[i]))
                {
                    failedCount++;
                }
            }
            return failedCount;
        }

        /// <summary>
        /// Add list to hash set.
        /// Return failed (already exists) count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int AddRange<T>(this HashSet<T> hashSet, IList<T> items)
        {
            int failedCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (!hashSet.Add(items[i]))
                {
                    failedCount++;
                }
            }
            return failedCount;
        }

        /// <summary>
        /// Add list to hash set.
        /// Return failed (already exists) count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <param name="readonlyList"></param>
        /// <returns></returns>
        public static int AddRange<T>(this HashSet<T> hashSet, IReadOnlyList<T> readonlyList)
        {
            int failedCount = 0;
            for (int i = 0; i < readonlyList.Count; i++)
            {
                if (!hashSet.Add(readonlyList[i]))
                {
                    failedCount++;
                }
            }
            return failedCount;
        }

        /// <summary>
        /// Add other hashset to hash set.
        /// Return failed (already exists) count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <param name="otherHashset"></param>
        /// <returns></returns>
        public static int AddRange<T>(this HashSet<T> hashSet, HashSet<T> otherHashset)
        {
            int failedCount = 0;
            foreach(var other in otherHashset)
            {
                if (!hashSet.Add(other))
                {
                    failedCount++;
                }
            }
            return failedCount;
        }

        /// <summary>
        /// Adds items for those not exists in the list.
        /// </summary>
        /// <returns>How many items added.</returns>
        /// <param name="genericList">Generic list.</param>
        /// <param name="items">Items.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int AddRangeUnduplicate<T>(this List<T> genericList, IEnumerable<T> items)
        {
            int added = 0;
            foreach (var item in items)
            {
                if (!genericList.Contains(item))
                {
                    genericList.Add(item);
                    added++;
                }
            }

            return added;
        }


        /// <summary>
        /// Loops on each element on a postive direction (starts from zero);
        /// Warning: ForEach 中如果传入匿名委托会导致GC !
        /// </summary>
        /// <param name="genericArray">Generic array.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForEach<T>(this T[] genericArray, System.Action<T> action)
        {
            for (int i = 0; i < genericArray.Length; i++)
            {
                action(genericArray[i]);
            }
        }

        /// <summary>
        /// Loops on each element on a postive direction (starts from zero);
        /// </summary>
        /// <param name="list">Generic list.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void For<T>(this List<T> list, System.Action<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i], i);
            }
        }

        /// <summary>
        /// Loops on each element on a negative direction (starts from last);
        /// </summary>
        /// <param name="list">Generic list.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForReverse<T>(this List<T> list, System.Action<T, int> action)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                action(list[i], i);
            }
        }

        /// <summary>
        /// Loops on each element on a postive direction (starts from zero);
        /// </summary>
        /// <param name="array">Generic list.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void For<T>(this T[] array, System.Action<T, int> action)
        {
            for (int i = 0; i < array.Length; i++)
            {
                action(array[i], i);
            }
        }

        /// <summary>
        /// Loops on each element on a negative direction (starts from last);
        /// </summary>
        /// <param name="array">Generic list.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForReverse<T>(this T[] array, System.Action<T, int> action)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                action(array[i], i);
            }
        }

        /// <summary>
        /// Loops on each element on a reversed direction (starts from max-index);
        /// </summary>
        /// <returns>How many items added.</returns>
        /// <param name="genericArray">Generic list.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void ForEachReversed<T>(this T[] genericArray, System.Action<T> action)
        {
            for (int i = genericArray.Length - 1; i >= 0; i--)
            {
                action(genericArray[i]);
            }
        }

        /// <summary>
        /// Counts how many item exists in genericArray
        /// </summary>
        /// <param name="genericArray">Generic array.</param>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int Count<T>(this T[] genericArray, T item) where T : class
        {
            int count = 0;
            for (int i = 0; i < genericArray.Length; i++)
            {
                if (genericArray[i] == item)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Contains the specified genericArray and item.
        /// </summary>
        /// <param name="genericArray">Generic array.</param>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool Contains<T>(this T[] genericArray, T item)
        {
            for (var i = 0; i < genericArray.Length; i++)
            {
                var e = genericArray[i];
                if (e.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the index of item.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="item">Item.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static int FindIndex<T>(this T[] genericArray, T item)
        {
            for (var i = 0; i < genericArray.Length; i++)
            {
                var e = genericArray[i];
                if (e.Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 对每一个 transform的子对象调用一次action， 如果 deepHierarchy = true，还会包含深层级的子对象。
        /// 如果 includeSelf = true，会对 transform 自身调用。
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="function"></param>
        /// <param name="deepHierarchy"></param>
        /// <param name="includeSelf"></param>
        public static void ForEachChildren(this Transform transform, System.Action<Transform> function, bool deepHierarchy, bool includeSelf)
        {
            if (includeSelf)
            {
                function(transform);
            }
            if (deepHierarchy == false)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    function(child);
                }
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    function(child);
                    if (child.childCount > 0)
                    {
                        ForEachChildren(child, function, true, false);
                    }
                }
            }
        }

        /// <summary>
        /// Actions on each children which has target component. 
        /// </summary>
        public static void ForEachChildren<T>(this GameObject gameObject, System.Action<T> function, bool includeInactiveChildren) where T : Component
        {
            var comps = gameObject.GetComponentsInChildren<T>(includeInactiveChildren);
            for (int i = 0, compsLength = comps.Length; i < compsLength; i++)
            {
                var children = comps[i];
                function(children);
            }
        }

        /// <summary>
        /// Call a for(i=0; i LE count; i++)
        /// {
        ///  action(child)
        /// }
        ///
        /// on the transform parent
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action">child transform and the index</param>
        public static void ForChild(this Transform transform, System.Action<Transform, int> action)
        {
            for (int i = 0, iMax = transform.childCount; i < iMax; i++)
            {
                var childT = transform.GetChild(i);
                action(childT, i);
            }
        }

        /// <summary>
        /// Call a for(i=count-1; i GE 0; i--)
        /// {
        ///  action(child)
        /// }
        ///
        /// on the transform parent
        /// 
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="action">child transform and the index</param>
        public static void ForReversedChild(this Transform transform, System.Action<Transform, int> action)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var childT = transform.GetChild(i);
                action(childT, i);
            }
        }

        /// <summary>
        /// Actions on each children. In deepHierarchy = false, function run on direct children, else if true, actions is guaranteed to run on each sub children.
        /// function's parameter is the children gameobject at the loop.
        /// </summary>
        public static void ForEachChildren(this GameObject gameObject, System.Action<Transform> function, bool deepHierarchy, bool includeSelf)
        {
            gameObject.transform.ForEachChildren(function, deepHierarchy, includeSelf);
        }

        /// <summary>
        /// Actions on each children. In deepHierarchy = false, function run on direct children, else if true, actions is guaranteed to run on each sub children.
        /// function's parameter is the children gameobject at the loop.
        /// </summary>
        public static void ForEachChildren(this Scene scene, System.Action<Transform> function, bool deepHierarchy)
        {
            GameObject[] gameObjects = scene.GetRootGameObjects();
            if (deepHierarchy == false)
            {
                for (int i = 0, gameObjectsLength = gameObjects.Length; i < gameObjectsLength; i++)
                {
                    GameObject firstChild = gameObjects[i];
                    function(firstChild.transform);
                }
            }
            else
            {
                for (int i = 0, gameObjectsLength = gameObjects.Length; i < gameObjectsLength; i++)
                {
                    Transform child = gameObjects[i].transform;
                    function(child);
                    if (child.childCount > 0)
                    {
                        ForEachChildren(child, function, true, false);
                    }
                }
            }
        }

        /// <summary>
        /// For each action on transform node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="function"></param>
        public static void ForEachParent(this Transform node, System.Action<Transform> function)
        {
            var p = node.parent;
            while (p != null)
            {
                function(p);
                p = p.parent;
            }
        }
    }
}