using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
namespace Ximmerse.XR.UnityNetworking
{
    public interface I_FileInfo
    {
        string fileName
        {
            get;
        }

        IReadOnlyList<byte> content
        {
            get;
        }
    }

    public struct TiNetFilePacket
    {
        public int index;
        public NativeArray<byte> data;
    }

    internal class TiNetFileInfo : I_FileInfo, IDisposable, IComparer<TiNetFilePacket>
    {
        public string FileName;

        public int PacketCount;

        public List<TiNetFilePacket> FilePackets = new List<TiNetFilePacket>();

        public List<byte> Content = new List<byte>();

        public bool IsReady;
        private bool disposedValue;

        public string fileName => FileName;

        public IReadOnlyList<byte> content => Content;

        public int TotalBytes
        {
            get; private set;
        }

        public void AddFilePacket(int index, byte[] bytes)
        {
            FilePackets.Add(new TiNetFilePacket()
            {
                data = new NativeArray<byte>(bytes, Allocator.TempJob),
                index = index,
            });
            FilePackets.Sort(this);
            TotalBytes += bytes.Length;
        }

        public void DoneTransport()
        {
            for (int i = 0; i < FilePackets.Count; i++)
            {
                TiNetFilePacket p = FilePackets[i];
                Content.AddRange(p.data);
            }
        }

        public int Compare(TiNetFilePacket x, TiNetFilePacket y)
        {
            return x.index < y.index ? -1 : 1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    foreach (var p in FilePackets)
                    {
                        if (p.data.IsCreated)
                            p.data.Dispose();
                    }
                    FilePackets.Clear();
                }
                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                Content = null;
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~TiNetFileInfo()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}