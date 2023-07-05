using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.SceneManagement;
using Ximmerse.XR.Asyncoroutine;
using System.Threading.Tasks;
using Array = System.Array;
using System.Text;
using System.Runtime.CompilerServices;
using System;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using Unity.Collections;

namespace Ximmerse
{
    /// <summary>
    /// Polyengine utils.
    /// PolyEngine 工具类方法。
    /// </summary>
    public static partial class PEUtils
    {
        private const string kErrorMsg_01 = "{0} not child of {1} !";

        //Cached for screen phsyicsal size
        static float s_ScreenPhysicalWidth = -1;
        static float s_ScreenPhysicalHeight = -1;
        static float s_ScreenDPIForAndroid = -1;

        static UIntFloat floatConverter;

        static DecimalLong decimalLongConverter;

        static System.Type kEditorUtilityType = null;
        static System.Reflection.MethodInfo kMethodEditorSetDirty = null;

        static List<Vector3> LineRendererPositions = new List<Vector3>();

        static Plane[] FrustumPlanes = new Plane[6];//Frustum 由 6个plane组成

        private static System.Security.Cryptography.SHA1 kHash = new System.Security.Cryptography.SHA1CryptoServiceProvider();

        // -- helpers for float conversion --//
        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntFloat
        {
            [FieldOffset(0)]
            public float floatValue;

            [FieldOffset(0)]
            public uint uintValue;

            [FieldOffset(0)]
            public double doubleValue;

            [FieldOffset(0)]
            public ulong ulongValue;

            /// <summary>
            /// Converts ulong to double.
            /// </summary>
            /// <returns>The double.</returns>
            /// <param name="value">Value.</param>
            public double ToDouble(ulong value)
            {
                ulongValue = value;
                return doubleValue;
            }
        }

        // -- helpers for decimal conversion --//
        [StructLayout(LayoutKind.Explicit)]
        internal struct DecimalLong
        {
            [FieldOffset(0)]
            public decimal _deciaml;

            [FieldOffset(0)]
            public ulong ulong01;

            [FieldOffset(8)]
            public ulong ulong02;
        }

        /// <summary>
        /// Adds element to an array.
        /// </summary>
        /// <returns>The to array.</returns>
        /// <param name="element">Element.</param>
        /// <param name="Array">Array.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] AddToArray<T>(T element, T[] Array)
        {
            T[] newArray = new T[Array.Length + 1];
            System.Array.Copy(Array, newArray, Array.Length);
            newArray[newArray.Length - 1] = element;
            return newArray;
        }
        /// <summary>
        /// Appends a format line.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static StringBuilder AppendFormattedLine(this StringBuilder buffer, string format, params object[] args)
        {
            return buffer.AppendLine(string.Format(format, args));
        }

        /// <summary>
        /// child 必须是 root 的子对象, 此方法计算 child 相对于 root 的 local matrix.
        /// 使用此方法可以计算出 prefab 的子对象相对于根对象的坐标偏移，而不需要实例化prefab.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="child"></param>
        /// <param name="matrix"></param>
        public static void CalculateLocalMatrix(Transform root, Transform child, out Matrix4x4 matrix)
        {
            if (!child.IsChildOf(root))
            {
                Debug.LogErrorFormat(child, kErrorMsg_01, child.name, root.name);
                matrix = Matrix4x4.zero;
                return;
            }
            matrix = Matrix4x4.identity;
            internalCalculateLocalMatrix(root, child, ref matrix);
        }

        private static void internalCalculateLocalMatrix(Transform root, Transform child, ref Matrix4x4 matrix)
        {
            var p = child.parent;
            var lp = child.localPosition;
            var lq = child.localRotation;
            var ls = child.localScale;
            matrix = Matrix4x4.TRS(lp, lq, ls) * matrix;

            if (!ReferenceEquals(p, root))
            {
                internalCalculateLocalMatrix(root, p, ref matrix);
            }
        }

        /// <summary>
        /// Transform approach to target position, at maxium speed, there is no need to multiple Time.deltaTime to speed,
        /// the method internally applys Time.deltaTime.
        /// Call this method per frame.
        /// Return true for totally position approaching.
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="TargetPosition"></param>
        /// <param name="Speed"></param>
        public static bool MoveToPosition(this Transform thisTransform, Vector3 TargetPosition, float Speed)
        {
            Vector3 vec3 = TargetPosition - thisTransform.position;
            Vector3 dir = vec3.normalized;
            float speed = Speed * Time.deltaTime;
            if (vec3.magnitude <= speed)
            {
                thisTransform.position = TargetPosition;
                return true;
            }
            else
            {
                thisTransform.position = thisTransform.position + dir * speed;
                return false;
            }
        }


        /// <summary>
        /// Transform approach to target position, at maxium speed, there is no need to multiple Time.deltaTime to speed at delta time.
        /// the method internally applys Time.deltaTime.
        /// Call this method per frame.
        /// Return true for totally position approaching.
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="TargetPosition"></param>
        /// <param name="Speed"></param>
        public static bool MoveToPosition(this Transform thisTransform, Vector3 TargetPosition, float Speed, float DeltaTime)
        {
            Vector3 vec3 = TargetPosition - thisTransform.position;
            Vector3 dir = vec3.normalized;
            float speed = Speed * DeltaTime;
            if (vec3.magnitude <= speed)
            {
                thisTransform.position = TargetPosition;
                return true;
            }
            else
            {
                thisTransform.position = thisTransform.position + dir * speed;
                return false;
            }
        }


        /// <summary>
        /// Transform approach to target position, at maxium speed, there is no need to multiple Time.deltaTime to speed,
        /// the method internally applys Time.deltaTime.
        /// Call this method per frame.
        /// Return true for totally position approaching.
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="Target"></param>
        /// <param name="Speed"></param>
        public static bool MoveToPosition(this Transform thisTransform, Transform Target, float Speed)
        {
            if (!Target)
                return false;
            return MoveToPosition(thisTransform, Target.position, Speed);
        }

