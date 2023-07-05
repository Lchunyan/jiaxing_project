using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using UnityEngine.Networking;
using Ximmerse.XR.Asyncoroutine;
using System.Linq;
using System.Text;


namespace Ximmerse
{
    /// <summary>
    /// 和 Stream 序列化相关的方法。
    /// </summary>
    public static partial class PEUtils
    {

        /// <summary>
        /// Writes the vector2 to the buffer.
        /// </summary>
        /// <param name="vect3"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        public static void WriteVector2(Vector2 vect2, Stream buffer)
        {
            WriteFloat(vect2.x, buffer);
            WriteFloat(vect2.y, buffer);
        }

        /// <summary>
        /// Writes the vector2Int to the buffer.
        /// </summary>
        /// <param name="vect3"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        public static void WriteVector2Int(Vector2Int vect2Int, Stream buffer)
        {
            WriteInt(vect2Int.x, buffer);
            WriteInt(vect2Int.y, buffer);
        }

        /// <summary>
        /// Writes the vector2 to the buffer.
        /// </summary>
        /// <param name="vect3"></param>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        public static void WriteVector3Int(Vector3Int vect3Int, Stream buffer)
        {
            WriteInt(vect3Int.x, buffer);
            WriteInt(vect3Int.y, buffer);
            WriteInt(vect3Int.z, buffer);
        }

        /// <summary>
        /// Reads the vector2 from buffer at start index, length = 8.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector2 ReadVector2(Stream buffer, int? startIndex = null)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads the vector2 from buffer at start index, length = 8.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector2Int ReadVector2Int(Stream buffer, int? startIndex = null)
        {
            int x = ReadInt(buffer, startIndex);
            int y = ReadInt(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Reads the float from buffer at start index, length = 4.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static float ReadFloat(Stream buffer, int? startIndex = null)
        {
            uint value = 0;
            if (startIndex.HasValue)
                buffer.Position = startIndex.Value;

            value |= (byte)buffer.ReadByte();// buffer[startIndex++];
            value |= (uint)((byte)buffer.ReadByte() << 8);
            value |= (uint)((byte)buffer.ReadByte() << 16);
            value |= (uint)((byte)buffer.ReadByte() << 24);
            floatConverter.uintValue = value;
            return floatConverter.floatValue;
        }

        /// <summary>
        /// Reads the color from buffer at start index, length = 4.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Color ReadColor(Stream buffer, int? startIndex = null)
        {
            float r = ReadFloat(buffer);
            float g = ReadFloat(buffer);
            float b = ReadFloat(buffer);
            float a = ReadFloat(buffer);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Reads the color32 from buffer at start index, length = 4.
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Color32 ReadColor32(Stream buffer, int? startIndex = null)
        {
            byte r = (byte)buffer.ReadByte();
            byte g = (byte)buffer.ReadByte();
            byte b = (byte)buffer.ReadByte();
            byte a = (byte)buffer.ReadByte();
            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Reads the vector2 from buffer at start index, length = 8.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector3 ReadVector3(Stream buffer, int? startIndex = null)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            float z = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 8) : null);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads the ushort array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static ushort[] ReadUshortArray(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            ushort[] array = new ushort[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = (ushort)PEUtils.ReadShort(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the ushort array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadUshortArrayToList(Stream buffer, IList<ushort> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add((ushort)PEUtils.ReadShort(buffer));
            }
            return arrayLength;
        }

        /// <summary>
        /// Reads the short array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static short[] ReadShortArray(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            short[] array = new short[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadShort(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the short array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadShortArrayToList(Stream buffer, IList<short> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadShort(buffer));
            }
            return arrayLength;
        }

        /// <summary>
        /// Reads the uint array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static uint[] ReadUintArray(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            uint[] array = new uint[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = (uint)PEUtils.ReadInt(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the uint array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadUintArrayToList(Stream buffer, IList<uint> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add((uint)PEUtils.ReadInt(buffer));
            }
            return arrayLength;
        }

