using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Ximmerse.XR.Asyncoroutine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// TiNet transport layer - handles low level socket I/O transporting.
    /// </summary>
    internal static class TiNetTransportLayer 
    { 
        /// <summary>
        /// Initialize the transport layer.
        /// </summary>
        public static void Initialize()
        {

        }

        /// <summary>
        /// Push a outgoing data into the outgoing queue.
        /// </summary>
        public static void PushOutgoingMessage()
        {

        }
    }
}