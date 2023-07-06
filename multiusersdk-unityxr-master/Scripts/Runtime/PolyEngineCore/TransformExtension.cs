using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using System;
using Object = UnityEngine.Object;
using System.Text;

namespace Ximmerse
{
    /// <summary>
    /// Rect transform's 5 corner position.
    /// </summary>
    public enum RectTransformAnchor
    {
        Center,

        LeftBottom,

        LeftTop,

        RightTop,

        RightBottom,

        LeftMiddle,

        RightMiddle,

        TopMiddle,

        BottomMiddle,
    }

    /// <summary>
    /// Transform extension : extension method for UnityEngine.Transform
    /// </summary>
    public static class TransformExtension
    {
        public enum ResetMask
        {
            ResetAll = 0,

            ResetPosition,

            ResetRotation,

            ResetLocalScale,
        }

        static List<Transform> tmpChilds = new List<Transform>();

        static StringBuilder kBuffer = new StringBuilder();

        /// <summary>
        /// 计算 other 相对于 this 的localposition 和 local rotation
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="other"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        public static void GetLocalPositionAndRotation(this Transform thisTransform, Transform other, out Vector3 localPosition, out Quaternion localRotation)
        {
            localPosition = thisTransform.InverseTransformPoint(other.position);
            localRotation = thisTransform.InverseTransformQuaternion(other.rotation);
        }

        /// <summary>
        /// 计算 other 相对于 this 的localposition 和 local rotation
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="other"></param>
        /// <param name="localPosition"></param>
        /// <param name="localRotation"></param>
        public static void GetLocalPositionAndRotation(this Transform thisTransform, Transform other, out Pose localPose)
        {
            var localPosition = thisTransform.InverseTransformPoint(other.position);
            var localRotation = thisTransform.InverseTransformQuaternion(other.rotation);
            localPose = new Pose(localPosition, localRotation);
        }

        /// <summary>
        /// Returns all children game object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Transform[] GetAllChildrenGameObjects(this Transform gameObject, bool includeSubChildren)
        {
            List<Transform> children = new List<Transform>();
            GetAllChildrenTransforms(gameObject, children, includeSubChildren);
            return children.ToArray();
        }

        /// <summary>
        /// 获取 Child 到 Parent 的路径, 和层级数。
        /// </summary>
        /// <param name="child"></param>
        /// <param name="Parent"></param>
        /// <param name="Path"></param>
        /// <param name="Hierarchy">Child depth in the parent's transform hierarchy.</param>
        public static bool GetPathToParent(this Transform child, Transform Parent, out string Path, out int Hierarchy)
        {
            if (!child.IsChildOf(Parent))
            {
                Debug.LogErrorFormat("{0} is not child of : {1}", child.name, Parent.name);
                Path = string.Empty;
                Hierarchy = -1;
                return false;
            }
            if (ReferenceEquals(child, Parent))
            {
                //Debug.LogErrorFormat("{0} == {1}", child.name, Parent.name);
                Path = string.Empty;
                Hierarchy = 0;
                return true;
            }
            kBuffer.Clear();
            Hierarchy = 1;
            Transform p = child.parent;
            kBuffer.Append(child.name);
            while (p != Parent)
            {
                var pre = p;
                p = p.parent;
                int l = pre.name.Length;
                kBuffer.Insert(0, pre.name);
                kBuffer.Insert(l, '/');
                Hierarchy++;
            }
            Path = kBuffer.ToString();
            kBuffer.Clear();
            return true;
        }

        /// <summary>
        /// 获取 Child 到 Parent 的路径, 和层级数。
        /// </summary>
        /// <param name="child"></param>
        /// <param name="Parent"></param>
        /// <param name="Path"></param>
        /// <param name="Hierarchy">Child depth in the parent's transform hierarchy.</param>
        /// <param name="childIndexList">The child index of each hierarchy parent, ordered from root to child.</param>
        public static bool GetPathToParent(this Transform child, Transform Parent, out string Path, out int Hierarchy, List<int> childIndexList)
        {
            if (!child.IsChildOf(Parent))
            {
                Debug.LogErrorFormat("{0} is not child of : {1}", child.name, Parent.name);
                Path = string.Empty;
                Hierarchy = -1;
                return false;
            }
            if (ReferenceEquals(child, Parent))
            {
                //Debug.LogErrorFormat("{0} == {1}", child.name, Parent.name);
                Path = string.Empty;
                Hierarchy = 0;
                return true;
            }
            kBuffer.Clear();
            Hierarchy = 1;
            Transform p = child.parent;
            kBuffer.Append(child.name);
            childIndexList.Add(child.GetSiblingIndex());
            while (p != Parent)
            {
                var pre = p;
                p = p.parent;
                int l = pre.name.Length;
                kBuffer.Insert(0, pre.name);
                kBuffer.Insert(l, '/');
                Hierarchy++;
                childIndexList.Add(pre.GetSiblingIndex());
            }
            Path = kBuffer.ToString();
            kBuffer.Clear();
            //reverse : [child to root ] => [root to child]
            childIndexList.Reverse();
            return true;
        }