        /// <summary>
        /// Reads the int array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static int[] ReadIntArray(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            int[] array = new int[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadInt(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the int array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadIntArrayToList(Stream buffer, IList<int> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadInt(buffer));
            }
            return arrayLength;
        }


        /// <summary>
        /// Reads the vector2 array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static Vector2[] ReadVector2Array(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            Vector2[] array = new Vector2[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadVector2(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the vector2 array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadVector2ArrayToList(Stream buffer, IList<Vector2> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadVector2(buffer));
            }
            return arrayLength;
        }


        /// <summary>
        /// Reads the vector3 array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static Vector3[] ReadVector3Array(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            Vector3[] array = new Vector3[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadVector3(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the vector3 array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadVector3ArrayToList(Stream buffer, IList<Vector3> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadVector3(buffer));
            }
            return arrayLength;
        }


        /// <summary>
        /// Reads the vector4 array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static Vector4[] ReadVector4Array(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            Vector4[] array = new Vector4[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadVector4(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the vector2 array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadVector4ArrayToList(Stream buffer, IList<Vector4> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadVector4(buffer));
            }
            return arrayLength;
        }

        /// <summary>
        /// Reads the color array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static Color[] ReadColorArray(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            Color[] array = new Color[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadColor(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the Color array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadColorArrayToList(Stream buffer, IList<Color> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadColor(buffer));
            }
            return arrayLength;
        }


        /// <summary>
        /// Reads the color32 array from buffer 
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        public static Color32[] ReadColor32Array(Stream buffer)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            Color32[] array = new Color32[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                array[i] = PEUtils.ReadColor32(buffer);
            }
            return array;
        }

        /// <summary>
        ///  Reads the Color32 array from buffer , and fill into list.
        /// </summary>
        /// <returns>The read vector3 count.</returns>
        /// <param name="buffer">Buffer.</param>
        public static int ReadColor32ArrayToList(Stream buffer, IList<Color32> list)
        {
            int arrayLength = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arrayLength; i++)
            {
                list.Add(PEUtils.ReadColor32(buffer));
            }
            return arrayLength;
        }


        /// <summary>
        /// Reads the vector3int from buffer
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector3Int ReadVector3Int(Stream buffer, int? startIndex = null)
        {
            int x = ReadInt(buffer, startIndex);
            int y = ReadInt(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            int z = ReadInt(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 8) : null);
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Writes the vector2 to the buffer.
        /// </summary>
        /// <param name="vect3"></param>
        /// <param name="buffer"></param>
        public static void WriteVector3(Vector3 vect3, Stream buffer)
        {
            WriteFloat(vect3.x, buffer);
            WriteFloat(vect3.y, buffer);
            WriteFloat(vect3.z, buffer);
        }

        /// <summary>
        /// Reads the vector4 from buffer.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Vector4 ReadVector4(Stream buffer, int? startIndex = null)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            float z = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 8) : null);
            float w = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 12) : null);
            return new Vector4(x, y, z, w);
        }


        /// <summary>
        /// Writes the vector4 to the buffer.
        /// </summary>
        /// <param name="vect3"></param>
        /// <param name="buffer"></param>
        public static void WriteVector4(Vector4 vect4, Stream buffer)
        {
            WriteFloat(vect4.x, buffer);
            WriteFloat(vect4.y, buffer);
            WriteFloat(vect4.z, buffer);
            WriteFloat(vect4.w, buffer);
        }

        /// <summary>
        /// Writes the int to the buffer.
        /// Return interger indicates the returned buffer index = startIndex + 4
        /// </summary>
        /// <param name="intValue">Int value.</param>
        /// <param name="buffer">Buffer.</param>
        public static void WriteInt(int intValue, Stream buffer)
        {
            buffer.WriteByte((byte)(intValue & 0xff));
            buffer.WriteByte((byte)(intValue >> 8 & 0xff));
            buffer.WriteByte((byte)(intValue >> 16 & 0xff));
            buffer.WriteByte((byte)(intValue >> 24 & 0xff));
        }

        /// <summary>
        /// Writes the short to the buffer from start index, length = 2
        /// Return interger indicates the returned buffer index = startIndex + 2
        /// </summary>
        /// <param name="shortValue">Short value.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static void WriteShort(short shortValue, Stream buffer)
        {
            buffer.WriteByte((byte)(shortValue & 0xff));
            buffer.WriteByte((byte)(shortValue >> 8 & 0xff));
        }

        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        public static void WriteFloat(float floatValue, Stream buffer)
        {
            floatConverter.floatValue = floatValue;
            buffer.WriteByte((byte)(floatConverter.uintValue & 0xff));
            buffer.WriteByte((byte)(floatConverter.uintValue >> 8 & 0xff));
            buffer.WriteByte((byte)(floatConverter.uintValue >> 16 & 0xff));
            buffer.WriteByte((byte)(floatConverter.uintValue >> 24 & 0xff));
        }

        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        public static void WriteFloatArray(float[] floatValue, Stream buffer)
        {
            PEUtils.WriteInt(floatValue.Length, buffer);
            for (int i = 0; i < floatValue.Length; i++)
            {
                float f = floatValue[i];
                PEUtils.WriteFloat(f, buffer);
            }
        }

        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        public static float[] ReadFloatArray(Stream buffer, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            int arraySize = PEUtils.ReadInt(buffer);
            float[] array = new float[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                float f = PEUtils.ReadFloat(buffer);
                array[i] = f;
            }
            return array;
        }

        /// <summary>
        /// Writes the float to the buffer from start index, length = 4.
        /// </summary>
        /// <param name="floatValue">Float value.</param>
        /// <param name="buffer">Buffer.</param>
        public static int ReadFloatArray(Stream buffer, List<float> floats, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            int arraySize = PEUtils.ReadInt(buffer);
            for (int i = 0; i < arraySize; i++)
            {
                float f = PEUtils.ReadFloat(buffer);
                floats.Add(f);
            }
            return arraySize;
        }

        /// <summary>
        /// Reads short from memory stream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static short ReadShort(Stream buffer, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            short value = 0;
            value |= (short)((byte)buffer.ReadByte());
            value |= (short)((byte)buffer.ReadByte() << 8);
            return value;
        }


        /// <summary>
        /// Reads the int from buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex">If null , reads from position</param>
        public static int ReadInt(Stream buffer, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            uint value = 0;
            value |= (byte)buffer.ReadByte();
            value |= (uint)((byte)buffer.ReadByte() << 8);
            value |= (uint)((byte)buffer.ReadByte() << 16);
            value |= (uint)((byte)buffer.ReadByte() << 24);
            return (int)value;
        }

        public static ulong ReadUlong(Stream buffer, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            ulong value = 0;
            ulong other = (byte)buffer.ReadByte();
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 8;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 16;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 24;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 32;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 40;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 48;
            value |= other;

            other = ((ulong)(byte)buffer.ReadByte()) << 56;
            value |= other;

            return value;
        }

        public static long ReadLong(Stream buffer, int? startIndex = null)
        {
            if (startIndex.HasValue)
            {
                buffer.Position = startIndex.Value;
            }
            long value = 0;
            long other = (byte)buffer.ReadByte();
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 8;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 16;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 24;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 32;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 40;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 48;
            value |= other;

            other = ((long)(byte)buffer.ReadByte()) << 56;
            value |= other;

            return value;
        }

        /// <summary>
        /// Reads a quaternion from buffer at start index, length = 16.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static Quaternion ReadQuaternion(Stream buffer, int? startIndex = null)
        {
            float x = ReadFloat(buffer, startIndex);
            float y = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 4) : null);
            float z = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 8) : null);
            float w = ReadFloat(buffer, startIndex.HasValue ? (int?)(startIndex.Value + 12) : null);
            return new Quaternion(x, y, z, w);
        }



        /// <summary>
        /// Writes the color to buffer
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static void WriteColor(Color color, Stream buffer)
        {
            WriteFloat(color.r, buffer);
            WriteFloat(color.g, buffer);
            WriteFloat(color.b, buffer);
            WriteFloat(color.a, buffer);
        }

        /// <summary>
        /// Writes the color32 to buffer
        /// </summary>
        /// <returns>The float.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static void WriteColor32(Color32 color32, Stream buffer)
        {
            buffer.WriteByte(color32.r);
            buffer.WriteByte(color32.r);
            buffer.WriteByte(color32.b);
            buffer.WriteByte(color32.a);
        }

        /// <summary>
        /// Writes a quaternion to buffer.
        /// </summary>
        /// <returns>The vector2.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="startIndex">Start index.</param>
        public static void WriteQuaternion(Quaternion quaternion, Stream buffer)
        {
            WriteFloat(quaternion.x, buffer);
            WriteFloat(quaternion.y, buffer);
            WriteFloat(quaternion.z, buffer);
            WriteFloat(quaternion.w, buffer);
        }

        /// <summary>
        /// Write the specified ulong value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static void WriteULong(ulong value, Stream ms)
        {
            ms.WriteByte((byte)(value & 0xff));
            ms.WriteByte((byte)((value >> 8) & 0xff));
            ms.WriteByte((byte)((value >> 16) & 0xff));
            ms.WriteByte((byte)((value >> 24) & 0xff));
            ms.WriteByte((byte)((value >> 32) & 0xff));
            ms.WriteByte((byte)((value >> 40) & 0xff));
            ms.WriteByte((byte)((value >> 48) & 0xff));
            ms.WriteByte((byte)((value >> 56) & 0xff));
        }

        /// <summary>
        /// Write the specified long value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static void WriteLong(long value, Stream ms)
        {
            ms.WriteByte((byte)(value & 0xff));
            ms.WriteByte((byte)((value >> 8) & 0xff));
            ms.WriteByte((byte)((value >> 16) & 0xff));
            ms.WriteByte((byte)((value >> 24) & 0xff));
            ms.WriteByte((byte)((value >> 32) & 0xff));
            ms.WriteByte((byte)((value >> 40) & 0xff));
            ms.WriteByte((byte)((value >> 48) & 0xff));
            ms.WriteByte((byte)((value >> 56) & 0xff));
        }


        /// <summary>
        /// Writes a text into buffer, starts at startIndex.
        /// First 2 bytes = string length. (Assume string within 65535 length
        /// Encoded in UTF8.
        /// Return : end index.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static void WriteString(string text, Stream buffer)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(text);
            PEUtils.WriteShort((short)stringBytes.Length, buffer);
            buffer.Write(stringBytes, 0, stringBytes.Length);
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
        public static long WriteByteArray(byte[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            dest.Write(source, sourceOffset, length);
            index += length;
            return index;
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
        public static long WriteByteList(IList<byte> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                byte value = source[i];
                dest.WriteByte(value);//write byte
                index += 1;
            }
            return index;
        }

        /// <summary>
        /// Write short array into another byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteShortArray(short[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                short value = source[i];
                WriteShort(value, dest);//write short
                index += 2;
            }
            return index;
        }

        /// <summary>
        /// Write short list into another byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteShortList(IList<short> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                short value = source[i];
                WriteShort(value, dest);//write short
                index += 2;
            }
            return index;
        }

        /// <summary>
        /// Write int array into another byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteIntArray(int[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                int value = source[i];
                WriteInt(value, dest);//write int
                index += 4;
            }
            return index;
        }

        /// <summary>
        /// Write int list into another byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteIntList(IList<int> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                int value = source[i];
                WriteInt(value, dest);//write int
                index += 4;
            }
            return index;
        }


        /// <summary>
        /// Write float array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteFloatArray(float[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                float value = source[i];
                WriteFloat(value, dest);//write int
                index += 4;
            }
            return index;
        }

        /// <summary>
        /// Write float list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteFloatList(IList<float> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                float value = source[i];
                WriteFloat(value, dest);//write int
                index += 4;
            }
            return index;
        }


        /// <summary>
        /// Write long array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteLongArray(long[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                long value = source[i];
                WriteLong(value, dest);//write long
                index += 8;
            }
            return index;
        }

        /// <summary>
        /// Write long list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteLongList(IList<long> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                long value = source[i];
                WriteLong(value, dest);//write long
                index += 8;
            }
            return index;
        }


        /// <summary>
        /// Write vector2 array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector2Array(Vector2[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector2 value = source[i];
                WriteVector2(value, dest);//write vector2
                index += 8;
            }
            return index;
        }

        /// <summary>
        /// Write vector2 list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector2List(IList<Vector2> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector2 value = source[i];
                WriteVector2(value, dest);//write vector2
                index += 8;
            }
            return index;
        }

        /// <summary>
        /// Write Vector3 array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector3Array(Vector3[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector3 value = source[i];
                WriteVector3(value, dest);//write vector3
                index += 12;
            }
            return index;
        }

        /// <summary>
        /// Write vector3 list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector3List(IList<Vector3> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector3 value = source[i];
                WriteVector3(value, dest);//write vector3
                index += 12;
            }
            return index;
        }


        /// <summary>
        /// Write Vector4 array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector4Array(Vector4[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector4 value = source[i];
                WriteVector4(value, dest);//write vector4
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write vector4 list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector4List(IList<Vector4> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Vector4 value = source[i];
                WriteVector4(value, dest);//write vector4
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write Quaternion array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteQuaternionArray(Quaternion[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Quaternion value = source[i];
                WriteQuaternion(value, dest);//write Quaternion
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write Quaternion list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteVector4List(IList<Quaternion> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Quaternion value = source[i];
                WriteQuaternion(value, dest);//write Quaternion
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write Color array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteColorArray(Color[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Color value = source[i];
                WriteColor(value, dest);//write color
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write Color list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteColorList(IList<Color> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Color value = source[i];
                WriteColor(value, dest);//write color
                index += 16;
            }
            return index;
        }

        /// <summary>
        /// Write Color32 array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteColor32Array(Color32[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Color32 value = source[i];
                WriteColor32(value, dest);//write color32
                index += 4;
            }
            return index;
        }

        /// <summary>
        /// Write Color32 list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteColor32List(IList<Color32> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            long index = dest.Position;
            WriteInt(length, dest);//write data length
            index += 4;
            for (int i = sourceOffset; i < length; i++)
            {
                Color32 value = source[i];
                WriteColor32(value, dest);//write color32
                index += 4;
            }
            return index;
        }

        /// <summary>
        /// Write String array into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteStringArray(string[] source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            WriteInt(length, dest);//write data length
            for (int i = sourceOffset; i < length; i++)
            {
                string value = source[i];
                WriteString(value, dest);//write string
            }
            return dest.Position;
        }

        /// <summary>
        /// Write String list into stream.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="length">The write data length</param>
        /// <param name="destBuffer"></param>
        /// <param name="destStartIndex">Dest start index.</param>
        /// <returns>The new index.</returns>
        public static long WriteStringList(IList<string> source, int sourceOffset, int length, Stream dest, long? destStartIndex = null)
        {
            if (destStartIndex.HasValue)
            {
                dest.Position = destStartIndex.Value;
            }
            WriteInt(length, dest);//write data length
            for (int i = sourceOffset; i < length; i++)
            {
                string value = source[i];
                WriteString(value, dest);//write string
            }
            return dest.Position;
        }

        /// <summary>
        /// Reads string from buffer, starts at startIndex.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string ReadString(Stream buffer, int? startIndex = null)
        {
            short length = PEUtils.ReadShort(buffer, startIndex);
            byte[] _bytes = new byte[length];
            buffer.Read(_bytes, 0, length);
            string text = Encoding.UTF8.GetString(_bytes);
            return text;
        }

        /// <summary>
        /// 从 buffer 中读取一个数组。
        /// 此方法存在动态内存分配。
        /// </summary>
        /// <param name="sourceBuffer">原数组。</param>
        /// <param name="sourceIndex">原数组的数据起点</param>
        /// <param name="length">输出的目标数组的长度。</param>
        /// <param name="destBuffer">目标数组接收容器。</param>
        /// <param name="destBufferOffset">目标数组接收数据的起点索引。</param>
        public static byte[] ReadByteArray(Stream buffer, int? startIndex = null)
        {
            int arrayLength = ReadInt(buffer, startIndex);
            byte[] ret = new byte[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                ret[i] = (byte)buffer.ReadByte();
            }
            return ret;
        }
    }
}