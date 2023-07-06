using Ximmerse.XR.UnityNetworking;
using UnityEngine;
using I_TiNetNode = Ximmerse.XR.UnityNetworking.I_TiNetNode;
using TiNet = Ximmerse.XR.UnityNetworking.TiNet;
namespace Ximmerse.XR
{
    /// <summary>
    /// Ti-Net system monob-behaviour.
    /// </summary>
    public class TiNetMonoBehaviour : MonoBehaviour, TiNetMessageHandler
    {

        public I_SyncIdentity SyncIdentity
        {
            get; private set;
        }

        protected virtual void Awake()
        {
            SyncIdentity = GetComponent<I_SyncIdentity>();
            TiNet.OnTiNetStart += TiNet_OnTiNetStart;
            TiNet.OnTiNetStop += TiNet_OnTiNetStop;
            TiNet.OnNodeConnected += TiNet_OnNodeConnected;
            TiNet.OnNodeDisconnected += TiNet_OnNodeDisconnected;
        }

        protected virtual void OnDestroy()
        {
            TiNet.OnTiNetStart -= TiNet_OnTiNetStart;
            TiNet.OnTiNetStop += TiNet_OnTiNetStop;
            TiNet.OnNodeConnected -= TiNet_OnNodeConnected;
            TiNet.OnNodeDisconnected -= TiNet_OnNodeDisconnected;
        }

        /// <summary>
        /// Event : on ti-net node connected.
        /// </summary>
        /// <param name="node"></param>
        protected virtual void TiNet_OnNodeConnected(I_TiNetNode node)
        {

        }

        /// <summary>
        /// Event : on ti-net node disconnected.
        /// </summary>
        /// <param name="node"></param>
        protected virtual void TiNet_OnNodeDisconnected(I_TiNetNode node)
        {
            
        }


        /// <summary>
        /// on network stop
        /// </summary>
        protected virtual void TiNet_OnTiNetStop()
        {

        }

        /// <summary>
        /// on network start
        /// </summary>
        protected virtual void TiNet_OnTiNetStart()
        {

        }
    }
}
