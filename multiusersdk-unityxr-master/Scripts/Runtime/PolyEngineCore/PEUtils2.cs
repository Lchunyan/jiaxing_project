using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine.SceneManagement;
using Ximmerse.XR.Asyncoroutine;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Ximmerse
{
    /// <summary>
    /// PEUtils2.cs.
    /// Partial class to PEUtils
    /// </summary>
    public static partial class PEUtils
    {
        public const string kEmpty = "";
        /// <summary>
        /// Finds a game object with tag, if not exists, create one.
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="Name">对象名字。如果对象不存在，则创建此名字的对象。</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject FindOrCreateGameObjectWithTag(string Tag, string Name = kEmpty)
        {
            var go = GameObject.FindGameObjectWithTag(Tag);
            if (!ReferenceEquals(null, go))
            {
                return go;
            }
            else
            {
                go = new GameObject(kEmpty);
                go.tag = Tag;
                return go;
            }
        }

        /// <summary>
        /// If the game object has component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject)
            {
                return false;
            }
            return gameObject.GetComponent<T>() != null;
        }

        /// <summary>
        /// If the game object has component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this GameObject gameObject, out T component) where T : Component
        {
            if (!gameObject)
            {
                component = null;
                return false;
            }
            component = gameObject.GetComponent<T>();
            return component != null;
        }

        /// <summary>
        /// If the game object has component in children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInChildren<T>(this GameObject gameObject) where T : Component
        {
            return gameObject != null ? gameObject.GetComponentInChildren<T>() != null : false;
        }

        /// <summary>
        /// If the game object has component in children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInChildren<T>(this GameObject gameObject, out T component) where T : Component
        {
            if (!gameObject)
            {
                component = null;
                return false;
            }
            component = gameObject.GetComponentInChildren<T>();
            return component != null;
        }

        /// <summary>
        /// If the game object has component in parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="IncludeSelf">是否从自身还是从parent开始查找组件</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInParent<T>(this GameObject gameObject, bool IncludeSelf, bool IncludeInactive = false) where T : Component
        {
            T component = default(T);
            if (!gameObject)
            {
                component = null;
                return false;
            }
            if (IncludeSelf)
            {
#if UNITY_2020_1_OR_NEWER
                component = gameObject.GetComponentInParent<T>(IncludeInactive);//unity builtin after 2020
#else
                component = gameObject.GetComponent<T>();
                if (!component)
                {
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
                }
#endif
                return component != null;
            }
            else
            {
                if (gameObject.transform.parent)
                {
#if UNITY_2020_1_OR_NEWER
                    component = gameObject.transform.parent.gameObject.GetComponentInParent<T>(IncludeInactive);
#else
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
#endif
                    return component != null;
                }
                else
                {
                    component = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// If the game object has component in parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="component"></param>
        /// <param name="IncludeSelf">是否从自身还是从parent开始查找组件</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInParent<T>(this GameObject gameObject, out T component, bool IncludeSelf, bool IncludeInactive = false) where T : Component
        {
            if (!gameObject)
            {
                component = null;
                return false;
            }
            if (IncludeSelf)
            {
#if UNITY_2020_1_OR_NEWER
                component = gameObject.GetComponentInParent<T>(IncludeInactive);//unity builtin after 2020
#else
                component = gameObject.GetComponent<T>();
                if (!component)
                {
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
                }
#endif
                return component != null;
            }
            else
            {
                if (gameObject.transform.parent)
                {
#if UNITY_2020_1_OR_NEWER
                    component = gameObject.transform.parent.gameObject.GetComponentInParent<T>(IncludeInactive);
#else
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
#endif
                    return component != null;
                }
                else
                {
                    component = null;
                    return false;
                }
            }
        }


        /// <summary>
        /// If the game object has component in parent and children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInHierarchy<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject)
            {
                return false;
            }
            return gameObject.GetComponentInParent<T>() != null || gameObject.GetComponentInChildren<T>() != null;
        }


        /// <summary>
        /// If the game object has component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this Component comp) where T : Component
        {
            if (!comp)
            {
                return false;
            }
            return comp.gameObject.GetComponent<T>() != null;
        }

        /// <summary>
        /// If the game object has component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this Component comp, out T component) where T : Component
        {
            if (!comp)
            {
                component = null;
                return false;
            }
            component = comp.gameObject.GetComponent<T>();
            return component != null;
        }

        /// <summary>
        /// If the game object has component in children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInChildren<T>(this Component comp) where T : Component
        {
            if (!comp)
            {
                return false;
            }
            return comp.GetComponentInChildren<T>() != null;
        }

        /// <summary>
        /// If the game object has component in parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <param name="includeSelf">是否包含自身?如果false，会忽略自身而只查找parent层级以上的对象。</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInParent<T>(this Component comp, bool IncludeSelf = true, bool IncludeInactive = false) where T : Component
        {
            T component = default(T);
            var gameObject = comp.gameObject;
            if (!gameObject)
            {
                component = null;
                return false;
            }
            if (IncludeSelf)
            {
#if UNITY_2020_1_OR_NEWER
                component = gameObject.GetComponentInParent<T>(IncludeInactive);//unity builtin after 2020
#else
                component = gameObject.GetComponent<T>();
                if (!component)
                {
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
                }
#endif
                return component != null;
            }
            else
            {
                if (gameObject.transform.parent)
                {
#if UNITY_2020_1_OR_NEWER
                    component = gameObject.transform.parent.gameObject.GetComponentInParent<T>(IncludeInactive);
#else
                    component = gameObject.InternalGetComponentInParent<T>(IncludeInactive);
#endif
                    return component != null;
                }
                else
                {
                    component = null;
                    return false;
                }
            }
        }


        /// <summary>
        /// If the game object has component in children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInChildren<T>(this Component comp, out T component) where T : Component
        {
            if (!comp)
            {
                component = null;
                return false;
            }
            component = comp.GetComponentInChildren<T>();
            return component != null;
        }

        /// <summary>
        /// If the game object has component in parent
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInParent<T>(this Component comp, out T component, bool includeSelf = true, bool IncludeInactive = false) where T : Component
        {
            return PEUtils.HasComponentInParent<T>(comp.gameObject, out component, includeSelf, IncludeInactive);
        }

        /// <summary>
        /// If the game object has component in parent and children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInHierarchy<T>(this Component comp, bool IncludeInactive = false) where T : Component
        {
            if (!comp)
            {
                return false;
            }
            return (comp.gameObject.HasComponentInParent<T>(true, IncludeInactive)) || (comp.gameObject.GetComponentInChildren<T>(IncludeInactive) != null);
        }

        /// <summary>
        /// If the game object has component in parent and children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInHierarchy<T>(this Component comp, out T component, bool IncludeInactive = false) where T : Component
        {
            if (!comp)
            {
                component = null;
                return false;
            }
            return (comp.gameObject.HasComponentInParent<T>(out component, true, IncludeInactive)) || (comp.gameObject.GetComponentInChildren<T>(IncludeInactive) != null);

        }

        /// <summary>
        /// If the game object has component in parent and children
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponentInHierarchy<T>(this GameObject gameObject, out T component, bool IncludeInactive = false) where T : Component
        {
            if (!gameObject)
            {
                component = null;
                return false;
            }
            gameObject.HasComponentInParent<T>(out component, true, IncludeInactive);
            if (component != null)
            {
                return true;
            }
            component = gameObject.GetComponentInChildren<T>(IncludeInactive);
            if (component != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes the vector2 to the buffer from start index, length = 8.
        /// </summary>
        /// <param name="vect2">Vector2 value.</param>
        /// <param name="buffer">Buffer list.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector2(Vector2 vect2, List<byte> buffer)
        {
            int from = buffer.Count - 1;
            floatConverter.floatValue = vect2.x;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect2.y;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            return from + 8;
        }

        /// <summary>
        /// Reads the vector2 from buffer at start index, length = 8.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector2 ReadVector2(List<byte> buffer, int startIndex = 0)
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
        public static int WriteVector3(Vector3 vect3, List<byte> buffer)
        {
            floatConverter.floatValue = vect3.x;
            int idx = buffer.Count - 1;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect3.y;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect3.z;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            return idx + 12;
        }

        /// <summary>
        /// Reads the vector3 from buffer at start index, length = 12
        /// </summary>
        /// <returns>The vector3.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector3 ReadVector3(List<byte> buffer, int startIndex = 0)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex + 4);
            float z = ReadFloat(buffer, startIndex + 8);
            return new Vector3(x, y, z);
        }


        /// <summary>
        /// Writes the vector4 to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="vect4">Vector4 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector4(Vector4 vect4, List<byte> buffer)
        {
            int from = buffer.Count - 1;
            floatConverter.floatValue = vect4.x;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect4.y;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect4.z;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = vect4.w;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            return from + 16;
        }


        /// <summary>
        /// Writes the vector4int to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="vect4">Vector4 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteVector4Int(pVector4Int vect4Int, byte[] buffer, int startIndex)
        {
            int from = startIndex;
            buffer[from++] = (byte)(vect4Int.x & 0xff);
            buffer[from++] = (byte)(vect4Int.x >> 8 & 0xff);
            buffer[from++] = (byte)(vect4Int.x >> 16 & 0xff);
            buffer[from++] = (byte)(vect4Int.x >> 24 & 0xff);

            buffer[from++] = (byte)(vect4Int.y & 0xff);
            buffer[from++] = (byte)(vect4Int.y >> 8 & 0xff);
            buffer[from++] = (byte)(vect4Int.y >> 16 & 0xff);
            buffer[from++] = (byte)(vect4Int.y >> 24 & 0xff);

            buffer[from++] = (byte)(vect4Int.z & 0xff);
            buffer[from++] = (byte)(vect4Int.z >> 8 & 0xff);
            buffer[from++] = (byte)(vect4Int.z >> 16 & 0xff);
            buffer[from++] = (byte)(vect4Int.z >> 24 & 0xff);

            buffer[from++] = (byte)(vect4Int.w & 0xff);
            buffer[from++] = (byte)(vect4Int.w >> 8 & 0xff);
            buffer[from++] = (byte)(vect4Int.w >> 16 & 0xff);
            buffer[from++] = (byte)(vect4Int.w >> 24 & 0xff);

            return startIndex + 16;
        }


        /// <summary>
        /// Writes the vector4int to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="vect4">Vector4 value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        /// <returns>buffer list's last element index.</returns>
        public static int WriteVector4Int(pVector4Int vect4Int, List<byte> buffer)
        {
            buffer.Add((byte)(vect4Int.x & 0xff));
            buffer.Add((byte)(vect4Int.x >> 8 & 0xff));
            buffer.Add((byte)(vect4Int.x >> 16 & 0xff));
            buffer.Add((byte)(vect4Int.x >> 24 & 0xff));

            buffer.Add((byte)(vect4Int.y & 0xff));
            buffer.Add((byte)(vect4Int.y >> 8 & 0xff));
            buffer.Add((byte)(vect4Int.y >> 16 & 0xff));
            buffer.Add((byte)(vect4Int.y >> 24 & 0xff));

            buffer.Add((byte)(vect4Int.z & 0xff));
            buffer.Add((byte)(vect4Int.z >> 8 & 0xff));
            buffer.Add((byte)(vect4Int.z >> 16 & 0xff));
            buffer.Add((byte)(vect4Int.z >> 24 & 0xff));

            buffer.Add((byte)(vect4Int.w & 0xff));
            buffer.Add((byte)(vect4Int.w >> 8 & 0xff));
            buffer.Add((byte)(vect4Int.w >> 16 & 0xff));
            buffer.Add((byte)(vect4Int.w >> 24 & 0xff));

            return buffer.Count - 1;
        }

        /// <summary>
        /// Read vector4 int from byte[] buffer.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static pVector4Int ReadVector4Int(byte[] Buffer, int startIndex)
        {
            pVector4Int ret = new pVector4Int();

            int _index = startIndex;
            uint x = 0;
            x |= Buffer[_index++];
            x |= (uint)(Buffer[_index++] << 8);
            x |= (uint)(Buffer[_index++] << 16);
            x |= (uint)(Buffer[_index++] << 24);
            ret.x = (int)x;


            uint y = 0;
            y |= Buffer[_index++];
            y |= (uint)(Buffer[_index++] << 8);
            y |= (uint)(Buffer[_index++] << 16);
            y |= (uint)(Buffer[_index++] << 24);
            ret.y = (int)y;


            uint z = 0;
            z |= Buffer[_index++];
            z |= (uint)(Buffer[_index++] << 8);
            z |= (uint)(Buffer[_index++] << 16);
            z |= (uint)(Buffer[_index++] << 24);
            ret.z = (int)z;


            uint w = 0;
            w |= Buffer[_index++];
            w |= (uint)(Buffer[_index++] << 8);
            w |= (uint)(Buffer[_index++] << 16);
            w |= (uint)(Buffer[_index++] << 24);
            ret.w = (int)w;

            return ret;
        }


        /// <summary>
        /// Read vector4 int from byte list.
        /// </summary>
        /// <param name="Buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static pVector4Int ReadVector4Int(List<byte> Buffer, int startIndex)
        {
            pVector4Int ret = new pVector4Int();

            int _index = startIndex;
            uint x = 0;
            x |= Buffer[_index++];
            x |= (uint)(Buffer[_index++] << 8);
            x |= (uint)(Buffer[_index++] << 16);
            x |= (uint)(Buffer[_index++] << 24);
            ret.x = (int)x;


            uint y = 0;
            y |= Buffer[_index++];
            y |= (uint)(Buffer[_index++] << 8);
            y |= (uint)(Buffer[_index++] << 16);
            y |= (uint)(Buffer[_index++] << 24);
            ret.y = (int)y;


            uint z = 0;
            z |= Buffer[_index++];
            z |= (uint)(Buffer[_index++] << 8);
            z |= (uint)(Buffer[_index++] << 16);
            z |= (uint)(Buffer[_index++] << 24);
            ret.z = (int)z;


            uint w = 0;
            w |= Buffer[_index++];
            w |= (uint)(Buffer[_index++] << 8);
            w |= (uint)(Buffer[_index++] << 16);
            w |= (uint)(Buffer[_index++] << 24);
            ret.w = (int)w;

            return ret;
        }

        /// <summary>
        /// Writes the quaternion to the buffer from start index, length = 16.
        /// </summary>
        /// <param name="q">Quaternion value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteQuaternion(Quaternion q, List<byte> buffer)
        {
            floatConverter.floatValue = q.x;
            int idx = buffer.Count - 1;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = q.y;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = q.z;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            floatConverter.floatValue = q.w;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            return idx + 16;
        }


        /// <summary>
        /// Writes the matrix 4x4.
        /// </summary>
        /// <param name="mtx">Value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteMatrix4x4(Matrix4x4 mtx, List<byte> buffer, int startIndex = 0)
        {
            startIndex = WriteFloat(mtx.m00, buffer);
            startIndex = WriteFloat(mtx.m01, buffer);
            startIndex = WriteFloat(mtx.m02, buffer);
            startIndex = WriteFloat(mtx.m03, buffer);

            startIndex = WriteFloat(mtx.m10, buffer);
            startIndex = WriteFloat(mtx.m11, buffer);
            startIndex = WriteFloat(mtx.m12, buffer);
            startIndex = WriteFloat(mtx.m13, buffer);

            startIndex = WriteFloat(mtx.m20, buffer);
            startIndex = WriteFloat(mtx.m21, buffer);
            startIndex = WriteFloat(mtx.m22, buffer);
            startIndex = WriteFloat(mtx.m23, buffer);

            startIndex = WriteFloat(mtx.m30, buffer);
            startIndex = WriteFloat(mtx.m31, buffer);
            startIndex = WriteFloat(mtx.m32, buffer);
            startIndex = WriteFloat(mtx.m33, buffer);

            return startIndex;
        }


        /// <summary>
        /// Reads matrix 4x4. The total read length = 4 * 16 = 64
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static Matrix4x4 ReadMatrix4x4(List<byte> buffer, int startIndex = 0)
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
        /// Reads the vector4 from buffer at start index, length = 16
        /// </summary>
        /// <returns>The vector4.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector4 ReadVector4(List<byte> buffer, int startIndex = 0)
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
        public static Quaternion ReadQuaternion(List<byte> buffer, int startIndex = 0)
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
        public static int WriteInt(int intValue, List<byte> buffer)
        {
            int from = buffer.Count - 1;
            buffer.Add((byte)(intValue & 0xff));
            buffer.Add((byte)(intValue >> 8 & 0xff));
            buffer.Add((byte)(intValue >> 16 & 0xff));
            buffer.Add((byte)(intValue >> 24 & 0xff));

            return from + 4;
        }

        /// <summary>
        /// Reads the int from buffer at start index, length = 4
        /// </summary>
        public static int ReadInt(List<byte> buffer, int startIndex = 0)
        {
            uint value = 0;
            value |= buffer[startIndex++];
            value |= (uint)(buffer[startIndex++] << 8);
            value |= (uint)(buffer[startIndex++] << 16);
            value |= (uint)(buffer[startIndex++] << 24);
            return (int)value;
        }

        /// <summary>
        /// Read the specified decimal value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static decimal ReadDecimal(byte[] buffer, int startIndex = 0)
        {
            decimalLongConverter.ulong01 = ReadUInt64(buffer, startIndex);
            decimalLongConverter.ulong02 = ReadUInt64(buffer, startIndex + 8);
            return decimalLongConverter._deciaml;//decimal = 2 个 long
        }

        /// <summary>
        /// Reads a ulong from buffer.
        /// </summary>
        /// <returns>The long.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static ulong ReadUInt64(List<byte> buffer, int startIndex = 0)
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
        /// Reads the double from buffer at start index, length = 8
        /// </summary>
        /// <returns>The double.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static double ReadDouble(List<byte> buffer, int startIndex = 0)
        {
            ulong value = ReadUInt64(buffer, startIndex);
            return floatConverter.ToDouble(value);
        }

        /// <summary>
        /// Write the specified ulong value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static int WriteULong(ulong value, List<byte> buffer)
        {
            int startIndex = buffer.Count - 1;
            buffer.Add((byte)(value & 0xff));
            buffer.Add((byte)((value >> 8) & 0xff));
            buffer.Add((byte)((value >> 16) & 0xff));
            buffer.Add((byte)((value >> 24) & 0xff));
            buffer.Add((byte)((value >> 32) & 0xff));
            buffer.Add((byte)((value >> 40) & 0xff));
            buffer.Add((byte)((value >> 48) & 0xff));
            buffer.Add((byte)((value >> 56) & 0xff));

            return startIndex + 8;
        }

        /// <summary>
        /// Write byte into buffer
        /// </summary>
        /// <param name="byteValue"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteByte(byte byteValue, List<byte> buffer, int startIndex = 0)
        {
            int from = startIndex;
            buffer[from++] = byteValue;
            return startIndex + 1;
        }


        /// <summary>
        /// Writes the double.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteDouble(double value, List<byte> buffer)
        {
            int startIndex = buffer.Count - 1;
            floatConverter.doubleValue = value;
            WriteULong(floatConverter.ulongValue, buffer);
            return startIndex + 8;
        }


        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteFloat(float floatValue, List<byte> buffer)
        {
            int from = buffer.Count - 1;
            floatConverter.floatValue = floatValue;
            buffer.Add((byte)(floatConverter.uintValue & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.Add((byte)(floatConverter.uintValue >> 24 & 0xff));

            return from + 4;
        }

        /// <summary>
        /// Reads the float from buffer at start index, length = 4.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static float ReadFloat(List<byte> buffer, int startIndex = 0)
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
        /// Snap the transform's position at X,Y,Z axis.
        /// Set X,Y,Z of snap length to zero to ignore snapping at the axe.
        /// The value should be GT 0.
        /// Return true if the transform's position is changed.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="SnapLength">Set X,Y,Z of snap length to zero to ignore snapping at the axe.</param>
        public static bool SnapPosition(this Transform transform, Vector3 SnapLength, Space space = Space.World)
        {
            Vector3 p = space == Space.World ? transform.position : transform.localPosition;
            //snap x:
            if (SnapLength.x > 0)
            {
                p.x = Mathf.RoundToInt(p.x / SnapLength.x) * SnapLength.x;
            }
            if (SnapLength.y > 0)
            {
                p.y = Mathf.RoundToInt(p.y / SnapLength.y) * SnapLength.y;
            }
            if (SnapLength.z > 0)
            {
                p.z = Mathf.RoundToInt(p.z / SnapLength.z) * SnapLength.z;
            }
            if (space == Space.World)
            {
                bool dirty = transform.position != p;
                if (dirty)
                    transform.position = p;
                return dirty;
            }
            else
            {
                bool dirty = transform.localPosition != p;
                if (dirty)
                    transform.localPosition = p;
                return dirty;
            }
        }



        /// <summary>
        /// Snap the position at X,Y,Z axis.
        /// Set X,Y,Z of snap length to zero to ignore snapping at the axe.
        /// The value should be GT 0.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="SnapLength">Set X,Y,Z of snap length to zero to ignore snapping at the axe.</param>
        public static Vector3 SnapPosition(this Vector3 position, Vector3 SnapLength)
        {
            Vector3 p = position;
            //snap x:
            if (SnapLength.x > 0)
            {
                p.x = Mathf.RoundToInt(p.x / SnapLength.x) * SnapLength.x;
            }
            if (SnapLength.y > 0)
            {
                p.y = Mathf.RoundToInt(p.y / SnapLength.y) * SnapLength.y;
            }
            if (SnapLength.z > 0)
            {
                p.z = Mathf.RoundToInt(p.z / SnapLength.z) * SnapLength.z;
            }
            return p;
        }


        /// <summary>
        /// Gets a new texture , only taking the target channel of the original texture.
        /// </summary>
        /// <param name="texture2D"></param>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <param name="A"></param>
        /// <param name="format">If not null, use the format, else, use original texture format</param>
        /// <returns></returns>
        public static Texture2D GetChannelTexture(this Texture2D texture2D, bool R, bool G, bool B, bool A, TextureFormat? format = default(TextureFormat?))
        {
            if (texture2D.isReadable)
            {
                try
                {
                    Color[] pixels = texture2D.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (!R)
                        {
                            pixels[i].r = 0;
                        }
                        if (!G)
                        {
                            pixels[i].g = 0;
                        }
                        if (!B)
                        {
                            pixels[i].b = 0;
                        }
                        if (!A)
                        {
                            pixels[i].a = 0;
                        }
                    }
                    var newTexture = new Texture2D(texture2D.width, texture2D.height, format.HasValue ? format.Value : texture2D.format, false);
                    newTexture.SetPixels(pixels);
                    return newTexture;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }
            else
            {
                Debug.LogErrorFormat("Texture2D: {0} is not readable .", texture2D.name);
                return null;
            }
        }

        /// <summary>
        /// Writes a text into buffer, starts at startIndex.
        /// Encoded in UTF8.
        /// Return the end index.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteString(string text, List<byte> buffer)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            int idx = 0;
            int startIndex = buffer.Count - 1;
            for (int i = 0; i < bytes.Length; i++)
            {
                buffer.Add(bytes[idx]);
                idx++;
            }
            return startIndex + bytes.Length;
        }

        /// <summary>
        /// Writes the short to the buffer from start index, length = 2
        /// Return interger indicates the returned buffer index = startIndex + 2
        /// </summary>
        /// <param name="shortValue">Short value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static int WriteShort(short shortValue, List<byte> buffer, int startIndex = 0)
        {
            int from = startIndex;
            buffer[from++] = (byte)(shortValue & 0xff);
            buffer[from++] = (byte)(shortValue >> 8 & 0xff);
            return startIndex + 2;
        }

        /// <summary>
        /// Read the int32 arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int[] ReadIntList(List<byte> buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            int[] value = new int[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadInt(buffer, startIndex);
                startIndex += 4;
            }
            return value;
        }

        /// <summary>
        /// Read the int32 arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int[] ReadIntList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            int[] value = new int[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadInt(buffer, startIndex);
                startIndex += 4;
            }
            return value;
        }

        /// <summary>
        /// Writes the int32 arr to the buffer from start index, length = 4 + length * 4, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteIntList(int[] array, List<byte> buffer)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteInt(array[i], buffer);
            }
            return index;
        }

        /// <summary>
        /// Writes the int32 arr to the buffer from start index, length = 4 + length * 4, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteIntList(int[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteInt(array[i], buffer, index);
            }
            return index;
        }



        /// <summary>
        /// Read the float arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static float[] ReadFloatList(List<byte> buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            float[] value = new float[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadFloat(buffer, startIndex);
                startIndex += 4;
            }
            return value;
        }

        /// <summary>
        /// Read the float arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static float[] ReadFloatList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            float[] value = new float[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadFloat(buffer, startIndex);
                startIndex += 4;
            }
            return value;
        }

        /// <summary>
        /// Writes the float arr to the buffer from start index, length = 4 + length * 4, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteFloatList(float[] array, List<byte> buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteFloat(array[i], buffer);
            }
            return index;
        }

        /// <summary>
        /// Writes the float arr to the buffer from start index, length = 4 + length * 4, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteFloatList(float[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteFloat(array[i], buffer, index);
            }
            return index;
        }

        /// <summary>
        /// Read the short arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static short[] ReadShortList(List<byte> buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            short[] value = new short[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadShort(buffer, startIndex);
                startIndex += 2;
            }
            return value;
        }

        /// <summary>
        /// Read the short arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static short[] ReadShortList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            short[] value = new short[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadShort(buffer, startIndex);
                startIndex += 2;
            }
            return value;
        }

        /// <summary>
        /// Writes the short arr to the buffer from start index, length = 4 + length * 2, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteShortList(short[] array, List<byte> buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteShort(array[i], buffer, index);
            }
            return index;
        }

        /// <summary>
        /// Writes the long arr to the buffer from start index, length = 4 + length * 2, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteShortList(short[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteShort(array[i], buffer, index);
            }
            return index;
        }

        /// <summary>
        /// Read the float arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static ulong[] ReadULongList(List<byte> buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            ulong[] value = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUInt64(buffer, startIndex);
                startIndex += 8;
            }
            return value;
        }

        /// <summary>
        /// Read the long arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static ulong[] ReadULongList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            ulong[] value = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadUInt64(buffer, startIndex);
                startIndex += 8;
            }
            return value;
        }

        /// <summary>
        /// Writes the long arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteULongList(ulong[] array, List<byte> buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteULong(array[i], buffer);
            }
            return index;
        }

        /// <summary>
        /// Writes the long arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteULongList(ulong[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteULong(array[i], buffer, index);
            }
            return index;
        }

        /// <summary>
        /// Read the double arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static double[] ReadDoubleList(List<byte> buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            double[] value = new double[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadDouble(buffer, startIndex);
                startIndex += 8;
            }
            return value;
        }

        /// <summary>
        /// Read the double arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static double[] ReadDoubleList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            double[] value = new double[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadDouble(buffer, startIndex);
                startIndex += 8;
            }
            return value;
        }

        /// <summary>
        /// Writes the double arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteDoubleList(double[] array, List<byte> buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteDouble(array[i], buffer);
            }
            return index;
        }

        /// <summary>
        /// Writes the double arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteDoubleList(double[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteDouble(array[i], buffer, index);
            }
            return index;
        }

        /// <summary>
        /// Read the string arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string[] ReadStringList(List<byte> bufferList, int startIndex = 0)
        {
            var buffer = bufferList.ToArray();
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            string[] value = new string[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadString(buffer, out int endIndex, startIndex);
                startIndex = endIndex;
            }
            return value;
        }

        /// <summary>
        /// Read the string arr from buffer at startIndex, start at arr length, then every arr element
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string[] ReadStringList(byte[] buffer, int startIndex = 0)
        {
            int length = ReadInt(buffer, startIndex);
            startIndex += 4;

            string[] value = new string[length];
            for (int i = 0; i < length; i++)
            {
                value[i] = ReadString(buffer, out int endIndex, startIndex);
                startIndex = endIndex;
            }
            return value;
        }

        /// <summary>
        /// Writes the string arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteStringList(string[] array, List<byte> buffer)
        {
            int index = WriteInt(array.Length, buffer);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteString(array[i], buffer);
            }
            return index;
        }

        /// <summary>
        /// Writes the string arr to the buffer from start index, length = 4 + length * 8, first byte will save the length of the arr then every arr element
        /// </summary>
        /// <param name="array"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int WriteStringList(string[] array, byte[] buffer, int startIndex = 0)
        {
            int index = WriteInt(array.Length, buffer, startIndex);
            for (int i = 0; i < array.Length; i++)
            {
                index = WriteString(array[i], buffer, index);
            }
            return index;
        }


        /// <summary>
        /// Convert a render texture into a pixel 2D texture。
        /// </summary>
        /// <param name="renderTexture"></param>
        /// <returns></returns>
        public static Texture2D Capture(this RenderTexture renderTexture, TextureFormat format = TextureFormat.RGBA32, bool mipChain = false, bool linear = false)
        {
            if (!renderTexture)
            {
                return null;
            }
            var old = RenderTexture.active;
            bool hasOld = old != null;
            RenderTexture.active = renderTexture;

            int w = renderTexture.width;
            int h = renderTexture.height;
            Texture2D buffer = new Texture2D(w, h, format, mipChain, linear);
            buffer.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            buffer.Apply();
            RenderTexture.active = old;
            return buffer;
        }

        /// <summary>
        /// Capture a rendertexture's pixels into an existsing texture2D object.
        /// </summary>
        /// <param name="renderTexture"></param>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Texture2D Capture(this RenderTexture renderTexture, Texture2D texture2D, TextureFormat format = TextureFormat.RGBA32, bool mipChain = false, bool linear = false)
        {
            if (!renderTexture)
            {
                return null;
            }
            var old = RenderTexture.active;
            bool hasOld = old != null;
            RenderTexture.active = renderTexture;

            int w = renderTexture.width;
            int h = renderTexture.height;
            if (texture2D.width != w || texture2D.height != h)
            {
                
#if UNITY_2021_2_OR_NEWER
            texture2D.Reinitialize(w, h, format, mipChain);
#else
            texture2D.Resize(w, h, format, mipChain);
#endif
            }
            texture2D.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        /// <summary>
        /// 对 Camera 进行截图。
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="format"></param>
        /// <param name="mipChain"></param>
        /// <param name="linear"></param>
        /// <returns></returns>
        public static Texture2D Capture(this Camera camera, TextureFormat format = TextureFormat.RGB24, bool mipChain = false, bool linear = false)
        {
            if (!camera)
                return null;


            RenderTexture tmp = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 24);
            if (tmp.IsCreated() == false)
            {
                tmp.Create();
            }
            camera.targetTexture = tmp;
            camera.Render();//manually render once
            camera.targetTexture = null;

            //var old = RenderTexture.active;
            //bool hasOld = old != null;
            RenderTexture.active = tmp;

            Texture2D buffer = new Texture2D(camera.pixelWidth, camera.pixelHeight, format, mipChain, linear);
            buffer.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
            buffer.Apply();
            RenderTexture.ReleaseTemporary(tmp);
            return buffer;
        }



        /// <summary>
        /// 使用自定义的 Pixel Width 和 Pixel Height, 对 Camera 进行截图。
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="PixelWidth"></param>
        /// <param name="PixelHeight"></param>
        /// <param name="format"></param>
        /// <param name="mipChain"></param>
        /// <param name="linear"></param>
        /// <returns></returns>
        public static Texture2D Capture(this Camera camera, int PixelWidth, int PixelHeight, TextureFormat format = TextureFormat.RGB24, bool mipChain = false, bool linear = false)
        {
            if (!camera)
                return null;


            RenderTexture tmp = RenderTexture.GetTemporary(PixelWidth, PixelHeight, 24);
            if (tmp.IsCreated() == false)
            {
                tmp.Create();
            }
            camera.targetTexture = tmp;
            camera.Render();//manually render once
            camera.targetTexture = null;

            //var old = RenderTexture.active;
            //bool hasOld = old != null;
            RenderTexture.active = tmp;

            Texture2D buffer = new Texture2D(PixelWidth, PixelHeight, format, mipChain, linear);
            buffer.ReadPixels(new Rect(0, 0, PixelWidth, PixelHeight), 0, 0);
            buffer.Apply();
            RenderTexture.ReleaseTemporary(tmp);
            return buffer;
        }

        /// <summary>
        /// Internal GetComponentInParent for unity 2018 , 2019
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="IncludeInactive"></param>
        /// <returns></returns>
        private static T InternalGetComponentInParent<T>(this GameObject gameObject, bool IncludeInactive)
        {
            //有上级对象:
            if (gameObject.transform.parent)
            {
                T t = gameObject.transform.parent.gameObject.GetComponent<T>();
                //No component:
                if (ReferenceEquals(t, default(T)))
                {
                    return InternalGetComponentInParent<T>(gameObject.transform.parent.gameObject, IncludeInactive);
                }
                //Has component:
                else
                {
                    bool isInactive = (!gameObject.transform.parent.gameObject.activeInHierarchy) ||
                         (t is MonoBehaviour && (t as MonoBehaviour).enabled == false);
                    if (!IncludeInactive && isInactive)
                    {
                        //如果parent 是deactive 的,  向上遍历
                        return InternalGetComponentInParent<T>(gameObject.transform.parent.gameObject, IncludeInactive);
                    }
                    else
                    {
                        return t;
                    }
                }
            }
            //向上遍历的层级已经到顶:
            else
            {
                return default(T);
            }
        }
    }
}