        /// <summary>
        /// Returns all children game object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static void GetAllChildrenTransforms(this Transform gameObject, List<Transform> children, bool includeSubChildren)
        {
            for (int i = 0; i < gameObject.childCount; i++)
            {
                Transform child = gameObject.GetChild(i);
                children.Add(child);
                if (includeSubChildren && child.childCount > 0)
                {
                    GetAllChildrenTransforms(child, children, includeSubChildren);
                }
            }
        }

        /// <summary>
        /// Gets the or create child transform of name.
        /// </summary>
        public static Transform GetOrCreateChild(this Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                return child;

            child = new GameObject(childName).transform;
            child.SetParent(parent, false);
            return child;
        }

        /// <summary>
        /// Inverses the quaternion to transform's local rotation
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="worldRotation">World rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion InverseTransformQuaternion(this Transform transform, Quaternion worldRotation)
        {
            return Quaternion.Inverse(transform.rotation) * worldRotation;
        }

        /// <summary>
        /// Transform local rotation to world space rotation.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
        {
            return transform.rotation * localRotation;
        }

        /// <summary>
        /// Deletes all children from self.
        /// </summary>
        /// <param name="transform">Transform.</param>
        public static void DeleteAllChildren(this Transform transform, float delay = 0)
        {
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                var childGO = transform.GetChild(i).gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    if (Mathf.Approximately(delay, 0))
                        Object.Destroy(childGO);
                    else
                        Object.Destroy(childGO, delay);
                }
            }
        }

        /// <summary>
        /// Deletes all children from self.
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="delay">Delay time</param>
        /// <param name="except">Delete all except this one.</param>
        public static void DeleteAllChildren(this Transform transform, float delay, Transform except)
        {
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (except == child) //except this one.
                {
                    continue;
                }
                var childGO = child.gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    if (Mathf.Approximately(delay, 0))
                        Object.Destroy(childGO);
                    else
                        Object.Destroy(childGO, delay);
                }
            }
        }

        /// <summary>
        /// Deletes all children from self.
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="delay">Delay time</param>
        /// <param name="except">Delete all except this one.</param>
        public static void DeleteAllChildren(this Transform transform, float delay, params Transform[] except)
        {
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                bool skip = false;
                var child = transform.GetChild(i);
                for (int j = 0; j < except.Length; j++)
                {
                    if (except[j] == child)
                    {
                        skip = true;
                    }
                }
                if (skip)
                {
                    continue;
                }
                var childGO = child.gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    if (Mathf.Approximately(delay, 0))
                        Object.Destroy(childGO);
                    else
                        Object.Destroy(childGO, delay);
                }
            }
        }

        /// <summary>
        /// Deletes all children from self.
        /// </summary>
        public static void DeleteAllChildren(this GameObject gameObject, float delay = 0)
        {
            Transform transform = gameObject.transform;
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                var childGO = transform.GetChild(i).gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    if (Mathf.Approximately(delay, 0))
                        Object.Destroy(childGO);
                    else
                        Object.Destroy(childGO, delay);
                }
            }
        }

        /// <summary>
        /// Deletes (immediately) all children from self.
        /// </summary>
        public static void DeleteAllChildrenImmediately(this GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                var childGO = transform.GetChild(i).gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    Object.DestroyImmediate(childGO);
                }
            }
        }

        /// <summary>
        /// Deletes (immediately) all children from self.
        /// </summary>
        public static void DeleteAllChildrenImmediately(this Transform transform)
        {
            for (int i = (transform.childCount - 1); i >= 0; i--)
            {
                var childGO = transform.GetChild(i).gameObject;
                if (Application.isEditor && !Application.isPlaying)
                {
                    Object.DestroyImmediate(childGO, true);
                }
                else
                {
                    Object.DestroyImmediate(childGO);
                }
            }
        }

        /// <summary>
        /// 从 GameObject 下删除名字叫 [childrenName] 的子对象。
        /// 这个方法不会遍历深层级对象，只会遍历第一层级。
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="childrenName">名字</param>
        /// <param name="destroyAll">是否删除全部叫做这个名字的Children</param>
        /// <param name="stringComparison">名字比较模式</param>
        /// <param name="delay">延迟</param>
        /// <param name="allowDestroyAsset">是否允许删除资源对象</param>
        /// <returns>被删除的个数</returns>
        public static int DestroyChildrenByName(this Transform transform, string childrenName, float delay = 0, bool destroyAll = false, StringComparison stringComparison = StringComparison.Ordinal, bool allowDestroyAsset = true)
        {
            Transform t = transform;
            int deleteCount = 0;
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Transform childT = t.GetChild(i);
                if (childT.name.Equals(childrenName, stringComparison))
                {
                    childT.gameObject.Destroy(delay, allowDestroyAsset);
                    deleteCount++;
                    if (destroyAll)//只删除第一个找到的匹配对象
                    {
                        break;
                    }
                }
            }
            return deleteCount;
        }

        /// <summary>
        /// Delete a child gameobject of the index, if it exists.
        /// Return true if the game object exists and deleted.
        /// </summary>
        public static bool DeleteChild(this GameObject gameObject, int childIndex, float delay = 0)
        {
            if (gameObject.transform.childCount > childIndex)
            {
                Transform child = gameObject.transform.GetChild(childIndex);
                if (child != null)
                {
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        Object.DestroyImmediate(child.gameObject, true);
                    }
                    else
                    {
                        if (Mathf.Approximately(delay, 0))
                            Object.Destroy(child.gameObject);
                        else
                            Object.Destroy(child.gameObject, delay);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Looks at world position but only rotate in yaw, rotate axis = Vector3.up
        /// </summary>
        /// <param name="WorldPosition">World position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LookAtXZ(this Transform transform, Vector3 WorldPosition)
        {
            WorldPosition.y = transform.position.y;
            transform.LookAt(WorldPosition, Vector3.up);
        }

        /// <summary>
        /// Resets the transform local matrix but its children remain unchange.
        /// </summary>
        /// <param name="transform"></param>
        public static void ResetButChildrenStays(this Transform transform, ResetMask resetMask = ResetMask.ResetAll)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;
            }

            //reset local:
            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetPosition)
                transform.localPosition = Vector3.zero;

            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetRotation)
                transform.localRotation = Quaternion.identity;

            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetLocalScale)
                transform.localScale = Vector3.one;

            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;
            }
            tmpChilds.Clear();
        }

        /// <summary>
        /// Resets the transform world matrix but its children remain unchange.
        /// </summary>
        /// <param name="transform"></param>
        public static void ResetWorldButChildrenStays(this Transform transform, ResetMask resetMask = ResetMask.ResetAll)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;
            }

            //reset local:
            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetPosition)
                transform.position = Vector3.zero;

            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetRotation)
                transform.rotation = Quaternion.identity;

            if (resetMask == ResetMask.ResetAll || resetMask == ResetMask.ResetLocalScale)
                transform.localScale = Vector3.one;


            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;
            }
            tmpChilds.Clear();
        }

        /// <summary>
        /// Scale transfrom around pivot.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="pivot"></param>
        /// <param name="newScale"></param>
        public static void ScaleAround(this Transform transform, Vector3 pivot, Vector3 newScale)
        {
            Vector3 A = transform.localPosition;
            Vector3 B = pivot;

            Vector3 C = A - B; // diff from object pivot to desired pivot/origin

            float RS = newScale.x / transform.localScale.x; // relataive scale factor

            // calc final position post-scale
            Vector3 FP = B + C * RS;

            // finally, actually perform the scale/translation
            transform.localScale = newScale;
            transform.localPosition = FP;
        }

        /// <summary>
        /// Sets the position and forward direction.
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="WorldPosition">World position.</param>
        /// <param name="Forward">Forward.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPositionAndForwardDirection(this Transform transform, Vector3 WorldPosition, Vector3 Forward)
        {
            transform.position = WorldPosition;
            transform.forward = Forward;
        }

        /// <summary>
        /// Transform sets parent transform, and set local position and local rotation to zero point.
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="parent">Parent.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetParenAtIdentityPosition(this Transform transform, Transform parent)
        {
            transform.SetParent(parent, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Set world position Y, keep X and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPositionY(this Transform transform, float Y)
        {
            var p = transform.position;
            p.y = Y;
            transform.position = p;
        }

        /// <summary>
        /// Set local position Y, keep X and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionY(this Transform transform, float Y)
        {
            var p = transform.localPosition;
            p.y = Y;
            transform.localPosition = p;
        }

        /// <summary>
        /// Add world position Y, keep X and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPositionY(this Transform transform, float Y)
        {
            var p = transform.position;
            p.y += Y;
            transform.position = p;
        }

        /// <summary>
        /// Add local position Y, keep X and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddLocalPositionY(this Transform transform, float Y)
        {
            var p = transform.localPosition;
            p.y += Y;
            transform.localPosition = p;
        }

        /// <summary>
        /// Set world position X, keep Y and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPositionX(this Transform transform, float X)
        {
            var p = transform.position;
            p.x = X;
            transform.position = p;
        }

        /// <summary>
        /// Set local position X, keep Y and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionX(this Transform transform, float X)
        {
            var p = transform.localPosition;
            p.x = X;
            transform.localPosition = p;
        }

        /// <summary>
        /// Set world position X and Z, keep Y.
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPositionXZ(this Transform transform, float X, float Z)
        {
            var p = transform.position;
            p.x = X;
            p.z = Z;
            transform.position = p;
        }

        /// <summary>
        /// Set local position X and Z, keep Y.
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionXZ(this Transform transform, float X, float Z)
        {
            var p = transform.localPosition;
            p.x = X;
            p.z = Z;
            transform.localPosition = p;
        }

        /// <summary>
        /// Add world position X, keep Y and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPositionX(this Transform transform, float X)
        {
            var p = transform.position;
            p.x += X;
            transform.position = p;
        }

        /// <summary>
        /// Add local position X, keep Y and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddLocalPositionX(this Transform transform, float X)
        {
            var p = transform.localPosition;
            p.y += X;
            transform.localPosition = p;
        }


        /// <summary>
        /// Set world position Z, keep Y and X
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPositionZ(this Transform transform, float Z)
        {
            var p = transform.position;
            p.z = Z;
            transform.position = p;
        }

        /// <summary>
        /// Set local position Z, keep Y and Z
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionZ(this Transform transform, float Z)
        {
            var p = transform.localPosition;
            p.z = Z;
            transform.localPosition = p;
        }

        /// <summary>
        /// Add world position Z, keep X and Y
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddPositionZ(this Transform transform, float Z)
        {
            var p = transform.position;
            p.z += Z;
            transform.position = p;
        }

        /// <summary>
        /// Add local position Z, keep X and Y
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddLocalPositionZ(this Transform transform, float Z)
        {
            var p = transform.localPosition;
            p.z += Z;
            transform.localPosition = p;
        }

        /// <summary>
        /// Sets world position without children transform changed
        /// </summary>
        public static void SetPositionButChildrenStays(this Transform transform, Vector3 WorldPosition)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;//detach children one by one
            }
            transform.position = WorldPosition;
            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;//reattach children
            }
            tmpChilds.Clear();
        }


        /// <summary>
        /// Sets world position without children transform changed
        /// </summary>
        public static void SetLocalPositionButChildrenStays(this Transform transform, Vector3 LocalPosition)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;//detach children one by one
            }
            transform.localPosition = LocalPosition;
            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;//reattach children
            }
            tmpChilds.Clear();
        }


        /// <summary>
        /// Sets world rotation without children transform changed
        /// </summary>
        public static void SetRotationButChildrenStays(this Transform transform, Quaternion WorldRotation)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;//detach children one by one
            }
            transform.rotation = WorldRotation;
            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;//reattach children
            }
            tmpChilds.Clear();
        }


        /// <summary>
        /// Sets world rotation without children transform changed
        /// </summary>
        public static void SetLocalRotationButChildrenStays(this Transform transform, Quaternion LocalRotation)
        {
            tmpChilds.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                tmpChilds.Add(child);
                child.parent = null;//detach children one by one
            }
            transform.localRotation = LocalRotation;
            for (int i = 0; i < tmpChilds.Count; i++)
            {
                var child = tmpChilds[i];
                child.parent = transform;//reattach children
            }
            tmpChilds.Clear();
        }

        /// <summary>
        /// Sets local position and local rotation
        /// </summary>
        /// <param name="transform"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionAndRotation(this Transform transform, Vector3 LocalPosition, Quaternion LocalRotation)
        {
            transform.localPosition = LocalPosition;
            transform.localRotation = LocalRotation;
        }

        /// <summary>
        /// Sort this transform's children by name, ascending or descending.
        /// If depth == true, children tree will be sorted too.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="ascending"></param>
        /// <param name="depth">Should deep down to sort chidren tree?</param>
        public static void SortChildrenByName(this Transform transform, bool ascending, bool depth = false)
        {
        rerun:
            for (int i = 0, iMax = transform.childCount - 1; i < iMax; i++)
            {
                var child1 = transform.GetChild(i);
                var child2 = transform.GetChild(i + 1);
                int compare = child1.name.CompareTo(child2.name);
                //如果 child1 命名比 child2 要后， 则交换位置
                if (ascending && compare > 0)
                {
                    child1.SetSiblingIndex(child1.GetSiblingIndex() + 1);
                    goto rerun;//一直运行到没有出现交换顺序的情况为止:
                }
                //如果 child1 命名比 child2 要前， 则交换位置
                else if (!ascending && compare < 0)
                {
                    child1.SetSiblingIndex(child1.GetSiblingIndex() + 1);
                    goto rerun;//一直运行到没有出现交换顺序的情况为止:
                }
            }

            if (depth)
            {
                for (int i = 0, iMax = transform.childCount; i < iMax; i++)
                {
                    var child = transform.GetChild(i);
                    SortChildrenByName(child, ascending, depth);
                }
            }
        }

        /// <summary>
        /// 给出 Anchor 做为世界坐标空间。Parent 和 Child 做为相对关系的一对空间对象。 获取一个新的世界空间的 Pose， Pose相对于 Anchor 的关系， 和 Child 相对于 Parent 的关系一致。
        /// 此方法的典型应用场景是 XimSDK 的Ground 矫正头部的应用 : Anchor 是Ground Plane, Parent 是头部（Vpu, 相对于 Marker不动), Child 是 Dynamic Marker (相对于 Vpu 运动)
        /// </summary>
        /// <param name="Anchor"></param>
        /// <param name="Parent"></param>
        /// <param name="Child"></param>
        /// <returns></returns>
        public static Pose InverseSpace(Transform Anchor, Transform Parent, Transform Child)
        {
            //ChildPoseInParent : Child在 Parent空间的local pose
            var ChildPoseInParent = new Pose(Parent.InverseTransformPoint(Child.position), Parent.InverseTransformQuaternion(Child.rotation));
            //parentInChildSpace : 将Parent 和 Child 的关系取逆, 得到 Parent 在 Child 空间的Matrix:
            Matrix4x4 parentInChildSpace = Matrix4x4.TRS(ChildPoseInParent.position, ChildPoseInParent.rotation, Vector3.one);
            //parentInChildPosition 和 parentInChildRotation : Parent 在 Chlid空间的 local pos 和 local rotation:
            Vector3 parentInChildPosition = parentInChildSpace.MultiplyPoint3x4(Parent.position);
            Quaternion parentInChildRotation = parentInChildSpace.rotation * Parent.rotation;
            //以 Anchor 为空间， 获取 Anchor 空间下的 Parent相对于 Child 的Pose
            return new Pose(
                Anchor.TransformPoint(parentInChildPosition),
                Anchor.rotation * parentInChildRotation
                );
        }

        /// <summary>
        /// Transform local pose to world space pose.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="localPose"></param>
        /// <returns></returns>
        public static Pose TransformPose(this Transform transform, Pose localPose)
        {
            return new Pose(transform.TransformPoint(localPose.position),
                transform.rotation * localPose.rotation);
        }

        /// <summary>
        /// Transform world space pose to local pose
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="worldPose"></param>
        /// <returns></returns>
        public static Pose InverseTransformPose(this Transform transform, Pose worldPose)
        {
            return new Pose(transform.InverseTransformPoint(worldPose.position), transform.InverseTransformQuaternion(worldPose.rotation));
        }

        /// <summary>
        /// 将 ThisTransform 下面的最多 [Maximum] 个子对象移动到 Another 节点下。
        /// 如果移动的对象等于 【Maximum】,返回true ，否则返回false。
        /// 【MoveCount】: 输出一共移动多少个对象。
        ///
        /// 此方法一般用于池对象的管理 ， 用于将对象移动/移除到一个代表池的根节点下。
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="Another"></param>
        /// <param name="ExpectMoveCount">预期的移动子对象数量。</param>
        /// <param name="StayWorldPosition">SetParent的第二个参数</param>
        /// <param name="MoveCount">输出的移动的子对象的个数。</param>
        /// <returns>移动个数是否等于[Maximum]</returns>
        public static bool MoveChildrensToOtherTransform(this Transform thisTransform, Transform Another, int ExpectMoveCount, bool StayWorldPosition, out int MoveCount)
        {
            MoveCount = 0;
            for (int i = 0, iMax = Mathf.Min(ExpectMoveCount, thisTransform.childCount); i < iMax; i++)
            {
                Transform child = thisTransform.GetChild(0);
                child.SetParent(Another, StayWorldPosition);
                MoveCount++;
            }
            return MoveCount == ExpectMoveCount;
        }

        /// <summary>
        /// 将 ThisTransform 下面的全部子对象移动到 Another 节点下。
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="Another"></param>
        /// <param name="StayWorldPosition"></param>
        /// <param name="Sequential">是否正序？如果false，则从尾巴开始.</param>
        /// <returns>移动的数量</returns>
        public static int MoveAllChildrensToOtherTransform(this Transform thisTransform, Transform Another, bool StayWorldPosition, bool Sequential = true)
        {
            int MoveCount = 0;
            if (Sequential)
            {
                for (int i = 0, iMax = thisTransform.childCount; i < iMax; i++)
                {
                    Transform child = thisTransform.GetChild(0);
                    child.SetParent(Another, StayWorldPosition);
                    MoveCount++;
                }
            }
            else
            {
                for (int i = thisTransform.childCount - 1; i >= 0; i--)
                {
                    Transform child = thisTransform.GetChild(i);
                    child.SetParent(Another, StayWorldPosition);
                    MoveCount++;
                }
            }
            return MoveCount;
        }

        /// <summary>
        /// Finds by child index list under the root node transform.
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="indexList"></param>
        /// <returns></returns>
        public static Transform FindChildByIndexList(this Transform rootNode, IList<int> indexList)
        {
            Transform parent = rootNode;
            Transform child = null;
            for (int i = 0, iMax = indexList.Count; i < iMax; i++)
            {
                int index = indexList[i];
                if (index >= 0 && parent.childCount > index)
                {
                    child = parent.GetChild(index);
                    parent = child;
                }
                else
                {
                    return null;//超出映射范围
                }
            }
            return child;
        }

        /// <summary>
        /// Finds by child index list under the root node transform.
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="indexList"></param>
        /// <returns></returns>
        public static Transform FindChildByIndexList(this Transform rootNode, int[] indexList)
        {
            Transform parent = rootNode;
            Transform child = null;
            for (int i = 0, iMax = indexList.Length; i < iMax; i++)
            {
                int index = indexList[i];
                if (index >= 0 && parent.childCount > index)
                {
                    child = parent.GetChild(index);
                    parent = child;
                }
                else
                {
                    return null;//超出映射范围
                }
            }
            return child;
        }

        /// <summary>
        /// Sets euler angle X
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        public static void SetEulerX(this Transform transform, float x)
        {
            var e = transform.eulerAngles;
            e.x = x;
            transform.eulerAngles = e;
        }

        /// <summary>
        /// Sets euler angle Y
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        public static void SetEulerY(this Transform transform, float y)
        {
            var e = transform.eulerAngles;
            e.y = y;
            transform.eulerAngles = e;
        }

        /// <summary>
        /// Sets euler angle Z
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        public static void SetEulerZ(this Transform transform, float z)
        {
            var e = transform.eulerAngles;
            e.z = z;
            transform.eulerAngles = e;
        }

        /// <summary>
        /// Sets local euler angle X
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        public static void SetLocalEulerX(this Transform transform, float x)
        {
            var localEuler = transform.localEulerAngles;
            localEuler.x = x;
            transform.localEulerAngles = localEuler;
        }

        /// <summary>
        /// Sets local euler angle Y
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        public static void SetLocalEulerY(this Transform transform, float y)
        {
            var localEuler = transform.localEulerAngles;
            localEuler.y = y;
            transform.localEulerAngles = localEuler;
        }

        /// <summary>
        /// Sets local euler angle Z
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        public static void SetLocalEulerZ(this Transform transform, float z)
        {
            var localEuler = transform.localEulerAngles;
            localEuler.z = z;
            transform.localEulerAngles = localEuler;
        }

        /// <summary>
        /// If has child transform at path ?
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool HasChild(this Transform root, string path, out Transform child)
        {
            child = root.Find(path);
            return child != null;
        }
    }
}