        /// <summary>
        /// This transform rotate its world rotation to target quaternion, at angular speed.
        /// Return true when reaches target quaternion.
        /// Commonly this method will be called per frame
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="TargetQ"></param>
        /// <param name="AngularSpeed">Will be multiple with Time.delta</param>
        /// <returns></returns>
        public static bool RotateTo(this Transform thisTransform, Quaternion TargetQ, float AngularSpeed)
        {
            float diffAngle = Quaternion.Angle(thisTransform.rotation, TargetQ);
            float angularSpeedAtFrame = AngularSpeed * Time.deltaTime;
            if (diffAngle <= angularSpeedAtFrame)
            {
                thisTransform.rotation = TargetQ;
                return true;
            }
            else
            {
                thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, TargetQ, angularSpeedAtFrame);
                return false;
            }
        }
        /// <summary>
        /// This transform rotate its world rotation to target transform's world-uaternion, at angular speed.
        /// Return true when reaches target quaternion.
        /// Commonly this method will be called per frame
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="TargetT">target transform</param>
        /// <param name="AngularSpeed">Will be multiple with Time.delta</param>
        /// <returns></returns>
        public static bool RotateTo(this Transform thisTransform, Transform TargetT, float AngularSpeed)
        {
            if (!TargetT)
            {
                return false;
            }
            float diffAngle = Quaternion.Angle(thisTransform.rotation, TargetT.rotation);
            float angularSpeedAtFrame = AngularSpeed * Time.deltaTime;
            if (diffAngle <= angularSpeedAtFrame)
            {
                thisTransform.rotation = TargetT.rotation;
                return true;
            }
            else
            {
                thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, TargetT.rotation, angularSpeedAtFrame);
                return false;
            }
        }


        /// <summary>
        /// This transform rotate its world rotation to target transform's world-uaternion, at angular speed of delta time
        /// Return true when reaches target quaternion.
        /// Commonly this method will be called per frame
        /// </summary>
        /// <param name="thisTransform"></param>
        /// <param name="TargetT">target transform</param>
        /// <param name="AngularSpeed">Will be multiple with Time.delta</param>
        /// <returns></returns>
        public static bool RotateTo(this Transform thisTransform, Transform TargetT, float AngularSpeed, float deltaTime)
        {
            if (!TargetT)
            {
                return false;
            }
            float diffAngle = Quaternion.Angle(thisTransform.rotation, TargetT.rotation);
            float angularSpeedAtFrame = AngularSpeed * deltaTime;
            if (diffAngle <= angularSpeedAtFrame)
            {
                thisTransform.rotation = TargetT.rotation;
                return true;
            }
            else
            {
                thisTransform.rotation = Quaternion.RotateTowards(thisTransform.rotation, TargetT.rotation, angularSpeedAtFrame);
                return false;
            }
        }



        /// <summary>
        /// Creates the file path's parent directory.
        /// Return true for directory created or already exists.
        /// </summary>
        /// <returns><c>true</c>, if file directory was created, <c>false</c> otherwise.</returns>
        /// <param name="FileFullPath">File full path.</param>
        public static bool CreateFileDirectory(string FileFullPath)
        {
            var parentDirectory = Directory.GetParent(FileFullPath);
            if (!parentDirectory.Exists)
            {
                try
                {
                    Directory.CreateDirectory(parentDirectory.FullName);
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Smart destroy self unity object - in editor , call destroy immediately.
        /// In runtime, call destroy with delay.
        /// </summary>
        /// <param name="unityObj"></param>
        /// <param name="delayed">Used in runtime only. In editor, always call destroyImmediately</param>
        /// <param name="allowDestoryAsset"></param>
        public static void Destroy(this Object unityObj, float delayed = 0, bool allowDestoryAsset = true)
        {
            if (unityObj != null)
            {
                if (Application.isEditor && Application.isPlaying == false)
                {
                    Object.DestroyImmediate(unityObj, allowDestoryAsset);
                }
                else
                {
                    Object.Destroy(unityObj, delayed);
                }
            }
        }

        /// <summary>
        /// Compares two unity object, check if both elements and array length are same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array01"></param>
        /// <param name="array02"></param>
        /// <returns></returns>
        public static bool EqualsTo<T>(this T[] array01, T[] array02) where T : UnityEngine.Object
        {
            if (array01.Length != array02.Length)
            {
                return false;
            }
            for (int i = 0; i < array01.Length; i++)
            {
                if (array01[i] != array02[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Removes the index from array and return new array
        /// </summary>
        /// <returns>The from array.</returns>
        /// <param name="index">Index.</param>
        /// <param name="Array">Array.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T[] RemoveFromArray<T>(int index, T[] Array)
        {
            T[] newArray = new T[Array.Length - 1];
            int copyIndex = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                if (i != index)
                {
                    newArray[copyIndex] = Array[i];
                    copyIndex++;
                }
                else
                    continue;
            }
            return newArray;
        }

        /// <summary>
        /// Removes element from array.
        /// </summary>
        /// <returns>The from array.</returns>
        /// <param name="element">Element.</param>
        /// <param name="Array">Array.</param>
        public static T[] RemoveFromArray<T>(T element, T[] Array)
        {
            int c = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Equals(element))
                {
                    c++;
                }
            }
            if (c == 0)
            {
                return new T[] { };
            }
            else
            {
                T[] ts = new T[Array.Length - c];
                int idx = 0;
                for (int i = 0; i < Array.Length; i++)
                {
                    if (!Array[i].Equals(element))
                    {
                        ts[idx++] = element;
                    }
                }
                return ts;
            }

        }

        /// <summary>
        /// 返回一个 vector3 的 单位元随机向量。
        /// </summary>
        /// <returns>The unit circle.</returns>
        public static Vector3 InsideUnitCircle()
        {
            var rnd = Random.insideUnitCircle;
            return new Vector3(rnd.x, 0, rnd.y);
        }



        public static Vector3 Snap(Vector3 pos)
        {
            pos.Set(Snap(pos.x), Snap(pos.y), Snap(pos.z));
            return pos;
        }

        /// <summary>
        /// 在 GameObject 上对所有的children遍历式的设置hide flags.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="hideFlags"></param>
        public static void SetHideflagsRecursive(this GameObject gameObject, HideFlags hideFlags)
        {
            gameObject.hideFlags = hideFlags;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.hideFlags = hideFlags;
            }
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                SetHideflagsRecursive(gameObject.transform.GetChild(i).gameObject, hideFlags);
            }
        }

        /// <summary>
        /// Sets transform's global scale.
        /// This is not thread safe API, should be called at main thread only.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="globalScale"></param>
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        /// <summary>
        /// Sets transforms pose, can be local or global.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="targetPose"></param>
        /// <param name="space"></param>
        public static void SetPose(this Transform transform, Pose targetPose, Space space)
        {
            if (space == Space.Self)
            {
                transform.localPosition = targetPose.position;
                transform.localRotation = targetPose.rotation;
            }
            else
            {
                transform.position = targetPose.position;
                transform.rotation = targetPose.rotation;
            }
        }

        /// <summary>
        /// Set position to line renderer.
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="transforms"></param>
        public static void SetPositions(this LineRenderer lineRenderer, Transform[] transforms)
        {
            lineRenderer.positionCount = transforms.Length;
            for (int i = 0; i < transforms.Length; i++)
            {
                lineRenderer.SetPosition(i, transforms[i].position);
            }
        }

        /// <summary>
        /// Set position to line renderer
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="transforms"></param>
        public static void SetPositions(this LineRenderer lineRenderer, IList<Transform> transforms)
        {
            lineRenderer.positionCount = transforms.Count;
            for (int i = 0; i < transforms.Count; i++)
            {
                lineRenderer.SetPosition(i, transforms[i].position);
            }
        }

        /// <summary>
        /// Set position to line renderer
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="Points"></param>
        public static void SetPositions(this LineRenderer lineRenderer, IList<Vector3> Points)
        {
            lineRenderer.positionCount = Points.Count;
            for (int i = 0; i < Points.Count; i++)
            {
                lineRenderer.SetPosition(i, Points[i]);
            }
        }

        /// <summary>
        /// Set position to line renderer.
        /// </summary>
        /// <param name="lineRenderer"></param>
        /// <param name="Points"></param>
        public static void SetPositions(this LineRenderer lineRenderer, Vector3[] Points)
        {
            lineRenderer.positionCount = Points.Length;
            for (int i = 0; i < Points.Length; i++)
            {
                lineRenderer.SetPosition(i, Points[i]);
            }
        }

        /// <summary>
        /// 按照文件后缀名输出被找到的所有文件。
        /// 返回文件的数量.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="files"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="extensions">extension of the file: .txt, .mp3, .fbx</param>
        /// <returns></returns>
        public static int SearchFileByExtension(string path, IList<string> files, bool ignoreCase = true, params string[] extensions)
        {
            if (files == null)
            {
                throw new UnityException("List: files == NULL");
            }

            int matchCount = 0;
            for (int i = 0; i < extensions.Length; i++)
            {
                string ext = extensions[i];
                string[] _files = Directory.GetFiles(path);
                for (int j = 0, _filesLength = _files.Length; j < _filesLength; j++)
                {
                    var _file = _files[j];
                    string _extension = Path.GetExtension(_file);
                    int comp = string.Compare(ext, _extension, ignoreCase: true);
                    if (comp == 0)
                    {
                        files.Add(_file);
                        matchCount++;
                    }
                }
            }
            return matchCount;
        }

        /// <summary>
        /// Gets transforms pose, can be local or global.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="space"></param>
        public static Pose GetPose(this Transform transform, Space space)
        {
            if (space == Space.Self)
            {
                return new Pose(transform.localPosition, transform.localRotation);
            }
            else
            {
                return new Pose(transform.position, transform.rotation);
            }
        }

        /// <summary>
        /// Sort the array by comparer.
        /// </summary>
        /// <param name="array">Array.</param>
        /// <param name="comparer">Comparer.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Sort<T>(this T[] array, IComparer<T> comparer)
        {
            Array.Sort<T>(array, comparer);
        }

        /// <summary>
        /// Sort the array by comparer.
        /// </summary>
        /// <param name="array">Array.</param>
        /// <param name="comparer">Comparer.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Sort<T>(this T[] array, IComparer<T> comparer, int startIndex, int length)
        {
            Array.Sort<T>(array, startIndex, length, comparer);
        }

        public static float Snap(float f)
        {
            float fraction = f - (int)f;
            f = fraction >= 0.5f ? Mathf.CeilToInt(f) : Mathf.FloorToInt(f);
            return f;
        }

        public static Vector3 SnapHalf(Vector3 pos)
        {
            float x = SnapHalf(pos.x);
            float y = SnapHalf(pos.y);
            float z = SnapHalf(pos.z);
            pos.Set(x, y, z);
            return pos;
        }

        public static float SnapHalf(float f)
        {
            int _base = (int)f;
            float fraction = f - (int)f;
            if (fraction < 0.25f)
            {
                fraction = 0;
            }
            else if (fraction > 0.25f && fraction < 0.75f)
            {
                fraction = 0.5f;
            }
            else
            {
                fraction = 1;
            }
            return (float)_base + fraction;
        }

        /// <summary>
        /// Check if arrays includes the value
        /// </summary>
        public static bool Include<T>(T[] arrays, T value)
        {
            for (int i = 0; i < arrays.Length; i++)
            {
                if (arrays[i].Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get random vector3 between [min] .. [max]
        /// </summary>
        /// <returns>The range.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static Vector3 RandomRange(Vector3 min, Vector3 max)
        {
            return new Vector3(Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z));
        }

        /// <summary>
        /// Get random vector2 between [min] .. [max]
        /// </summary>
        /// <returns>The range.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RandomRange(Vector2 min, Vector2 max)
        {
            return new Vector2(Random.Range(min.x, max.x),
                Random.Range(min.y, max.y));
        }

        /// <summary>
        /// Get random color between [min] .. [max]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color RandomRange(Color min, Color max)
        {
            return new Color(Random.Range(min.r, max.r), Random.Range(min.g, max.g), Random.Range(min.b, max.b), Random.Range(min.a, max.a));
        }

        /// <summary>
        /// Get random color between [min] .. [max]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 RandomRange(Color32 min, Color32 max)
        {
            return new Color32((byte)Random.Range((int)min.r, (int)(max.r + 1)),
                (byte)Random.Range((int)min.g, (int)(max.g + 1)),
                (byte)Random.Range((int)min.b, (int)(max.b + 1)),
                (byte)Random.Range((int)min.a, (int)(max.a + 1)));
        }

        /// <summary>
        /// Gets the or add component.
        /// </summary>
        /// <returns>The or add component.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            if (!component)
                component = self.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// Gets or add a child gameObject of the name.
        /// If the child gameObject is created, it is created at the identity point to its parent space.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static GameObject GetOrAddChild(this GameObject gameObject, string childName)
        {
            var child = gameObject.transform.Find(childName);
            if (!child)
            {
                GameObject gameObjectChild = new GameObject(childName);
                gameObjectChild.transform.SetParenAtIdentityPosition(gameObject.transform);
                child = gameObjectChild.transform;
            }
            return child.gameObject;
        }


        /// <summary>
        /// Gets or add a child gameObject of the name.
        /// If the child gameObject is created, it is created at the identity point to its parent space.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static GameObject GetOrAddChild(this Component component, string childName)
        {
            var child = component.transform.Find(childName);
            if (!child)
            {
                GameObject gameObjectChild = new GameObject(childName);
                gameObjectChild.transform.SetParenAtIdentityPosition(component.transform);
                child = gameObjectChild.transform;
            }
            return child.gameObject;
        }

        /// <summary>
        /// Gets the or add component.
        /// </summary>
        /// <returns>The or add component.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
        {
            return self.gameObject.GetOrAddComponent<T>();
        }

        /// <summary>
        /// Get first not null reference from the objects array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static T GetNotNull<T>(params T[] objects) where T : class
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    return objects[i];
                }
            }
            return default(T);
        }

        /// <summary>
        /// Gets the distance measured in sreen pixel space. Returned distance is measured in meters.
        /// </summary>
        /// <returns>The distance of screen points.</returns>
        public static float GetPhysicalDistanceOfScreenSpace(float screenDistance)
        {
            float dpi;
            GetActualDPI(out dpi);
            if (dpi <= 0)
            {
                return 0;
            }
            else
            {
                return (screenDistance / dpi) * 0.0254f;
            }
        }

        /// <summary>
        /// If the name of the scene exists in the build scene list ?
        /// The first parameter is the unity scene name without extension.
        /// </summary>
        /// <returns>The scene by name.</returns>
        /// <param name="Name">Name.</param>
        public static bool IsSceneInBuildList(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return false;
            }
            int sCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (!string.IsNullOrEmpty(path) && Name.Equals(Path.GetFileNameWithoutExtension(path)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the index of the builtin scene that matches that name.
        /// </summary>
        /// <returns><c>true</c>, if scene build index was gotten, <c>false</c> otherwise.</returns>
        /// <param name="Name">Name.</param>
        public static int GetSceneBuildIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return -1;
            }
            int sCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sCount; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (!string.IsNullOrEmpty(path) && Name.Equals(Path.GetFileNameWithoutExtension(path)))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the actual DPI.
        /// Always return true if not running on Android.
        /// When running on android, try getting DPI from native java class, if failed, using Screen.dpi and return false.
        /// </summary>
        /// <returns><c>true</c>, if actual DP was gotten, <c>false</c> otherwise.</returns>
        /// <param name="dpi">Dpi.</param>
        public static bool GetActualDPI(out float dpi)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (s_ScreenDPIForAndroid != -1)
                {
                    dpi = s_ScreenDPIForAndroid;
                    return true;
                }
                try
                {
                    AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

                    AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
                    activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

                    dpi = (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;
                    s_ScreenDPIForAndroid = dpi;
                    return true;
                }
                catch
                {
                    dpi = Screen.dpi; //fail to get native interface for DPI
                    return false;
                }
            }
            else
            {
                dpi = Screen.dpi;
                return true;
            }
        }

        /// <summary>
        /// Gets the size of the physical screen.
        /// return false if fail to get current DPI.
        /// useDensityDpiInAnroid : refer to file:///Applications/Unity/Documentation/en/ScriptReference/Screen-dpi.html
        /// If useDensityDpiInAnroid = true, use Screen.dpi in calculation, else use native java class interface to get a more precise DPI in calculation.
        /// </summary>
        /// <returns><c>true</c>, if physical screen size was gotten, <c>false</c> otherwise.</returns>
        /// <param name="w">The width.</param>
        /// <param name="h">The height.</param>
        public static bool GetPhysicalScreenSize(out float w, out float h)
        {
            if (s_ScreenPhysicalWidth != -1 && s_ScreenPhysicalHeight != -1)
            {
                w = s_ScreenPhysicalWidth;
                h = s_ScreenPhysicalHeight;
                return true;
            }
            float dpi = 0;
            GetActualDPI(out dpi);

            if (dpi <= 0)
            {
                w = 0;
                h = 0;
                return false;
            }
            else
            {
                float screenWidth = Screen.safeArea.width;//width in pixel
                float screenHeight = Screen.safeArea.height; //height in pixel
                float ScreenWidthInch = screenWidth / dpi;
                float ScreenHeightInch = screenHeight / dpi;
                w = ScreenWidthInch * 0.0254f;
                h = ScreenHeightInch * 0.0254f;

                s_ScreenPhysicalWidth = w;
                s_ScreenPhysicalHeight = h;
                return screenWidth > 0 && screenHeight > 0;
            }
        }

        /// <summary>
        /// Gets a random element from list. 
        /// </summary>
        /// <returns><c>true</c>, if random was gotten, <c>false</c> otherwise.</returns>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRandom<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default(T);
            int index = Random.Range(0, list.Count);
            T retVal = list[index];
            return retVal;
        }

        /// <summary>
        /// Gets a random element from read-only list. 
        /// </summary>
        /// <returns><c>true</c>, if random was gotten, <c>false</c> otherwise.</returns>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRandomFromReadOnlyList<T>(this IReadOnlyList<T> list)
        {
            if (list.Count == 0)
                return default(T);
            int index = Random.Range(0, list.Count);
            T retVal = list[index];
            return retVal;
        }

        /// <summary>
        /// Gets a random element from list.
        /// if [dropTheIndex] = true, the list will remove the chosen element
        /// </summary>
        /// <returns><c>true</c>, if random was gotten, <c>false</c> otherwise.</returns>
        /// <param name="list">List.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetRandom<T>(List<T> list, bool dropTheIndex = true)
        {
            int index = Random.Range(0, list.Count);
            T retVal = list[index];
            if (dropTheIndex)
            {
                list.RemoveAt(index);
            }
            return retVal;
        }

        /// <summary>
        /// Gets random element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRandom<T>(this T[] array)
        {
            if (array.Length == 0)
            {
                return default(T);
            }
            int index = Random.Range(0, array.Length);
            T retVal = array[index];
            return retVal;
        }

        /// <summary>
        /// 获得p1,p2两点在XZ平面上的Distance
        /// </summary>
        /// <returns>The XZ sqr distance.</returns>
        /// <param name="p1">P1.</param>
        /// <param name="p2">P2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetXZDistance(this Vector3 p1, Vector3 p2)
        {
            p1.y = 0;
            p2.y = 0;
            return Vector3.Distance(p1, p2);
        }


        /// <summary>
        /// 获得p1,p2两点在XZ平面上的Sqr Distance
        /// </summary>
        /// <returns>The XZ sqr distance.</returns>
        /// <param name="p1">P1.</param>
        /// <param name="p2">P2.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetXZSqrDistance(this Vector3 p1, Vector3 p2)
        {
            Vector3 vect = p1 - p2;
            vect.y = 0;
            return vect.sqrMagnitude;
        }

        /// <summary>
        /// Check element exists at array
        /// </summary>
        /// <param name="array">Array.</param>
        /// <param name="element">Element.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool Exists<T>(T[] array, T element)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (element.Equals(array[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 计算transform.forward到 targetPosition 的夹角。
        /// 只考虑XZ平面的夹角。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateFacingAngleXZ(Transform transform, Vector3 targetPosition)
        {
            Vector3 dir1 = transform.forward;
            Vector3 dir2 = targetPosition - transform.position;
            dir1.y = dir1.z;
            dir2.y = dir2.z;
            return Vector2.Angle(dir1, dir2);
        }

        /// <summary>
        /// Clear the array , set all elements to be null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] array) where T : class
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = null;
            }
        }

        /// <summary>
        /// Clear the array , set elements at the range to be null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this T[] array, int startIndex, int length) where T : class
        {
            for (int i = startIndex; i < length; i++)
            {
                if (i >= 0 && i <= array.Length - 1)
                {
                    array[i] = null;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Clamps the scalar.
        /// 限制 single 的长度不大于 Scalar， 返回符号 = single 的浮点值.
        /// </summary>
        /// <returns>The scalar.</returns>
        /// <param name="single">Single.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampScalar(this float single, float Scalar)
        {
            int sign = single < 0 ? -1 : 1;
            single = Mathf.Min(Mathf.Abs(single), Mathf.Abs(Scalar));
            return single * sign;
        }

        /// <summary>
        /// Clamps the vect2 in rect bounds.
        /// </summary>
        /// <returns>The rect vect2.</returns>
        /// <param name="rect">Rect.</param>
        /// <param name="point">Point.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Clamp(this Rect rect, Vector2 point)
        {
            if (rect.xMin <= point.x && rect.xMax >= point.x && rect.yMin <= point.y && rect.yMax >= point.y)
                return point;
            else
                return new Vector2(Mathf.Clamp(point.x, rect.xMin, rect.xMax), Mathf.Clamp(point.y, rect.yMin, rect.yMax));
        }

        /// <summary>
        /// Clamps a vector3 into min and max range.
        /// </summary>
        /// <param name="vector3"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Clamp(this Vector3 vector3, Vector3 min, Vector3 max)
        {
            vector3.x = Mathf.Clamp(vector3.x, min.x, max.x);
            vector3.y = Mathf.Clamp(vector3.y, min.y, max.y);
            vector3.z = Mathf.Clamp(vector3.z, min.z, max.z);
            return vector3;
        }

        /// <summary>
        /// 把一个 0 - 360之间的代表角度的4个长度的float压缩为一个2个长度的 short。
        /// </summary>
        /// <returns>The angle.</returns>
        public static short ZipAngle(float angle)
        {
            if (angle >= 360 || angle <= 0)
            {
                angle = Mathf.Repeat(angle, 360);
            }
            short ret = (short)angle;
            return ret;
        }

        public static float[] ToArray(this Vector3 vector3)
        {
            return new float[] { vector3.x, vector3.y, vector3.z };
        }

        public static void ToArray(this Vector3 vector3, float[] array, int startIndex = 0)
        {
            array[startIndex] = vector3.x;
            array[startIndex + 1] = vector3.y;
            array[startIndex + 2] = vector3.z;
        }

        public static void ToArray(this Vector3 vector3, NativeArray<float> array, int startIndex = 0)
        {
            array[startIndex] = vector3.x;
            array[startIndex + 1] = vector3.y;
            array[startIndex + 2] = vector3.z;
        }


        public static float[] ToArray(this Vector4 vector4)
        {
            return new float[] { vector4.x, vector4.y, vector4.z, vector4.w };
        }

        public static void ToArray(this Vector4 vector4, float[] array, int startIndex = 0)
        {
            array[startIndex] = vector4.x;
            array[startIndex + 1] = vector4.y;
            array[startIndex + 2] = vector4.z;
            array[startIndex + 3] = vector4.w;
        }

        public static void ToArray(this Vector4 vector4, NativeArray<float> array, int startIndex = 0)
        {
            array[startIndex] = vector4.x;
            array[startIndex + 1] = vector4.y;
            array[startIndex + 2] = vector4.z;
            array[startIndex + 3] = vector4.w;
        }

        public static float[] ToArray(this Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        public static void ToArray(this Quaternion q, float[] array, int startIndex = 0)
        {
            array[startIndex] = q.x;
            array[startIndex + 1] = q.y;
            array[startIndex + 2] = q.z;
            array[startIndex + 3] = q.w;
        }

        public static void ToArray(this Quaternion q, NativeArray<float> array, int startIndex = 0)
        {
            array[startIndex] = q.x;
            array[startIndex + 1] = q.y;
            array[startIndex + 2] = q.z;
            array[startIndex + 3] = q.w;
        }



        /// <summary>
        /// 用一个 Vector2 表示一个方向.
        /// Vector2.x = 方向的Yaw角度。
        /// Vector2.y = 方向的Pitch角度。
        /// </summary>
        /// <returns>The direction.</returns>
        /// <param name="Direction">Direction.</param>
        public static Vector2 ZipDirection(Vector3 Direction)
        {
            if (Direction != Vector3.zero)
            {
                var euler = Quaternion.LookRotation(Direction).eulerAngles;
                return new Vector2(euler.x, euler.y);
            }
            else
                return Vector2.zero;
        }

        /// <summary>
        /// 把 Rotation 压缩成 Vector2. X = Yaw, Y = Pitch, 忽略 Roll
        /// </summary>
        /// <returns>The direction.</returns>
        /// <param name="rotation">Rotation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ZipDirection(Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            return new Vector2(euler.x, euler.y);
        }

        /// <summary>
        /// ZipDirection 的反函数. 把一个压缩过的 Dir 还原.
        /// 只能还原 Yaw 和 Pitch, 不适用于带 Roll 的方向。
        /// </summary>
        /// <returns>The direction.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 UnzipDirection(Vector2 eulerXY)
        {
            var dir = Quaternion.Euler(eulerXY.x, eulerXY.y, 0) * Vector3.forward;
            return dir;
        }

        /// <summary>
        /// 压缩一个 Direction : 把Direction投影到XZ面，用一个 Float 表示 XZ 投影向量和 (1,0)的角度
        /// </summary>
        /// <returns>The direction flatten.</returns>
        /// <param name="Direction">Direction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ZipXZDirection(Vector3 Direction)
        {
            Direction.y = 0;
            Vector2 xz = new Vector2(Direction.x, Direction.z);
            float angle = PEMathf.SignedAngle(xz, new Vector2(1, 0));
            return angle;
        }

        /// <summary>
        /// ZipXZDirection 的反函数 。 把一个 angle 还原为一个 Vector3 方向.
        /// </summary>
        /// <returns>The XZ direction.</returns>
        /// <param name="angle">Angle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 UnzipXZDirection(float angle)
        {
            float rad = Mathf.Deg2Rad * angle;
            float x = Mathf.Cos(rad);
            float z = Mathf.Sin(rad);
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// Open directory in file browser.
        /// Only supports windows/mac osx
        /// </summary>
        /// <param name="directoty"></param>
        public static void OpenInFileBrowser(string directoty)
        {
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                OpenInMacFileBrowser(directoty);
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                OpenInWinFileBrowser(directoty);
            //unsupported platform
        }

        private static void OpenInMacFileBrowser(string path)
        {
            bool openInsidesOfFolder = false;

            // try mac
            string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

            if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }
            else
            {
                return;
            }

            //Debug.Log("macPath: " + macPath);
            //Debug.Log("openInsidesOfFolder: " + openInsidesOfFolder);

            if (!macPath.StartsWith("\""))
            {
                macPath = "\"" + macPath;
            }
            if (!macPath.EndsWith("\""))
            {
                macPath = macPath + "\"";
            }
            string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
            //Debug.Log("arguments: " + arguments);
            try
            {
                System.Diagnostics.Process.Start("open", arguments);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        private static void OpenInWinFileBrowser(string path)
        {
            bool openInsidesOfFolder = false;

            // try windows
            string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }
            else
            {
                return;
            }
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        /// <summary>
        /// Gets the world position of the component
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 position(this Component component)
        {
            return component.transform.position;
        }

        /// <summary>
        /// Gets the world rotation of the component
        /// </summary>
        /// <returns>The rotation.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion rotation(this Component component)
        {
            return component.transform.rotation;
        }

        /// <summary>
        /// Gets the world euler of the component
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 eulerAngles(this Component component)
        {
            return component.transform.eulerAngles;
        }

        /// <summary>
        /// Gets the local position of the component
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 localPosition(this Component component)
        {
            return component.transform.localPosition;
        }
        /// <summary>
        /// Gets the local rotation of the component
        /// </summary>
        /// <returns>The rotation.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion localRotation(this Component component)
        {
            return component.transform.localRotation;
        }

        /// <summary>
        /// Lerp pose between this pose and target pose.
        /// </summary>
        /// <param name="thisPose"></param>
        /// <param name="targetPose"></param>
        /// <param name="lerpT"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose LerpPose(this Pose thisPose, Pose targetPose, float lerpT)
        {
            var p = Vector3.Lerp(thisPose.position, targetPose.position, lerpT);
            var q = Quaternion.Lerp(thisPose.rotation, targetPose.rotation, lerpT);
            return new Pose(p, q);
        }

        /// <summary>
        /// Slerp pose between this pose and target pose.
        /// </summary>
        /// <param name="thisPose"></param>
        /// <param name="targetPose"></param>
        /// <param name="lerpT"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pose SlerpPose(this Pose thisPose, Pose targetPose, float lerpT)
        {
            var p = Vector3.Slerp(thisPose.position, targetPose.position, lerpT);
            var q = Quaternion.Slerp(thisPose.rotation, targetPose.rotation, lerpT);
            return new Pose(p, q);
        }

        /// <summary>
        /// Gets the local euler of the component
        /// </summary>
        /// <returns>The position.</returns>
        /// <param name="component">Component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 localEulerAngles(this Component component)
        {
            return component.transform.localEulerAngles;
        }

        /// <summary>
        /// Using a ray to raycast the camera's frustum planes.
        /// Returns the raycast distance.
        /// Return false for not hit any point of the frustum planes.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool RaycastOnCameraFrustum(this Ray ray, Camera camera, out float distance)
        {
            GeometryUtility.CalculateFrustumPlanes(camera, FrustumPlanes); //get 6 frustum planes
            float nearestHitDistance = Mathf.Infinity;
            bool hitAnyPlane = false;
            for (int i = 0; i < 6; i++)
            {
                bool hit = FrustumPlanes[i].Raycast(ray, out float _d);
                Vector3 hitPoint = ray.GetPoint(_d);
                //如果和6个plane中的任意一个有命中:
                if (hit && _d < nearestHitDistance)
                {
                    nearestHitDistance = _d;
                    hitAnyPlane = true;
                }
            }
            if (hitAnyPlane)
            {
                distance = nearestHitDistance;
                return true;
            }
            else
            {
                distance = 0;
                return false;
            }
        }


        /// <summary>
        /// 给出 File Path 文件完整路径， 重命名文件后缀。
        /// </summary>
        /// <param name="FilePath">文件完整路径</param>
        /// <param name="FileExtension">.png, .jpeg 等等</param>
        /// <returns></returns>
        public static string RenameFileExtension(string FilePath, string FileExtension)
        {
            string DirectoryName = Directory.GetParent(FilePath).FullName;
            string FileNameNoExt = Path.GetFileNameWithoutExtension(FilePath);
            return Path.Combine(DirectoryName, FileNameNoExt + FileExtension);
        }

        /// <summary>
        /// Get a random value from array, the result will exclude the value.
        /// If the array has only one element, then there is no choice - result will be the only element.
        /// </summary>
        public static T RandomExcept<T>(this T[] array, T ExcludedValue)
        {
            if (array.Length == 1)
            {
                return array[0];
            }
            else if (array.Length == 2)
            {
                return array[0].Equals(ExcludedValue) ? array[1] : array[0];
            }
            else
            {
                NativeArray<int> IndicesPool = new NativeArray<int>(array.Length - 1, Allocator.Temp, NativeArrayOptions.ClearMemory);
                for (int i = 0, iMax = array.Length - 1; i < iMax; i++)
                {
                    if (array[i].Equals(ExcludedValue))
                    {
                        continue;
                    }
                    IndicesPool[i] = i;
                }
                int rnd = Random.Range(0, IndicesPool.Length);
                int idx = IndicesPool[rnd];
                var ret = array[idx];
                IndicesPool.Dispose();
                return ret;
            }
        }
        /// <summary>
        /// Get a random value from array, the result will exclude the value.
        /// If the array has only one element, then there is no choice - result will be the only element.
        /// </summary>
        public static T RandomExcept<T>(this List<T> list, T ExcludedValue)
        {
            if (list.Count == 1)
            {
                return list[0];
            }
            else if (list.Count == 2)
            {
                return list[0].Equals(ExcludedValue) ? list[1] : list[0];
            }
            else
            {
                NativeArray<int> IndicesPool = new NativeArray<int>(list.Count - 1, Allocator.Temp, NativeArrayOptions.ClearMemory);
                for (int i = 0, iMax = list.Count - 1; i < iMax; i++)
                {
                    if (list[i].Equals(ExcludedValue))
                    {
                        continue;
                    }
                    IndicesPool[i] = i;
                }
                int rnd = Random.Range(0, IndicesPool.Length);
                int idx = IndicesPool[rnd];
                var ret = list[idx];
                IndicesPool.Dispose();
                return ret;
            }
        }

        /// <summary>
        /// Clone an array, exclude the "except"
        /// </summary>
        /// <param name="array"></param>
        /// <param name="except"></param>
        /// <returns></returns>
        public static T[] CloneExcept<T>(T[] array, T except)
        {
            List<T> ret = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(except) == false)
                {
                    ret.Add(array[i]);
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// 把 array 克隆一份同样size的数组.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] CloneArray<T>(T[] array)
        {
            T[] newArray = new T[array.Length];
            Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }

        /// <summary>
        /// 把 array 克隆一份同样size的数组, 以 offset 为起点, 长度为length
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] CloneArray<T>(T[] array, int offset, int size)
        {
            T[] newArray = new T[size];
            Array.Copy(array, offset, newArray, 0, size);
            return newArray;
        }

        ///// <summary>
        ///// 在 XZ Surface 水平平面上随机获取一个 Nav Point。
        ///// 随机失败前提：
        ///// - 距离太近
        ///// - 得不到 navmesh 点。
        ///// </summary>
        ///// <returns>The roaming point on XZ surface.</returns>
        ///// <param name="Radius">Radius.</param>
        ///// <param name="CenterPosition">Center position.</param>
        ///// /// <param name="MaxTry">Max try count.</param>
        //public static Vector3? FindRoamingPointOnXZSurface(this NavMeshAgent nAgent,
        //    float MinRadius,
        //    float MaxRadius,
        //    Vector3 CenterPosition,
        //    int MaxTry = 1, float minDropSqrDistance = 1)
        //{
        //    Vector3? retRandomPoint = default(Vector3?);

        //    for (int i = 0; i < MaxTry; i++)
        //    {
        //        float radius = Mathf.Lerp(MinRadius, MaxRadius, Random.Range(0, 1));
        //        Vector2 rndVect2 = Random.insideUnitCircle * radius;
        //        Vector3 rndVect3 = new Vector3(rndVect2.x, 0, rndVect2.y);
        //        Vector3 rndPoint = CenterPosition + rndVect3;
        //        UnityEngine.AI.NavMeshHit navMeshHit = default(UnityEngine.AI.NavMeshHit);
        //        bool positionGet = UnityEngine.AI.NavMesh.SamplePosition(rndPoint, out navMeshHit, 1, -1);
        //        if (positionGet)
        //        {
        //            Vector3 samplePos = navMeshHit.position;
        //            //避免随机到隔断区域点
        //            if (nAgent.Raycast(samplePos, out navMeshHit))
        //            {
        //                samplePos = navMeshHit.position;
        //            }
        //            //检查距离:
        //            var sqrDistance = Vector3.SqrMagnitude(samplePos - nAgent.transform.position);
        //            //距离太近，丢弃
        //            if (sqrDistance <= minDropSqrDistance)
        //            {
        //                retRandomPoint = default(Vector3?);
        //            }
        //            else
        //            {
        //                retRandomPoint = samplePos;
        //            }
        //        }
        //        else
        //        {
        //            retRandomPoint = default(Vector3?);
        //        }

        //        if (retRandomPoint.HasValue)
        //        {
        //            return retRandomPoint;
        //        }
        //    }

        //    return retRandomPoint;
        //}

        /// <summary>
        /// Finds the closest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static Transform FindClosest(IEnumerable<Component> transforms, Vector3 center, out float distance)
        {
            Transform closest = null;
            float closestSqrLength = 0;
            foreach (var transform in transforms)
            {
                if (!closest)
                {
                    closest = transform.transform;
                    closestSqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                    if (sqrLength < closestSqrLength)
                    {
                        closestSqrLength = sqrLength;
                        closest = transform.transform;
                    }
                }
            }
            distance = Mathf.Sqrt(closestSqrLength);
            return closest;
        }


        /// <summary>
        /// Finds the closest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static Transform FindClosest<T>(List<T> transforms, Vector3 center, out float distance) where T : Component
        {
            Transform closest = null;
            float closestSqrLength = 0;
            for (int i = 0, transformsCount = transforms.Count; i < transformsCount; i++)
            {
                var transform = transforms[i];
                if (!closest)
                {
                    closest = transform.transform;
                    closestSqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                    if (sqrLength < closestSqrLength)
                    {
                        closestSqrLength = sqrLength;
                        closest = transform.transform;
                    }
                }
            }

            distance = Mathf.Sqrt(closestSqrLength);
            return closest;
        }


        /// <summary>
        /// Finds the closest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static Transform FindClosestGameObject(List<GameObject> transforms, Vector3 center, out float distance)
        {
            Transform closest = null;
            float closestSqrLength = 0;
            for (int i = 0, transformsCount = transforms.Count; i < transformsCount; i++)
            {
                var transform = transforms[i];
                if (!closest)
                {
                    closest = transform.transform;
                    closestSqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                    if (sqrLength < closestSqrLength)
                    {
                        closestSqrLength = sqrLength;
                        closest = transform.transform;
                    }
                }
            }

            distance = Mathf.Sqrt(closestSqrLength);
            return closest;
        }


        /// <summary>
        /// Finds the closest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static GameObject FindClosest(IEnumerable<GameObject> transforms, Vector3 center, out float distance)
        {
            Transform closest = null;
            float closestSqrLength = 0;
            foreach (var transform in transforms)
            {
                if (!closest)
                {
                    closest = transform.transform;
                    closestSqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                    if (sqrLength < closestSqrLength)
                    {
                        closestSqrLength = sqrLength;
                        closest = transform.transform;
                    }
                }
            }
            if (!closest)
            {
                distance = -1;
                return null;
            }
            else
            {
                distance = Mathf.Sqrt(closestSqrLength);
                return closest.gameObject;
            }
        }

        /// <summary>
        /// Finds the farest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static Transform FindFarest(IEnumerable<Transform> transforms, Vector3 center, out float distance)
        {
            Transform farest = null;
            float farestSqrLength = 0;
            foreach (var transform in transforms)
            {
                if (!farest)
                {
                    farest = transform;
                    farestSqrLength = Vector3.SqrMagnitude(transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.position - center);
                    if (sqrLength > farestSqrLength)
                    {
                        farestSqrLength = sqrLength;
                        farest = transform;
                    }
                }
            }
            distance = Mathf.Sqrt(farestSqrLength);
            return farest;
        }

        /// <summary>
        /// Finds the farest transform to the center point.
        /// </summary>
        /// <param name="transforms">Transforms.</param>
        /// <param name="center">Center.</param>
        public static GameObject FindFarest(IEnumerable<GameObject> transforms, Vector3 center, out float distance)
        {
            GameObject farest = null;
            float farestSqrLength = 0;
            foreach (var transform in transforms)
            {
                if (!farest)
                {
                    farest = transform;
                    farestSqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(transform.transform.position - center);
                    if (sqrLength > farestSqrLength)
                    {
                        farestSqrLength = sqrLength;
                        farest = transform;
                    }
                }
            }
            distance = Mathf.Sqrt(farestSqrLength);
            return farest;
        }

        /// <summary>
        /// 查找名为name 并且tag 相符的gameobject。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject FindGameObjectOfTag(string name, string tag)
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject g = gameObjects[i];
                if (g.name.Equals(name))
                {
                    return g;
                }
            }
            return default(GameObject);
        }

        /// <summary>
        /// Find object of type, if none, add the component to gameObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T FindOrAddObjectOfType<T>(GameObject gameObject) where T : Component
        {
            T comp = Object.FindObjectOfType<T>();
            if (!comp)
            {
                return gameObject.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// Find object of type, if none, create a new GameObject of the Name and add the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newCreatedGameObjectName">If component not found, the new created gameObject's name</param>
        /// <returns></returns>
        public static T FindOrAddObjectOfType<T>(string newCreatedGameObjectName = "") where T : Component
        {
            T comp = Object.FindObjectOfType<T>();
            if (!comp)
            {
                return new GameObject(newCreatedGameObjectName).AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// Finds the child by given path.
        /// This method supports recursive path .
        /// </summary>
        /// <returns>The child path.</returns>
        /// <param name="parent">Parent.</param>
        /// <param name="Path">Path. For example: "root/skin/weapon"</param>
        public static Transform FindChildByPath(this Transform parent, string Path, bool IgnoreCase = true)
        {
            var subpath = Path.Split(new char[] { '/' });
            if (subpath.Length > 0 && string.Compare(parent.name, subpath[0], IgnoreCase) == 0)
            {
                if (subpath.Length == 1)
                {
                    return parent;
                }
                else
                {
                    string[] newPath = new string[subpath.Length - 1];
                    System.Array.Copy(subpath, 1, newPath, 0, subpath.Length - 1);
                    string newFilter = string.Empty;
                    for (int i = 0; i < newPath.Length; i++)
                    {
                        newFilter += newPath[i];
                        if (i != (newPath.Length - 1))
                        {
                            newFilter += '/';
                        }
                    }

                    for (int i = 0; i < parent.childCount; i++)
                    {
                        var found = FindChildByPath(parent.GetChild(i), newFilter);
                        if (found != null)
                        {
                            return found;
                        }
                    }

                    return null;
                }
            }
            else
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    var found = FindChildByPath(parent.GetChild(i), Path);
                    if (found != null)
                    {
                        return found;
                    }
                }

                return null;
            }

        }

        /// <summary>
        /// Finds the closest transform to the center point.
        /// </summary>
        public static Vector3 FindClosest(IEnumerable<Vector3> points, Vector3 center, out float distance)
        {
            Vector3? closest = default(Vector3?);
            float closestSqrLength = 0;
            foreach (var point in points)
            {
                if (closest == null)
                {
                    closest = point;
                    closestSqrLength = Vector3.SqrMagnitude(point - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(point - center);
                    if (sqrLength < closestSqrLength)
                    {
                        closestSqrLength = sqrLength;
                        closest = point;
                    }
                }
            }
            distance = Mathf.Sqrt(closestSqrLength);
            return closest.Value;
        }

        /// <summary>
        /// Finds the farest transform to the center point.
        /// </summary>
        public static Vector3 FindFarest(IEnumerable<Vector3> points, Vector3 center, out float distance)
        {
            Vector3? farest = default(Vector3?);
            float farestSqrLength = 0;
            foreach (var point in points)
            {
                if (farest == null)
                {
                    farest = point;
                    farestSqrLength = Vector3.SqrMagnitude(point - center);
                }
                else
                {
                    float sqrLength = Vector3.SqrMagnitude(point - center);
                    if (sqrLength > farestSqrLength)
                    {
                        farestSqrLength = sqrLength;
                        farest = point;
                    }
                }
            }
            distance = Mathf.Sqrt(farestSqrLength);
            return farest.Value;
        }

        /// <summary>
        /// Convert world direction to xz surface direction.
        /// </summary>
        /// <returns>The XZ direction.</returns>
        /// <param name="direction">Direction.</param>
        /// <param name="normalized">If set to <c>true</c> normalized.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXZDirection(this Vector3 direction, bool normalized = true)
        {
            Vector3 dir2 = new Vector3(direction.x, 0, direction.z);
            if (normalized)
                return dir2.normalized;

            return dir2;
        }

        /// <summary>
        /// Convert quaternion to vector3 direction at XZ surface
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXZDirection(this Quaternion rotation)
        {
            Vector3 dir = rotation * Vector3.forward;
            Vector3 dir2 = new Vector3(dir.x, 0, dir.z);
            return dir2.normalized;
        }

        /// <summary>
        /// Convert world direction to xy surface direction.
        /// </summary>
        /// <returns>The XY direction.</returns>
        /// <param name="direction">Direction.</param>
        /// <param name="normalized">If set to <c>true</c> normalized.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXYDirection(this Vector3 direction, bool normalized = true)
        {
            Vector3 dir2 = new Vector3(direction.x, direction.y, 0);
            if (normalized)
                return dir2.normalized;
            return dir2;
        }

        /// <summary>
        /// Convert quaternion to vector3 direction at XY surface
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXYDirection(this Quaternion rotation)
        {
            Vector3 dir = rotation * Vector3.forward;
            Vector3 dir2 = new Vector3(dir.x, dir.y, 0);
            return dir2.normalized;
        }

        /// <summary>
        /// Convert world direction to yz surface direction.
        /// </summary>
        /// <returns>The YZ direction.</returns>
        /// <param name="direction">Direction.</param>
        /// <param name="normalized">If set to <c>true</c> normalized.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYZDirection(this Vector3 direction, bool normalized = true)
        {
            Vector3 dir2 = new Vector3(0, direction.y, direction.z);
            if (normalized)
                return dir2.normalized;
            return dir2;
        }

        /// <summary>
        /// Convert quaternion to vector3 direction at YZ surface
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToYZDirection(this Quaternion rotation)
        {
            Vector3 dir = rotation * Vector3.forward;
            Vector3 dir2 = new Vector3(0, dir.y, dir.z);
            return dir2.normalized;
        }

        /// <summary>
        /// Converts XZ vector to XYZ. (Y is pass as parameter, default = 0)
        /// </summary>
        /// <returns>The to XY.</returns>
        /// <param name="xzVector">Xz vector.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToXYZ(this Vector2 xzVector, float Y = 0)
        {
            return new Vector3(xzVector.x, Y, xzVector.y);
        }

        /// <summary>
        /// 把3D坐标转换为2D (XZ)坐标。
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="pos3D">Pos3 d.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToXZ(this Vector3 pos3D)
        {
            return new Vector2(pos3D.x, pos3D.z);
        }


        /// <summary>
        /// 把3D坐标转换为2D (ZY)坐标。
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="pos3D">Pos3 d.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToZY(this Vector3 pos3D)
        {
            return new Vector2(pos3D.z, pos3D.y);
        }

        /// <summary>
        /// 把3D坐标转换为2D (ZY)方向。
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="pos3D">Pos3 d.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToZYDirection(this Vector3 pos3D)
        {
            return new Vector2(pos3D.z, pos3D.y).normalized;
        }

        /// <summary>
        /// Make the direction drift by given angle of pitch, yaw, roll.
        /// Internally this method equals to :
        /// return Quaternion.Euler(Pitch, Yaw, Roll) * direction;
        /// </summary>
        /// <param name="Pitch">Pitch 仰角, 正数为向下， 正数为向下</param>
        /// <param name="Yaw">Yaw 水平旋转角, 正数向右， 负数向左.</param>
        /// <param name="Roll">前向旋角，正数逆时针， 负数顺时针</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DriftDirection(this Vector3 direction, float Pitch, float Yaw, float Roll)
        {
            return (Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(Pitch, Yaw, Roll)) * Vector3.forward;
        }

        /// <summary>
        /// Add offset to vector3Int
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int Add(this Vector3Int vector3, int x, int y, int z)
        {
            return vector3 + new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Add offset to vector2Int
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Int Add(this Vector2Int vector2, int x, int y)
        {
            return vector2 + new Vector2Int(x, y);
        }

        /// <summary>
        /// Add offset to vector3
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Add(this Vector3 vector3, float x, float y, float z)
        {
            return vector3 + new Vector3(x, y, z);
        }

        /// <summary>
        /// Set X of the vector2
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetX(this Vector2 vector2, float x)
        {
            return new Vector2(x, vector2.y);
        }

        /// <summary>
        /// Set Y of the vector2
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetY(this Vector2 vector2, float y)
        {
            return new Vector2(vector2.x, y);
        }

        /// <summary>
        /// Set X of the vector3
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 vector3, float x)
        {
            return new Vector3(x, vector3.y, vector3.z);
        }

        /// <summary>
        /// Set Y of the vector3
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 vector3, float y)
        {
            return new Vector3(vector3.x, y, vector3.z);
        }

        /// <summary>
        /// Set Z of the vector3
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 vector3, float z)
        {
            return new Vector3(vector3.x, vector3.y, z);
        }

        /// <summary>
        /// Sets alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color SetAlpha(this Color color, float a)
        {
            color.a = a;
            return color;
        }

        /// <summary>
        /// 获取color 的 r 和 g 的相对权重。
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        public static void GetColorWeightedValue(this Color color, out float r, out float g)
        {
            float total = color.r + color.g;
            r = color.r / total;
            g = color.g / total;
        }

        /// <summary>
        /// 获取color 的 r 和 g 和 b 的相对权重。
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void GetColorWeightedValue(this Color color, out float r, out float g, out float b)
        {
            float total = color.r + color.g + color.b;
            r = color.r / total;
            g = color.g / total;
            b = color.b / total;
        }
        /// <summary>
        /// 获取color 的 r 和 g 和 b 和 a 的相对权重。
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void GetColorWeightedValue(this Color color, out float r, out float g, out float b, out float a)
        {
            float total = color.r + color.g + color.b + color.a;
            r = color.r / total;
            g = color.g / total;
            b = color.b / total;
            a = color.a / total;
        }

        /// <summary>
        /// Sets alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 SetAlpha(this Color32 color, byte a)
        {
            color.a = a;
            return color;
        }

        /// <summary>
        /// Array as an one-dimension array, and counts as a two-dimension array in column = [columnCount].
        /// This function sets the element's value at the row index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">一维数组</param>
        /// <param name="value">数值</param>
        /// <param name="row">行数</param>
        /// <param name="columnCount">二维数组的列数</param>
        public static void SetValueAtRow<T>(this T[] array, T value, int rowIndex, int columnCount)
        {
            int aLength = array.Length;
            int rowCount = Mathf.FloorToInt((float)aLength / (float)columnCount) + 1;
            int start = rowIndex * columnCount;
            if (start >= aLength)
            {
                Debug.LogErrorFormat("Invalid parameter : rowIndex = {0}, array length = {1}, column count = {2}, row count = {3}", rowIndex, aLength, columnCount, rowCount);
                return;
            }
            int end = Mathf.Min(aLength, start + columnCount);
            for (int i = start; i < end; i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Array as an one-dimension array, and counts as a two-dimension array in column = [columnCount].
        /// This function sets the element's value at the column index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">一维数组</param>
        /// <param name="value">数值</param>
        /// <param name="row">行数</param>
        /// <param name="columnCount">二维数组的列数</param>
        public static void SetValueAtColumn<T>(this T[] array, T value, int columnIndex, int columnCount)
        {
            if (columnIndex >= columnCount)
            {
                Debug.LogErrorFormat("Invalid parameter : columnIndex = {0}, column count = {1}", columnIndex, columnCount);
                return;
            }
            int aLength = array.Length;
            int rowCount = Mathf.FloorToInt((float)aLength / (float)columnCount) + 1;
            for (int r = 0; r < rowCount; r++)
            {
                int index = columnIndex + r * columnCount;
                if (index < aLength)
                {
                    array[index] = value;
                }
            }
        }

        /// <summary>
        /// Add offset to vector2
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Add(this Vector2 vector2, float x, float y)
        {
            return vector2 + new Vector2(x, y);
        }
        /// <summary>
        /// Add offset to vector4
        /// </summary>
        /// <returns>The X.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Add(this Vector4 vector4, float x, float y, float z, float w)
        {
            return vector4 + new Vector4(x, y, z, w);
        }
        /// <summary>
        /// 把3D坐标转换为2D (XY)坐标。
        /// </summary>
        /// <returns>The X.</returns>
        /// <param name="pos3D">Pos3 d.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToXY(this Vector3 pos3D)
        {
            return new Vector2(pos3D.x, pos3D.y);
        }

        /// <summary>
        /// Equals to Quaternion.LookRotation(direction, Vector3.up);
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToRotation(this Vector3 direction)
        {
            return Quaternion.LookRotation(direction, Vector3.up);
        }

        /// <summary>
        /// Equals to Quaternion.LookRotation(direction, up);
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ToRotation(this Vector3 direction, Vector3 up)
        {
            return Quaternion.LookRotation(direction, up);
        }

        /// <summary>
        /// 从 list 列表中，随机抽取 count 个元素组成一个新的列表返回.
        /// </summary>
        /// <returns>The pick.</returns>
        /// <param name="list">List.</param>
        /// <param name="count">Count.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static List<T> GetRandom<T>(List<T> list, int count)
        {
            List<T> ret = new List<T>();
            ret.AddRange(list);
            int removeCount = ret.Count - count;
            for (int i = 0; i < removeCount; i++)
            {
                int idx = Random.Range(0, ret.Count);
                ret.RemoveAt(idx);
            }
            return ret;
        }


        /// <summary>
        /// 从 list 列表中，随机抽取 count 个元素组成一个新的列表返回.
        /// </summary>
        /// <returns>The pick.</returns>
        /// <param name="list">List.</param>
        /// <param name="count">Count.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static List<T> GetRandom<T>(List<T> list, int count, ref List<T> ret)
        {
            ret.Clear();
            ret.AddRange(list);
            int removeCount = ret.Count - count;
            for (int i = 0; i < removeCount; i++)
            {
                int idx = Random.Range(0, ret.Count);
                ret.RemoveAt(idx);
            }
            return ret;
        }



        /// <summary>
        /// Convert text to hash number. guaranteed to be unique each runtime.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static uint GetPersisentHash(this string strText)
        {
            if (string.IsNullOrEmpty(strText))
            {
                return 0;
            }

            //Unicode Encode Covering all characterset
            byte[] byteContents = Encoding.Unicode.GetBytes(strText);
            byte[] hashText = kHash.ComputeHash(byteContents);
            uint hashCodeStart = BitConverter.ToUInt32(hashText, 0);
            uint hashCodeMedium = BitConverter.ToUInt32(hashText, 8);
            uint hashCodeEnd = BitConverter.ToUInt32(hashText, 16);
            var hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            return uint.MaxValue - hashCode;

        }

        /// <summary>
        /// 给出一个 Plane，获取以 Position, Normal 定义的Plane， 半径为radius 的随机点
        /// </summary>
        /// <returns>The point on plane.</returns>
        /// <param name="position">Position.</param>
        /// <param name="normal">Normal.</param>
        /// <param name="radius">Radius.</param>
        public static Vector3 RandomPointOnPlane(Vector3 position, Vector3 normal, float radius)
        {
            Vector3 randomPoint;

            do
            {
                randomPoint = Vector3.Cross(Random.insideUnitSphere, normal);
            } while (randomPoint == Vector3.zero);

            randomPoint.Normalize();
            randomPoint *= radius;
            randomPoint += position;

            return randomPoint;
        }

        /// <summary>
        /// 给出 RootTransform 和 其下的一个 childTransform, 放置rootTransform, 得出的结果是 childTransform.rotation 和 TargetRotation 对齐
        /// 此方法只会旋转，不会设置位置。
        /// </summary>
        /// <param name="rootTransform"></param>
        /// <param name="childTransform"></param>
        /// <param name="TargetRotation"></param>
        public static void AlignByChildRotation(Transform rootTransform, Transform childTransform, Quaternion TargetRotation)
        {
            if (childTransform == rootTransform)
            {
                rootTransform.rotation = TargetRotation;
                return;
            }
            else if (!childTransform.IsChildOf(rootTransform))
            {
                Debug.LogErrorFormat("AlignByChildRotation : {0} is not child of : {1}", childTransform.name,
                    rootTransform.name);
                return;
            }
            else
            {
                //以childTransform为基础，向上遍历父节点直到设置了 rootTranform的 rotation
                Transform pTransform = childTransform.parent;
                Transform cTransform = childTransform;
                Quaternion pRotation;
                Quaternion tRotaton = TargetRotation;
                while (true)
                {
                    pRotation = tRotaton * Quaternion.Inverse(cTransform.localRotation);
                    // pTransform.rotation = pRotation;
                    if (pTransform == rootTransform) //如果到达 rootTranform, break loop
                    {
                        pTransform.rotation = pRotation;
                        break;
                    }
                    else //否则继续向上遍历
                    {
                        cTransform = pTransform;
                        pTransform = cTransform.parent;
                        tRotaton = pRotation;
                    }
                }
            }
        }


        /// <summary>
        /// 给出 RootTransform 和 其下的一个 childTransform, 放置rootTransform, 得出的结果是 childTransform.position 和 TargetPosition 对齐
        /// 此方法只会设置位置。
        /// </summary>
        /// <param name="rootTransform"></param>
        /// <param name="childTransform"></param>
        /// <param name="TargetPos"></param>
        public static void AlignByChildPosition(Transform rootTransform, Transform childTransform, Vector3 TargetPos)
        {
            if (childTransform == rootTransform)
            {
                rootTransform.position = TargetPos;
                return;
            }
            else if (!childTransform.IsChildOf(rootTransform))
            {
                Debug.LogErrorFormat("AlignByChildPosition : {0} is not child of : {1}", childTransform.name,
                    rootTransform.name);
                return;
            }
            else
            {
                Vector3 distance = TargetPos - childTransform.position;
                rootTransform.position += distance;
            }
        }

        /// <summary>
        /// 给出 RootTransform 和 其下的一个 childTransform, 放置rootTransform, 得出的结果是 childTransform 和 TargetMatrix 对齐
        /// </summary>
        public static void AlignByChildTransform(
            Transform rootTransform,
            Transform childTransform,
            Vector3 TargetPos,
            Quaternion TargetRotation)
        {
            if (childTransform == rootTransform)
            {
                rootTransform.rotation = TargetRotation;
                rootTransform.position = TargetPos;
                return;
            }
            else if (!childTransform.IsChildOf(rootTransform))
            {
                Debug.LogErrorFormat("AlignByChildTransform : {0} is not child of : {1}", childTransform.name,
                    rootTransform.name);
                return;
            }
            else
            {
                //以childTransform为基础，向上遍历父节点直到设置了 rootTranform的 rotation
                Transform pTransform = childTransform.parent;
                Transform cTransform = childTransform;
                Quaternion pRotation;
                Quaternion tRotaton = TargetRotation;
                while (true)
                {
                    pRotation = tRotaton * Quaternion.Inverse(cTransform.localRotation);
                    // pTransform.rotation = pRotation;
                    if (pTransform == rootTransform) //如果到达 rootTranform, break loop
                    {
                        pTransform.rotation = pRotation;
                        break;
                    }
                    else //否则继续向上遍历
                    {
                        cTransform = pTransform;
                        pTransform = cTransform.parent;
                        tRotaton = pRotation;
                    }
                }
                //                Vector3 offset = childTransform.position - rootTransform.position;
                Vector3 distance = TargetPos - childTransform.position;
                rootTransform.position += distance;
            }
        }


        /// <summary>
        /// 给出 RootTransform 和 其下的一个 childTransform, 放置rootTransform,
        /// 得出的结果是 childTransform 和 TargetMatrix 对齐。
        /// 此方法输出 新的RootPosition，和新的RootRotation。
        /// </summary>
        public static void AlignByChildTransform(
            Transform rootTransform,
            Transform childTransform,
            Vector3 TargetPos,
            Quaternion TargetRotation, out Vector3 NewRootPosition, out Quaternion NewRootRotation)
        {
            Vector3 originalPosition = rootTransform.position;
            Quaternion originalRotation = rootTransform.rotation;
            NewRootPosition = Vector3.zero;
            NewRootRotation = Quaternion.identity;
            if (childTransform == rootTransform)
            {
                NewRootRotation = TargetRotation;
                NewRootPosition = TargetPos;
                return;
            }
            else if (!childTransform.IsChildOf(rootTransform))
            {
                Debug.LogErrorFormat("AlignByChildTransform : {0} is not child of : {1}", childTransform.name,
                    rootTransform.name);
                return;
            }
            else
            {
                //以childTransform为基础，向上遍历父节点直到设置了 rootTranform的 rotation
                Transform pTransform = childTransform.parent;
                Transform cTransform = childTransform;
                Quaternion pRotation;
                Quaternion tRotaton = TargetRotation;
                while (true)
                {
                    pRotation = tRotaton * Quaternion.Inverse(cTransform.localRotation);
                    // pTransform.rotation = pRotation;
                    if (pTransform == rootTransform) //如果到达 rootTranform, break loop
                    {
                        pTransform.rotation = pRotation;
                        break;
                    }
                    else //否则继续向上遍历
                    {
                        cTransform = pTransform;
                        pTransform = cTransform.parent;
                        tRotaton = pRotation;
                    }
                }
                //                Vector3 offset = childTransform.position - rootTransform.position;
                Vector3 distance = TargetPos - childTransform.position;
                rootTransform.position += distance;

                //输出新的姿态:
                NewRootPosition = rootTransform.position;
                NewRootRotation = rootTransform.rotation;

                //root transform回归原始姿态:
                rootTransform.position = originalPosition;
                rootTransform.rotation = originalRotation;
            }
        }

        /// <summary>
        /// Aligns world position and world rotation of this transform to other transform.
        /// </summary>
        /// <param name="thisTransform">This transform.</param>
        /// <param name="otherTransform">Other transform.</param>
        public static void AlignPoseTo(this Transform thisTransform, Transform otherTransform)
        {
            thisTransform.SetPositionAndRotation(otherTransform.position, otherTransform.rotation);
        }

        /// <summary>
        /// Full aligns source transform to dest transform.
        /// Source transform's postiion/rotation/world scale will be same to dest transform
        /// </summary>
        /// <param name="sourceTransform"></param>
        /// <param name="destTransform"></param>
        public static void AlignTo(this Transform sourceTransform, Transform destTransform)
        {
            sourceTransform.rotation = destTransform.rotation;
            sourceTransform.position = destTransform.position;
            sourceTransform.SetGlobalScale(destTransform.lossyScale);
        }

        /// <summary>
        /// Aligns the world position to.
        /// </summary>
        /// <param name="thisTransform">This transform.</param>
        /// <param name="otherTransform">Other transform.</param>
        public static void AlignWorldPositionTo(this Transform thisTransform, Transform otherTransform)
        {
            thisTransform.position = otherTransform.position;
        }

        /// <summary>
        /// Aligns the world rotation to.
        /// </summary>
        /// <param name="thisTransform">This transform.</param>
        /// <param name="otherTransform">Other transform.</param>
        public static void AlignWorldRotationTo(this Transform thisTransform, Transform otherTransform)
        {
            thisTransform.rotation = otherTransform.rotation;
        }

        /// <summary>
        /// Tests the point is contained inside the collider.
        /// </summary>
        /// <returns><c>true</c>, if is contained was tested, <c>false</c> otherwise.</returns>
        /// <param name="collider">Collider.</param>
        /// <param name="Position">Position.</param>
        public static bool TestContain(Collider collider, Vector3 Position)
        {
            if (!collider)
            {
                Debug.LogWarning("Collider is null");
                return true;
            }

            Vector3 posToCheck = Position;
            Vector3 offset = collider.bounds.center - Position;
            Ray inputRay = new Ray(posToCheck, offset.normalized);
            RaycastHit rHit;
            bool inside;
            if (!collider.Raycast(inputRay, out rHit, offset.magnitude * 1.1f))
                inside = true;
            else
                inside = false;

            return inside;
        }

        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteFloat(float floatValue, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            floatConverter.floatValue = floatValue;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            return startIndex + 4;
        }

        /// <summary>
        /// Reads the float from buffer at start index, length = 4.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static float ReadFloat(byte[] buffer, int startIndex = 0)
        {
            uint value = 0;
            value |= buffer[startIndex++];
            value |= (uint)(buffer[startIndex++] << 8);
            value |= (uint)(buffer[startIndex++] << 16);
            value |= (uint)(buffer[startIndex++] << 24);
            floatConverter.uintValue = value;
            return floatConverter.floatValue;
        }

        /// <summary>
        /// Compares two array T1 and T2, starts from index, at length.
        /// </summary>
        /// <returns><c>true</c>, if array was compared, <c>false</c> otherwise.</returns>
        /// <param name="array01">Array01.</param>
        /// <param name="startIndex01">Start index01.</param>
        /// <param name="array02">Array02.</param>
        /// <param name="startIndex02">Start index02.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool CompareArray<T>(T[] array01, int startIndex01, T[] array02, int startIndex02, int length)
        {
            if (array01 == null || array02 == null)
            {
                return false;
            }
            if ((startIndex01 + length) > array01.Length)
            {
                return false;
            }
            if ((startIndex02 + length) > array02.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (array01[startIndex01 + i].Equals(array02[startIndex02 + i]) == false)
                {
                    return false;
                }
            }
            return true;
        }



        /// <summary>
        /// Compares two array T1 and T2, starts from index, at length.
        /// </summary>
        /// <returns><c>true</c>, if array was compared, <c>false</c> otherwise.</returns>
        /// <param name="array01">Array01.</param>
        /// <param name="startIndex01">Start index01.</param>
        /// <param name="list02">List 02.</param>
        /// <param name="startIndex02">Start index02.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool CompareArray<T>(T[] array01, int startIndex01, List<T> list02, int startIndex02, int length)
        {
            if (array01 == null || list02 == null)
            {
                return false;
            }
            if ((startIndex01 + length) > array01.Length)
            {
                return false;
            }
            if ((startIndex02 + length) > list02.Count)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (array01[startIndex01 + i].Equals(list02[startIndex02 + i]) == false)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Compares two array T1 and T2, starts from index, at length.
        /// </summary>
        /// <returns><c>true</c>, if array was compared, <c>false</c> otherwise.</returns>
        /// <param name="list01">List 01.</param>
        /// <param name="startIndex01">Start index01.</param>
        /// <param name="list02">List 02.</param>
        /// <param name="startIndex02">Start index02.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static bool CompareArray<T>(List<T> list01, int startIndex01, List<T> list02, int startIndex02, int length)
        {
            if (list01 == null || list02 == null)
            {
                return false;
            }
            if ((startIndex01 + length) > list01.Count)
            {
                return false;
            }
            if ((startIndex02 + length) > list02.Count)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (list01[startIndex01 + i].Equals(list02[startIndex02 + i]) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Writes the vector2 to the buffer from start index, length = 8.
        /// </summary>
        /// <param name="vect2">Vector2 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector2(Vector2 vect2, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            floatConverter.floatValue = vect2.x;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect2.y;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            return startIndex + 8;
        }

        /// <summary>
        /// Reads the vector2 from buffer at start index, length = 8.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector2 ReadVector2(byte[] buffer, int startIndex = 0)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex + 4);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Writes the vector3 to the buffer from start index, length = 12.
        /// </summary>
        /// <param name="vect3">Vector3 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector3(Vector3 vect3, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            floatConverter.floatValue = vect3.x;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect3.y;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect3.z;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            return startIndex + 12;
        }


        /// <summary>
        /// Writes the vector2int to the buffer from start index, length = 12.
        /// </summary>
        /// <param name="vect2Int">Vector3 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector2Int(Vector2Int vect2Int, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            WriteInt(vect2Int.x, buffer, from);
            WriteInt(vect2Int.y, buffer, from + 4);
            return startIndex + 8;
        }

        /// <summary>
        /// Writes the vector3int to the buffer from start index, length = 12.
        /// </summary>
        /// <param name="vect3Int">Vector3 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector3Int(Vector3Int vect3Int, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            WriteInt(vect3Int.x, buffer, from);
            WriteInt(vect3Int.y, buffer, from + 4);
            WriteInt(vect3Int.z, buffer, from + 8);
            return startIndex + 12;
        }

        /// <summary>
        /// Reads the vector3 from buffer at start index, length = 12
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector3 ReadVector3(byte[] buffer, int startIndex = 0)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex + 4);
            float z = ReadFloat(buffer, startIndex + 8);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads the vector3int from buffer at start index, length = 12
        /// </summary>
        /// <returns>The vector3int.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector3Int ReadVector3Int(byte[] buffer, int startIndex = 0)
        {
            int x = ReadInt(buffer, startIndex);
            int y = ReadInt(buffer, startIndex + 4);
            int z = ReadInt(buffer, startIndex + 8);
            return new Vector3Int(x, y, z);
        }
        /// <summary>
        /// Reads the vector2int from buffer at start index, length = 12
        /// </summary>
        /// <returns>The vector2 int.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector2Int ReadVector2Int(byte[] buffer, int startIndex = 0)
        {
            int x = ReadInt(buffer, startIndex);
            int y = ReadInt(buffer, startIndex + 4);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Writes the vector4 to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="vect4">Vector4 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector4(Vector4 vect4, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            floatConverter.floatValue = vect4.x;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect4.y;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect4.z;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = vect4.w;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            return startIndex + 16;
        }


        /// <summary>
        /// Writes the quaternion to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="q">Quaternion value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteQuaternion(Quaternion q, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            floatConverter.floatValue = q.x;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = q.y;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = q.z;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            floatConverter.floatValue = q.w;
            buffer[from++] = (byte)(floatConverter.uintValue & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 8 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 16 & 0xff);
            buffer[from++] = (byte)(floatConverter.uintValue >> 24 & 0xff);

            return startIndex + 16;
        }

        /// <summary>
        /// Reads the vector4 from buffer at start index, length = 16
        /// </summary>
        /// <returns>The vector4.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector4 ReadVector4(byte[] buffer, int startIndex = 0)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex + 4);
            float z = ReadFloat(buffer, startIndex + 8);
            float w = ReadFloat(buffer, startIndex + 12);
            return new Vector4(x, y, z, w);
        }


        /// <summary>
        /// Reads the quaternion from buffer at start index, length = 16
        /// </summary>
        /// <returns>The vector4.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Quaternion ReadQuaternion(byte[] buffer, int startIndex = 0)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex + 4);
            float z = ReadFloat(buffer, startIndex + 8);
            float w = ReadFloat(buffer, startIndex + 12);
            return new Quaternion(x, y, z, w);
        }


        /// <summary>
        /// Writes the int to the buffer from start index, length = 4.
        /// Return interger indicates the returned buffer index = startIndex + 4
        /// </summary>
        /// <param name="intValue">Int value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteInt(int intValue, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            buffer[from++] = (byte)(intValue & 0xff);
            buffer[from++] = (byte)(intValue >> 8 & 0xff);
            buffer[from++] = (byte)(intValue >> 16 & 0xff);
            buffer[from++] = (byte)(intValue >> 24 & 0xff);

            return startIndex + 4;
        }

        /// <summary>
        /// Write byte into buffer
        /// </summary>
        /// <param name="byteValue"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteByte(byte byteValue, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            buffer[from++] = byteValue;
            return startIndex + 1;
        }

        /// <summary>
        /// Write byte array into another byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static int WriteBytes(byte[] source, int sourceOffset, int length, byte[] destBuffer, int destStartIndex = 0)
        {
            int index = destStartIndex;
            index = WriteInt(length, destBuffer, index);//write data length
            Array.ConstrainedCopy(source, sourceOffset, destBuffer, index, length);
            return index + length;
        }

        /// <summary>
        /// Writes the short to the buffer from start index, length = 2
        /// Return interger indicates the returned buffer index = startIndex + 2
        /// </summary>
        /// <param name="shortValue">Short value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteShort(short shortValue, byte[] buffer, int startIndex = 0)
        {
            int from = startIndex;
            buffer[from++] = (byte)(shortValue & 0xff);
            buffer[from++] = (byte)(shortValue >> 8 & 0xff);
            return startIndex + 2;
        }

        /// <summary>
        /// Read short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static short ReadShort(byte[] buffer, int startIndex = 0)
        {
            short value = 0;
            value |= (short)buffer[startIndex++];
            value |= (short)(buffer[startIndex++] << 8);
            return value;
        }


        /// <summary>
        /// Read short
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static short ReadShort(List<byte> buffer, int startIndex = 0)
        {
            short value = 0;
            value |= (short)buffer[startIndex++];
            value |= (short)(buffer[startIndex++] << 8);
            return value;
        }


        /// <summary>
        /// Reads the int from buffer at start index, length = 4
        /// </summary>
        public static int ReadInt(byte[] buffer, int startIndex = 0)
        {
            uint value = 0;
            value |= buffer[startIndex++];
            value |= (uint)(buffer[startIndex++] << 8);
            value |= (uint)(buffer[startIndex++] << 16);
            value |= (uint)(buffer[startIndex++] << 24);
            return (int)value;
        }

        /// <summary>
        /// Reads a ulong from buffer.
        /// </summary>
        /// <returns>The long.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static ulong ReadUInt64(byte[] buffer, int startIndex = 0)
        {
            ulong value = 0;
            ulong other = buffer[startIndex++];
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 8;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 16;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 24;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 32;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 40;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 48;
            value |= other;

            other = ((ulong)buffer[startIndex++]) << 56;
            value |= other;

            return value;
        }

        /// <summary>
        /// 从 sourceBuffer 中读取一段指定长度的数组并填充到 destBuffer 中。
        /// </summary>
        /// <param name="sourceBuffer">原数组。</param>
        /// <param name="sourceIndex">原数组的数据起点</param>
        /// <param name="length">输出的目标数组的长度。</param>
        /// <param name="destBuffer">目标数组接收容器。</param>
        /// <param name="destBufferOffset">目标数组接收数据的起点索引。</param>
        public static void ReadByteArray(byte[] sourceBuffer, int sourceIndex, out int length, byte[] destBuffer, int destBufferOffset)
        {
            length = PEUtils.ReadInt(sourceBuffer, sourceIndex);
            Array.ConstrainedCopy(sourceBuffer, sourceIndex + 4, destBuffer, destBufferOffset, length);
        }

        /// <summary>
        /// 从 sourceBuffer 中读取一个数组。
        /// 此方法存在动态内存分配。
        /// </summary>
        /// <param name="sourceBuffer">原数组。</param>
        /// <param name="sourceIndex">原数组的数据起点</param>
        /// <param name="length">输出的目标数组的长度。</param>
        /// <param name="destBuffer">目标数组接收容器。</param>
        /// <param name="destBufferOffset">目标数组接收数据的起点索引。</param>
        public static byte[] ReadByteArray(List<byte> sourceBuffer, int sourceIndex)
        {
            int length = PEUtils.ReadInt(sourceBuffer, sourceIndex);
            byte[] ret = new byte[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = sourceBuffer[sourceIndex + 4 + i];
            }
            return ret;
        }

        /// <summary>
        /// 从 sourceBuffer 中读取一个数组。
        /// 此方法存在动态内存分配。
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceIndex"></param>
        /// <returns>The read byte[]</returns>
        public static byte[] ReadByteArray(byte[] sourceBuffer, int sourceIndex)
        {
            int length = PEUtils.ReadInt(sourceBuffer, sourceIndex);
            byte[] ret = new byte[length];
            sourceIndex += 4;
            Array.ConstrainedCopy(sourceBuffer, sourceIndex, ret, 0, length);
            return ret;
        }

        /// <summary>
        /// Reads the double from buffer at start index, length = 8
        /// </summary>
        /// <returns>The double.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static double ReadDouble(byte[] buffer, int startIndex = 0)
        {
            ulong value = ReadUInt64(buffer, startIndex);
            return floatConverter.ToDouble(value);
        }

        /// <summary>
        /// Write the specified ulong value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static int WriteULong(ulong value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex++] = (byte)(value & 0xff);
            buffer[startIndex++] = (byte)((value >> 8) & 0xff);
            buffer[startIndex++] = (byte)((value >> 16) & 0xff);
            buffer[startIndex++] = (byte)((value >> 24) & 0xff);
            buffer[startIndex++] = (byte)((value >> 32) & 0xff);
            buffer[startIndex++] = (byte)((value >> 40) & 0xff);
            buffer[startIndex++] = (byte)((value >> 48) & 0xff);
            buffer[startIndex++] = (byte)((value >> 56) & 0xff);

            return startIndex;
        }

        /// <summary>
        /// Write the specified decimal value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static int WriteDecimal(decimal value, byte[] buffer, int startIndex = 0)
        {
            decimalLongConverter._deciaml = value;
            PEUtils.WriteULong(decimalLongConverter.ulong01, buffer, startIndex);
            PEUtils.WriteULong(decimalLongConverter.ulong02, buffer, startIndex + 8);
            return 16;//decimal = 2 个 long
        }


        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteDouble(double value, byte[] buffer, int startIndex = 0)
        {
            floatConverter.doubleValue = value;
            WriteULong(floatConverter.ulongValue, buffer, startIndex);
            return startIndex + 8;
        }

        /// <summary>
        /// Writes a text into buffer, starts at startIndex.
        /// First 2 bytes = string length. (Assume string within 65535 length
        /// Encoded in UTF8.
        /// Return : end index.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteString(string text, byte[] buffer, int startIndex = 0)
        {
            int byteCount = Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, startIndex + 2);
            PEUtils.WriteShort((short)byteCount, buffer, startIndex);
            return startIndex + byteCount + 2;
        }

        /// <summary>
        /// Reads string from buffer, starts at startIndex.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string ReadString(byte[] buffer, out int endIndex, int startIndex = 0)
        {
            short length = PEUtils.ReadShort(buffer, startIndex);
            string text = Encoding.UTF8.GetString(buffer, startIndex + 2, (int)length);
            endIndex = startIndex + 2 + length;
            return text;
        }

        /// <summary>
        /// Writes the matrix 4x4.
        /// </summary>
        /// <param name="mtx">Value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteMatrix4x4(Matrix4x4 mtx, byte[] buffer, int startIndex = 0)
        {
            startIndex = WriteFloat(mtx.m00, buffer, startIndex);
            startIndex = WriteFloat(mtx.m01, buffer, startIndex);
            startIndex = WriteFloat(mtx.m02, buffer, startIndex);
            startIndex = WriteFloat(mtx.m03, buffer, startIndex);

            startIndex = WriteFloat(mtx.m10, buffer, startIndex);
            startIndex = WriteFloat(mtx.m11, buffer, startIndex);
            startIndex = WriteFloat(mtx.m12, buffer, startIndex);
            startIndex = WriteFloat(mtx.m13, buffer, startIndex);

            startIndex = WriteFloat(mtx.m20, buffer, startIndex);
            startIndex = WriteFloat(mtx.m21, buffer, startIndex);
            startIndex = WriteFloat(mtx.m22, buffer, startIndex);
            startIndex = WriteFloat(mtx.m23, buffer, startIndex);

            startIndex = WriteFloat(mtx.m30, buffer, startIndex);
            startIndex = WriteFloat(mtx.m31, buffer, startIndex);
            startIndex = WriteFloat(mtx.m32, buffer, startIndex);
            startIndex = WriteFloat(mtx.m33, buffer, startIndex);

            return startIndex;
        }

        /// <summary>
        /// Reads matrix 4x4. The total read length = 4 * 16 = 64
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static Matrix4x4 ReadMatrix4x4(byte[] buffer, int startIndex = 0)
        {
            Matrix4x4 mtx = new Matrix4x4();
            mtx.m00 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m01 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m02 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m03 = ReadFloat(buffer, startIndex); startIndex += 4;

            mtx.m10 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m11 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m12 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m13 = ReadFloat(buffer, startIndex); startIndex += 4;

            mtx.m20 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m21 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m22 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m23 = ReadFloat(buffer, startIndex); startIndex += 4;

            mtx.m30 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m31 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m32 = ReadFloat(buffer, startIndex); startIndex += 4;
            mtx.m33 = ReadFloat(buffer, startIndex); startIndex += 4;

            return mtx;
        }

        /// <summary>
        /// Gets the raycast hit point of the plane and the ray.
        /// </summary>
        /// <returns><c>true</c>, if raycast point was gotten, <c>false</c> otherwise.</returns>
        /// <param name="plane">Plane.</param>
        /// <param name="hitPoint">Raycast hit point.</param>
        public static bool GetRaycastPoint(this Plane plane, Ray ray, out Vector3 hitPoint)
        {
            bool ret = plane.Raycast(ray, out float enter);
            hitPoint = ray.GetPoint(enter);
            return ret;
        }

        /// <summary>
        /// Gets the raycast hit point of the plane and the ray.
        /// </summary>
        /// <returns><c>true</c>, if raycast point was gotten, <c>false</c> otherwise.</returns>
        /// <param name="plane">Plane.</param>
        /// <param name="hitPoint">Raycast hit point.</param>
        /// <param name="distance">distance.</param>
        public static bool GetRaycastPoint(this Plane plane, Ray ray, out Vector3 hitPoint, out float distance)
        {
            bool ret = plane.Raycast(ray, out float enter);
            distance = enter;
            hitPoint = ray.GetPoint(enter);
            return ret;
        }


        /// <summary>
        /// Copies the directionary from src to dst.
        /// </summary>
        /// <param name="src">Source.</param>
        /// <param name="dst">Dst.</param>
        /// <typeparam name="K">The 1st type parameter.</typeparam>
        /// <typeparam name="V">The 2nd type parameter.</typeparam>
        public static void CopyTo<K, V>(this Dictionary<K, V> src, Dictionary<K, V> dst)
        {
            foreach (var k in src.Keys)
            {
                dst.Add(k, src[k]);
            }
        }

        /// <summary>
        /// If text contains char.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="Char"></param>
        /// <returns></returns>
        public static bool Contains(this string text, char Char)
        {
            for (int i = 0, iMax = text.Length; i < iMax; i++)
            {
                if (text[i] == Char)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查 texts[] 是否包含 other 字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="Char"></param>
        /// <returns></returns>
        public static bool Contains(this string[] texts, string other, System.StringComparison stringComparison)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                string s = texts[i];
                if (s.Equals(other, stringComparison))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查 texts[] 是否包含 other 字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="Char"></param>
        /// <returns></returns>
        public static bool Contains(this IList<string> texts, string other, System.StringComparison stringComparison)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                string s = texts[i];
                if (s.Equals(other, stringComparison))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Fades out audio source.
        /// After fading , the volume will be restored to orginal value and the audio source will be stopped.
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="fadeOutTime"></param>
        public static async Task FadeOutAudio(this AudioSource audioSource, float fadeOutTime, float targetVolume = 0)
        {
            float originalVolume = audioSource.volume;
            float st = Time.time;
            while ((Time.time - st) <= fadeOutTime)
            {
                await new WaitForNextFrame();
                float vol = Mathf.Lerp(originalVolume, targetVolume, t: (Time.time - st) / fadeOutTime);
                audioSource.volume = vol;
            }

            //Stop at the end:
            audioSource.Stop();
            audioSource.volume = originalVolume;//restore original volume
        }
        /// <summary>
        /// Creates and adds a child game object to gameObject's hierarchy, then adds a component and return the added component.
        /// The child is set to identity point of game object's space.
        /// </summary>
        /// <typeparam name="T">The component.</typeparam>
        /// <param name="gameObject"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static T AddChildGameObject<T>(this GameObject gameObject, string childName) where T : Component
        {
            var child = new GameObject(childName);
            child.transform.SetParenAtIdentityPosition(gameObject.transform);
            return child.AddComponent<T>();
        }

        /// <summary>
        /// Fades in audio source. The audio volume will be fade from 0 to targetVolume.
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="fadeInTime"></param>
        public static async Task FadeInAudio(this AudioSource audioSource, float fadeInTime, float targetVolume = 1, float delayTime = 0)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0;
                if (delayTime > 0)
                {
                    audioSource.PlayDelayed(delayTime);
                    await new WaitForGameTime(delayTime);
                }
                else
                    audioSource.Play();
            }
            float st = Time.time;
            while ((Time.time - st) <= fadeInTime)
            {
                await new WaitForNextFrame();
                float t = (Time.time - st) / fadeInTime;
                float vol = Mathf.Lerp(0, targetVolume, t);
                audioSource.volume = vol;
                //Debug.LogFormat("Set volume : {0}.{1}, time:{2}, target volume: {3}", audioSource.name, vol, Time.time - st, targetVolume);
            }
            audioSource.volume = targetVolume;
        }
    }
}
