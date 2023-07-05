using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Ti-network message.
    /// </summary>
    public abstract class TiNetMessage
    {
        /// <summary>
        /// The unique self-increased ID.
        /// </summary>
        internal int ID;

        /// <summary>
        /// Start index of the message buffer inside the total buffer block. Will be reset each time before sent.
        /// </summary>
        internal int StartIndex;

        /// <summary>
        /// Current index of the message operation cursor.
        /// </summary>
        internal int CurrentIndex;

        /// <summary>
        /// The total pass read/write buffer count.
        /// </summary>
        internal int TotalPassBufferLength;

        internal byte[] Buffer = null;

        /// <summary>
        /// Is the tinet message sent ?
        /// </summary>
        public bool IsSent
        {
            get; internal set;
        }

        /// <summary>
        /// Serialize message content to buffer, starts at StartIndex.
        /// Return the total write byte count.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="StartIndex"></param>
        /// <returns>总共写出的数据长度.</returns>
        public int Serialize(byte[] buffer, int StartIndex)
        {
            Buffer = buffer;
            this.StartIndex = StartIndex;
            CurrentIndex = StartIndex;
            OnSerialize();
            return TotalPassBufferLength;
        }

        /// <summary>
        /// Deserialize the message, starts at StartIndex.
        /// Return the total read byte count.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="StartIndex"></param>
        /// <returns>总共读取的数据长度.</returns>
        public int Deserialize(byte[] buffer, int StartIndex)
        {
            Buffer = buffer;
            this.StartIndex = StartIndex;
            CurrentIndex = StartIndex;
            OnDeserialize();
            return TotalPassBufferLength;
        }

        /// <summary>
        /// Implment on serialized in child class.
        /// </summary>
        public abstract void OnSerialize();

        /// <summary>
        /// Implment on deserialized in child class.
        /// </summary>
        public abstract void OnDeserialize();

        /// <summary>
        /// Resets this message instance.
        /// Reset is called when the message is returned to pool.
        /// </summary>
        internal virtual void Reset()
        {
            OnReset();
            StartIndex = 0;
            CurrentIndex = 0;
            TotalPassBufferLength = 0;
            Buffer = null;
        }

        protected virtual void OnReset()
        {

        }

        #region protected message APIs - Write Value types

        /// <summary>
        /// Writes a bool into message buffer
        /// </summary>
        /// <param name="Bool"></param>
        protected void WriteBool(bool Bool)
        {
            PEUtils.WriteByte(Bool ? (byte)1 : (byte)0, Buffer, CurrentIndex);
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
        }

        /// <summary>
        /// Writes a byte into message buffer.
        /// </summary>
        /// <param name="Byte"></param>
        protected void WriteByte(byte Byte)
        {
            int Index = PEUtils.WriteByte(Byte, Buffer, CurrentIndex);
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
        }

        /// <summary>
        /// Writes a signed byte into message buffer.
        /// </summary>
        /// <param name="Byte"></param>
        protected void WriteSbyte(sbyte Byte)
        {
            PEUtils.WriteByte((byte)Byte, Buffer, CurrentIndex);
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
        }

        /// <summary>
        /// Writes a ushort into message buffer.
        /// </summary>
        /// <param name="UShort"></param>
        protected void WriteUShort(ushort UShort)
        {
            PEUtils.WriteShort((short)UShort, Buffer, CurrentIndex);
            TotalPassBufferLength += 2;
            CurrentIndex += 2;
        }

        /// <summary>
        /// Writes a short into message buffer.
        /// </summary>
        /// <param name="Short"></param>
        protected void WriteShort(short Short)
        {
            PEUtils.WriteShort(Short, Buffer, CurrentIndex);
            TotalPassBufferLength += 2;
            CurrentIndex += 2;
        }

        /// <summary>
        /// Writes an interger into message buffer.
        /// </summary>
        /// <param name="Interger"></param>
        protected void WriteInt(int Interger)
        {
            int Index = PEUtils.WriteInt(Interger, Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
        }

        /// <summary>
        /// Writes an unsigned interger into message buffer.
        /// </summary>
        /// <param name="Interger"></param>
        protected void WriteUInt(uint Interger)
        {
            int Index = PEUtils.WriteInt((int)Interger, Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
        }

        /// <summary>
        /// Writes a float into message buffer.
        /// </summary>
        /// <param name="Single"></param>
        protected void WriteFloat(float Single)
        {
            int Index = PEUtils.WriteFloat(Single, Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
        }

        /// <summary>
        /// Writes a long into message buffer
        /// </summary>
        /// <param name="Long"></param>
        protected void WriteLong(long Long)
        {
            int Index = PEUtils.WriteULong((ulong)Long, Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
        }

        /// <summary>
        /// Writes a ulong into message buffer
        /// </summary>
        /// <param name="uLong"></param>
        protected void WriteULong(ulong uLong)
        {
            int Index = PEUtils.WriteULong(uLong, Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
        }

        /// <summary>
        /// Writes a double
        /// </summary>
        /// <param name="Double"></param>
        protected void WriteDouble(double Double)
        {
            PEUtils.WriteDouble(Double, Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
        }

        /// <summary>
        /// Writes a decimal into message buffer
        /// </summary>
        /// <param name="Decimal"></param>
        protected void WriteDecimal(decimal Decimal)
        {
            int Index = PEUtils.WriteDecimal((ulong)Decimal, Buffer, CurrentIndex);
            TotalPassBufferLength += 16;//decimal = 2 个 long
            CurrentIndex += 16;
        }

        /// <summary>
        /// Writes a vector2 into message buffer
        /// </summary>
        /// <param name="vector2"></param>
        protected void WriteVector2(Vector2 vector2)
        {
            int Index = PEUtils.WriteVector2(vector2, Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
        }

        /// <summary>
        /// Writes a vector3 into message buffer
        /// </summary>
        /// <param name="vector3"></param>
        protected void WriteVector3(Vector3 vector3)
        {
            int Index = PEUtils.WriteVector3(vector3, Buffer, CurrentIndex);
            TotalPassBufferLength += 12;
            CurrentIndex += 12;
        }

        /// <summary>
        /// Writes a vector4 into message buffer
        /// </summary>
        /// <param name="vector4"></param>
        protected void WriteVector4(Vector4 vector4)
        {
            int Index = PEUtils.WriteVector4(vector4, Buffer, CurrentIndex);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
        }

        /// <summary>
        /// Writes a vector4 into message buffer
        /// </summary>
        /// <param name="quaternion"></param>
        protected void WriteQuaternion(Quaternion quaternion)
        {
            int Index = PEUtils.WriteQuaternion(quaternion, Buffer, CurrentIndex);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
        }

        /// <summary>
        /// Writes a vector2int into message buffer
        /// </summary>
        /// <param name="vector2int"></param>
        protected void WriteVector2Int(Vector2Int vector2int)
        {
            int Index = PEUtils.WriteInt(vector2int.x, Buffer, CurrentIndex);
            Index = PEUtils.WriteInt(vector2int.y, Buffer, Index);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
        }

        /// <summary>
        /// Writes a vector2int into message buffer
        /// </summary>
        /// <param name="vector3int"></param>
        protected void WriteVector3Int(Vector3Int vector3int)
        {
            int Index = PEUtils.WriteInt(vector3int.x, Buffer, CurrentIndex);
            Index = PEUtils.WriteInt(vector3int.y, Buffer, Index);
            Index = PEUtils.WriteInt(vector3int.z, Buffer, Index);
            TotalPassBufferLength += 12;
            CurrentIndex += 12;
        }

        /// <summary>
        /// Writes a vector4int into message buffer
        /// </summary>
        /// <param name="vector4int"></param>
        protected void WriteVector4Int(pVector4Int vector4int)
        {
            int Index = PEUtils.WriteInt(vector4int.x, Buffer, CurrentIndex);
            Index = PEUtils.WriteInt(vector4int.y, Buffer, Index);
            Index = PEUtils.WriteInt(vector4int.z, Buffer, Index);
            Index = PEUtils.WriteInt(vector4int.w, Buffer, Index);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
        }

        /// <summary>
        /// Writes a string into message buffer
        /// </summary>
        /// <param name="text"></param>
        protected void WriteString(string text)
        {
            int NewIndex = PEUtils.WriteString(text, Buffer, CurrentIndex);
            TotalPassBufferLength += NewIndex - CurrentIndex;
            CurrentIndex += NewIndex - CurrentIndex;
        }

        /// <summary>
        /// Writes a buffer into message buffer
        /// </summary>
        /// <param name="buffer"></param>
        protected void WriteBytes(byte[] buffer)
        {
            PEUtils.WriteShort((short)buffer.Length, Buffer, CurrentIndex);
            Array.ConstrainedCopy(buffer, 0, Buffer, CurrentIndex + 2, buffer.Length);
            TotalPassBufferLength += buffer.Length + 2;
            CurrentIndex += buffer.Length + 2;
        }

        /// <summary>
        /// Writes a buffer into message buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="startIndex"></param>
        protected void WriteBytes(byte[] buffer, int startIndex, int length)
        {
            PEUtils.WriteShort((short)length, Buffer, CurrentIndex);
            Array.ConstrainedCopy(buffer, startIndex, Buffer, CurrentIndex + 2, length);
            TotalPassBufferLength += length + 2;
            CurrentIndex += length + 2;
        }

        /// <summary>
        /// Writes a buffer into message buffer
        /// </summary>
        /// <param name="buffer"></param>
        protected void WriteByteArray(List<byte> buffer)
        {
            int Index = PEUtils.WriteShort((short)buffer.Count, Buffer, CurrentIndex);
            for (int i = 0; i < buffer.Count; i++)
            {
                Buffer[Index++] = buffer[i];
            }
            TotalPassBufferLength += buffer.Count + 2;
            CurrentIndex += buffer.Count + 2;
        }

        /// <summary>
        /// Writes a bool list into message buffer
        /// </summary>
        /// <param name="boolList"></param>
        protected void WriteBoolArray(bool[] boolArray)
        {
            int index = PEUtils.WriteShort((short)boolArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < boolArray.Length; i++)
            {
                index = PEUtils.WriteByte(boolArray[i] ? (byte)1 : (byte)0, Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a bool list into message buffer
        /// </summary>
        /// <param name="boolList"></param>
        protected void WriteBoolList(List<bool> boolList)
        {
            int index = PEUtils.WriteShort((short)boolList.Count, Buffer, CurrentIndex);
            for (int i = 0; i < boolList.Count; i++)
            {
                index = PEUtils.WriteByte(boolList[i] ? (byte)1 : (byte)0, Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a short array into message buffer
        /// </summary>
        /// <param name="shortArray"></param>
        protected void WriteShortArray(short[] shortArray)
        {
            int index = PEUtils.WriteShort((short)shortArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < shortArray.Length; i++)
            {
                index = PEUtils.WriteShort(shortArray[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a short array into message buffer
        /// </summary>
        /// <param name="shortList"></param>
        protected void WriteShortList(List<short> shortList)
        {
            int index = PEUtils.WriteShort((short)shortList.Count, Buffer, CurrentIndex);
            for (int i = 0; i < shortList.Count; i++)
            {
                index = PEUtils.WriteShort(shortList[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a int array into message buffer
        /// </summary>
        /// <param name="intArray"></param>
        protected void WriteIntArray(int[] intArray)
        {
            int index = PEUtils.WriteShort((short)intArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < intArray.Length; i++)
            {
                index = PEUtils.WriteInt(intArray[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a int list into message buffer
        /// </summary>
        /// <param name="intList"></param>
        protected void WriteIntList(List<int> intList)
        {
            int index = PEUtils.WriteShort((short)intList.Count, Buffer, CurrentIndex);
            for (int i = 0; i < intList.Count; i++)
            {
                index = PEUtils.WriteInt(intList[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }


        /// <summary>
        /// Writes a float array into message buffer
        /// </summary>
        /// <param name="floatArray"></param>
        protected void WriteFloatArray(float[] floatArray)
        {
            int index = PEUtils.WriteShort((short)floatArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < floatArray.Length; i++)
            {
                index = PEUtils.WriteFloat(floatArray[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a int list into message buffer
        /// </summary>
        /// <param name="floatList"></param>
        protected void WriteFloatList(List<float> floatList)
        {
            int index = PEUtils.WriteShort((short)floatList.Count, Buffer, CurrentIndex);
            for (int i = 0; i < floatList.Count; i++)
            {
                index = PEUtils.WriteFloat(floatList[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }


        /// <summary>
        /// Writes a long array into message buffer
        /// </summary>
        /// <param name="longArray"></param>
        protected void WriteLongArray(long[] longArray)
        {
            int index = PEUtils.WriteShort((short)longArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < longArray.Length; i++)
            {
                index = PEUtils.WriteULong((ulong)longArray[i], Buffer, index);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
        }

        /// <summary>
        /// Writes a vector3 array into message buffer
        /// </summary>
        /// <param name="vector3Array"></param>
        protected void WriteVector3Array(Vector3[] vector3Array)
        {
            int Index = PEUtils.WriteShort((short)vector3Array.Length, Buffer, CurrentIndex);
            for (int i = 0; i < vector3Array.Length; i++)
            {
                Index = PEUtils.WriteVector3(vector3Array[i], Buffer, Index);
            }
            TotalPassBufferLength += Index - CurrentIndex;
            CurrentIndex += Index - CurrentIndex;
        }

        /// <summary>
        /// Writes a quaternion array into message buffer
        /// </summary>
        /// <param name="quaternionArray"></param>
        protected void WriteQuaternionArray(Quaternion[] quaternionArray)
        {
            int Index = PEUtils.WriteShort((short)quaternionArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < quaternionArray.Length; i++)
            {
                Index = PEUtils.WriteQuaternion(quaternionArray[i], Buffer, Index);
            }
            TotalPassBufferLength += Index - CurrentIndex;
            CurrentIndex += Index - CurrentIndex;
        }

        protected void WritePose(Pose pose)
        {
            this.WriteVector3(pose.position);
            this.WriteQuaternion(pose.rotation);
            TotalPassBufferLength += 12 + 16;
            CurrentIndex += 12 + 16;
        }

        /// <summary>
        /// Writes a string array into message buffer
        /// </summary>
        /// <param name="stringArray"></param>
        protected void WriteStringArray(string[] stringArray)
        {
            int Index = PEUtils.WriteShort((short)stringArray.Length, Buffer, CurrentIndex);
            for (int i = 0; i < stringArray.Length; i++)
            {
                Index = PEUtils.WriteString(stringArray[i], Buffer, Index);
            }
            TotalPassBufferLength += Index - CurrentIndex;
            CurrentIndex += Index - CurrentIndex;
        }

        /// <summary>
        /// Writes a string list into message buffer
        /// </summary>
        /// <param name="stringList"></param>
        protected void WriteStringList(List<string> stringList)
        {
            int Index = PEUtils.WriteShort((short)stringList.Count, Buffer, CurrentIndex);
            for (int i = 0; i < stringList.Count; i++)
            {
                Index = PEUtils.WriteString(stringList[i], Buffer, Index);
            }
            TotalPassBufferLength += Index - CurrentIndex;
            CurrentIndex += Index - CurrentIndex;
        }

        #endregion

        #region protected message APIs - Read value types

        /// <summary>
        /// Reads a boolean value from message buffer.
        /// </summary>
        /// <returns></returns>
        protected bool ReadBool()
        {
            byte ret = Buffer[CurrentIndex];
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
            return ret == 1;
        }

        /// <summary>
        /// Reads byte from message buffer.
        /// </summary>
        /// <param name="Byte"></param>
        protected byte ReadByte()
        {
            byte ret = Buffer[CurrentIndex];
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
            return ret;
        }

        /// <summary>
        /// Reads sbyte from message buffer.
        /// </summary>
        /// <param name="Byte"></param>
        protected sbyte ReadSbyte()
        {
            sbyte ret = (sbyte)Buffer[CurrentIndex];
            TotalPassBufferLength += 1;
            CurrentIndex += 1;
            return ret;
        }


        /// <summary>
        /// Reads a ushort from message buffer.
        /// </summary>
        protected ushort ReadUShort()
        {
            ushort ret = (ushort)PEUtils.ReadShort(Buffer, CurrentIndex);
            TotalPassBufferLength += 2;
            CurrentIndex += 2;
            return ret;
        }

        /// <summary>
        /// Writes a short from message buffer.
        /// </summary>
        protected short ReadShort()
        {
            short ret = PEUtils.ReadShort(Buffer, CurrentIndex);
            TotalPassBufferLength += 2;
            CurrentIndex += 2;
            return ret;
        }

        /// <summary>
        /// Reads an interger from message buffer.
        /// </summary>
        protected int ReadInt()
        {
            int ret = PEUtils.ReadInt(Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
            return ret;
        }

        /// <summary>
        /// Reads an unsigned interger from message buffer.
        /// </summary>
        protected uint ReadUInt()
        {
            uint ret = (uint)PEUtils.ReadInt(Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
            return ret;
        }

        /// <summary>
        /// Reads a float from message buffer.
        /// </summary>
        protected float ReadFloat()
        {
            float ret = PEUtils.ReadFloat(Buffer, CurrentIndex);
            TotalPassBufferLength += 4;
            CurrentIndex += 4;
            return ret;
        }

        /// <summary>
        /// Reads a long from message buffer.
        /// </summary>
        protected long ReadLong()
        {
            long ret = (long)PEUtils.ReadUInt64(Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
            return ret;
        }

        /// <summary>
        /// Reads an ulong from message buffer.
        /// </summary>
        protected ulong ReadUlong()
        {
            ulong ret = (ulong)PEUtils.ReadUInt64(Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
            return ret;
        }

        /// <summary>
        /// Reads a double from message buffer
        /// </summary>
        /// <returns></returns>
        protected double ReadDouble()
        {
            double ret = PEUtils.ReadDouble(Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
            return ret;
        }

        /// <summary>
        /// Reads a decimal from message buffer.
        /// </summary>
        protected decimal ReadDecimal()
        {
            decimal ret = PEUtils.ReadDecimal(Buffer, CurrentIndex);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
            return ret;
        }

        /// <summary>
        /// Reads a pose from message buffer.
        /// </summary>
        /// <returns></returns>
        protected Pose ReadPose()
        {
            var p = new Pose()
            {
                position = this.ReadVector3(),
                rotation = this.ReadQuaternion(),
            };
            TotalPassBufferLength += 12 + 16;
            CurrentIndex += 12 + 16;
            return p;
        }

        /// <summary>
        /// Reads a vector2 from message buffer.
        /// </summary>
        protected Vector2 ReadVector2()
        {
            Vector2 ret = PEUtils.ReadVector2(Buffer, CurrentIndex);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
            return ret;
        }

        /// <summary>
        /// Reads a vector3 from message buffer.
        /// </summary>
        protected Vector3 ReadVector3()
        {
            Vector3 ret = PEUtils.ReadVector3(Buffer, CurrentIndex);
            TotalPassBufferLength += 12;
            CurrentIndex += 12;
            return ret;
        }

        /// <summary>
        /// Reads a vector4 from message buffer.
        /// </summary>
        protected Vector2 ReadVector4()
        {
            Vector4 ret = PEUtils.ReadVector4(Buffer, CurrentIndex);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
            return ret;
        }


        /// <summary>
        /// Reads a quaternion from message buffer.
        /// </summary>
        protected Quaternion ReadQuaternion()
        {
            Quaternion ret = PEUtils.ReadQuaternion(Buffer, CurrentIndex);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
            return ret;
        }

        /// <summary>
        /// Reads a vector2int from message buffer.
        /// </summary>
        protected Vector2Int ReadVector2Int()
        {
            int x = PEUtils.ReadInt(Buffer, CurrentIndex);
            int y = PEUtils.ReadInt(Buffer, CurrentIndex + 4);
            TotalPassBufferLength += 8;
            CurrentIndex += 8;
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Reads a vector3int from message buffer.
        /// </summary>
        protected Vector3Int ReadVector3Int()
        {
            int x = PEUtils.ReadInt(Buffer, CurrentIndex);
            int y = PEUtils.ReadInt(Buffer, CurrentIndex + 4);
            int z = PEUtils.ReadInt(Buffer, CurrentIndex + 8);
            TotalPassBufferLength += 12;
            CurrentIndex += 12;
            return new Vector3Int(x, y, z);
        }

        /// <summary>
        /// Reads a vector4int from message buffer.
        /// </summary>
        protected pVector4Int ReadVector4Int()
        {
            int x = PEUtils.ReadInt(Buffer, CurrentIndex);
            int y = PEUtils.ReadInt(Buffer, CurrentIndex + 4);
            int z = PEUtils.ReadInt(Buffer, CurrentIndex + 8);
            int w = PEUtils.ReadInt(Buffer, CurrentIndex + 12);
            TotalPassBufferLength += 16;
            CurrentIndex += 16;
            return new pVector4Int(x, y, z, w);
        }

        /// <summary>
        /// Reads a string from message buffer.
        /// </summary>
        protected string ReadString()
        {
            string ret = PEUtils.ReadString(Buffer, out int EndIndex, CurrentIndex);
            TotalPassBufferLength += EndIndex - CurrentIndex;
            CurrentIndex += EndIndex - CurrentIndex;
            return ret;
        }

        /// <summary>
        /// Reads a buffer from message buffer
        /// </summary>
        protected byte[] ReadBytes()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            byte[] bytes = new byte[ArrayLength];
            Array.ConstrainedCopy(Buffer, CurrentIndex + 2, bytes, 0, ArrayLength);
            TotalPassBufferLength += ArrayLength + 2;
            CurrentIndex += ArrayLength + 2;
            return bytes;
        }


        /// <summary>
        /// Reads a buffer from message buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>the return buffer count.</returns>
        protected int ReadBytes(byte[] buffer)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            Array.ConstrainedCopy(Buffer, CurrentIndex + 2, buffer, 0, ArrayLength);
            TotalPassBufferLength += ArrayLength + 2;
            CurrentIndex += ArrayLength + 2;
            return ArrayLength;
        }

        /// <summary>
        /// Reads a bool list from message buffer.
        /// </summary>
        /// <param name="boolList">The bool list</param>
        /// <returns>Read count.</returns>
        protected int ReadBools(List<bool> boolList)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                boolList.Add(Buffer[index++] == 0 ? false : true);
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return ArrayLength;
        }

        /// <summary>
        /// Reads a short array from message buffer
        /// </summary>
        protected short[] ReadShorts()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            short[] shorts = new short[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                shorts[i] = PEUtils.ReadShort(Buffer, index);
                index += 2;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return shorts;
        }

        /// <summary>
        /// Reads a short list from message buffer.
        /// </summary>
        /// <param name="shortList">The short list</param>
        /// <returns>Read count.</returns>
        protected int ReadShorts(List<short> shortList)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                shortList.Add(PEUtils.ReadShort(Buffer, index));
                index += 2;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return ArrayLength;
        }

        /// <summary>
        /// Reads a int array from message buffer
        /// </summary>
        protected int[] ReadInts()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int[] ints = new int[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                ints[i] = PEUtils.ReadInt(Buffer, index);
                index += 4;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return ints;
        }

        /// <summary>
        /// Reads a int list from message buffer.
        /// </summary>
        /// <param name="intList">The int list</param>
        /// <returns>Read count.</returns>
        protected int ReadInts(List<int> intList)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                intList.Add(PEUtils.ReadInt(Buffer, index));
                index += 4;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return ArrayLength;
        }


        /// <summary>
        /// Reads a float array from message buffer
        /// </summary>
        protected float[] ReadFloats()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            float[] floats = new float[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                floats[i] = PEUtils.ReadFloat(Buffer, index);
                index += 4;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return floats;
        }

        /// <summary>
        /// Reads a float list from message buffer.
        /// </summary>
        /// <param name="floatList">The float list</param>
        /// <returns>Read count.</returns>
        protected int ReadFloats(List<float> floatList)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                floatList.Add(PEUtils.ReadFloat(Buffer, index));
                index += 4;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return ArrayLength;
        }

        /// <summary>
        /// Reads a long array from message buffer
        /// </summary>
        protected long[] ReadLongs()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            long[] longs = new long[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                longs[i] = (long)PEUtils.ReadUInt64(Buffer, index);
                index += 8;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return longs;
        }

        /// <summary>
        /// Reads a vector2 array from message buffer
        /// </summary>
        protected Vector2[] ReadVector2s()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            Vector2[] vector2s = new Vector2[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                vector2s[i] = PEUtils.ReadVector2(Buffer, index);
                index += 8;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return vector2s;
        }

        /// <summary>
        /// Reads a vector3 array from message buffer
        /// </summary>
        protected Vector3[] ReadVector3s()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            Vector3[] vector3s = new Vector3[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                vector3s[i] = PEUtils.ReadVector3(Buffer, index);
                index += 12;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return vector3s;
        }

        /// <summary>
        /// Reads a quaternion array from message buffer
        /// </summary>
        protected Quaternion[] ReadQuaternions()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            Quaternion[] quaternions = new Quaternion[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                quaternions[i] = PEUtils.ReadQuaternion(Buffer, index);
                index += 16;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return quaternions;
        }

        /// <summary>
        /// Reads a string array from message buffer
        /// </summary>
        protected string[] ReadStrings()
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            string[] strings = new string[ArrayLength];
            int index = CurrentIndex + 2;
            for (int i = 0; i < ArrayLength; i++)
            {
                strings[i] = PEUtils.ReadString(Buffer, out int endindex, index);
                index += endindex - index;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return strings;
        }

        /// <summary>
        /// Reads a string array from message buffer and insert into list.
        /// </summary>
        protected int ReadStringsToList(List<String> strings)
        {
            short ArrayLength = PEUtils.ReadShort(Buffer, CurrentIndex);
            int index = CurrentIndex + 2;
            int readCount = 0;
            for (int i = 0; i < ArrayLength; i++)
            {
                strings.Add(PEUtils.ReadString(Buffer, out int endindex, index));
                index += endindex - index;
                readCount++;
            }
            TotalPassBufferLength += index - CurrentIndex;
            CurrentIndex += index - CurrentIndex;
            return readCount;
        }

        #endregion
    }
}