namespace Ximmerse.XR.UnityNetworking
{
    using System.Collections.Generic;
    using Ximmerse.XR.Reflection;
    using System;
    using UnityEngine;

    /// <summary>
    /// TiNet message instance pool.
    /// </summary>
    internal static class TiNetworkMessagePool
    {
        static Dictionary<short, List<TiNetMessage>> MessagePool = new Dictionary<short, List<TiNetMessage>>();

        /// <summary>
        /// Message type - key map
        /// </summary>
        static Dictionary<Type, short> MessageKeyCodeMap = new Dictionary<Type, short>();

        /// <summary>
        /// Message key - type map.
        /// </summary>
        static Dictionary<short, Type> MessageKeyTypeMap = new Dictionary<short, Type>();

        static Dictionary<Type, MessageAttribute> MessageAttributes = new Dictionary<Type, MessageAttribute>();

        const int kInitMessageCount = 3;

        private const string kErrorFormat = "Receive unknown message of code: {0}";

        public static void Init()
        {
            //Setup message type map:s
            List<Type> MessageTypes = new List<Type>();
            PEReflectionUtility.SearchForChildrenTypes(typeof(TiNetMessage), MessageTypes);
            MessagePool.Clear();
            MessageKeyCodeMap.Clear();
            MessageKeyTypeMap.Clear();
            MessageAttributes.Clear();
            for (int i = 0, MessageTypesCount = MessageTypes.Count; i < MessageTypesCount; i++)
            {
                Type messageType = MessageTypes[i];
                if (messageType.IsAbstract)
                {
                    continue;
                }
                MessageAttribute messageAttribute = messageType.GetAttribute<MessageAttribute>();
                if (messageAttribute == null)
                {
                    Debug.LogErrorFormat("TiNetMessage: {0} missing [MessageAttribute]", messageType.Name);
                    continue;
                }
                short msgKey = messageAttribute.MessageCode;
                if (MessagePool.ContainsKey(msgKey))
                {
                    Debug.LogErrorFormat("Duplicate key registered : {0} msgKey on type: {1}", msgKey, messageType.Name);
                    continue;
                }
                List<TiNetMessage> msgs = new List<TiNetMessage>();
                for (int j = 0; j < kInitMessageCount; j++)
                {
                    var messageInstance = (TiNetMessage)Activator.CreateInstance(messageType);
                    msgs.Add(messageInstance);
                }
                MessagePool.Add(msgKey, msgs);
                MessageKeyCodeMap.Add(messageType, msgKey);
                MessageKeyTypeMap.Add(msgKey, messageType);
                MessageAttributes.Add(messageType, messageAttribute);
            }
        }


        /// <summary>
        /// Gets a pooled TiNetMessage instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="MessageKeyCode"></param>
        /// <returns></returns>
        public static TiNetMessage GetMessage(short MessageKeyCode)
        {
            if (MessagePool.TryGetValue(MessageKeyCode, out List<TiNetMessage> msgList))
            {
                if (msgList.Count == 0)
                {
                    Type msgType = MessageKeyTypeMap[MessageKeyCode];
                    TiNetMessage _msg = (TiNetMessage)Activator.CreateInstance(msgType);
                    return _msg;
                }
                else
                {
                    var _msg = msgList[msgList.Count - 1];
                    msgList.RemoveAt(msgList.Count - 1);
                    _msg.IsSent = false;
                    return _msg;
                }
            }
            else
            {
                Debug.LogErrorFormat(kErrorFormat, MessageKeyCode);
                return null;
            }
        }


        /// <summary>
        /// Gets a pooled TiNetMessage instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="MessageKeyCode"></param>
        /// <returns></returns>
        public static T GetMessage<T>(short MessageKeyCode) where T : TiNetMessage
        {
            var msgList = MessagePool[MessageKeyCode];
            if (msgList.Count == 0)
            {
                T _msg = (T)Activator.CreateInstance<T>();
                return _msg;
            }
            else
            {
                var _msg = msgList[msgList.Count - 1];
                msgList.RemoveAt(msgList.Count - 1);
                _msg.IsSent = false;
                return (T)_msg;
            }
        }

        /// <summary>
        /// Gets a pooled TiNetMessage instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetMessage<T>() where T : Ximmerse.XR.UnityNetworking.TiNetMessage
        {
            short keyCode = MessageKeyCodeMap[typeof(T)];
            var msgList = MessagePool[keyCode];
            if (msgList.Count == 0)
            {
                T _msg = (T)Activator.CreateInstance<T>();
                return _msg;
            }
            else
            {
                var _msg = msgList[msgList.Count - 1];
                msgList.RemoveAt(msgList.Count - 1);
                _msg.IsSent = false;
                return (T)_msg;
            }
        }

        /// <summary>
        /// Returns a message to pool.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="MessageKeyCode"></param>
        /// <param name="msg"></param>
        public static void ReturnMessage<T>(short MessageKeyCode, T msg) where T : TiNetMessage
        {
            msg.Reset();
            var msgList = MessagePool[MessageKeyCode];
            msgList.Add(msg);
        }

        /// <summary>
        /// Gets TiNetwork message attribute.
        /// </summary>
        /// <param name="msgType"></param>
        /// <returns></returns>
        public static MessageAttribute GetAttribute(Type msgType)
        {
            if (MessageAttributes.TryGetValue(msgType, out MessageAttribute attr))
            {
                return attr;
            }
            else return null;
        }
    }
}