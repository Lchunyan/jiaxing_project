using UnityEngine;
using System;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// 标记一个TiNet 网络回调方法。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TiNetMessageCallbackAttribute : Attribute
    {
        public readonly short MessageCode;

        public TiNetMessageCallbackAttribute(short messageCode)
        {
            MessageCode = messageCode;
        }
    }
}