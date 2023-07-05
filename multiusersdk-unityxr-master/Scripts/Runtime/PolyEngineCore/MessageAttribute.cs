using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Message channel attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MessageAttribute : PropertyAttribute
    {
        public short MessageCode;

        public MessageAttribute(short messageCode) : base()
        {
            this.MessageCode = messageCode;
        }
    }
}