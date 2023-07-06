using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
	/// network channel represents a network behaviour that can receive data.
	/// </summary>
	public abstract class NetworkChannel : MonoBehaviour
	{
		/// <summary>
		/// Event : on receive data and flush all the data at one call
		/// </summary>
		public event Action<IEnumerable<NetworkDataPacket>> OnFlushData;

		/// <summary>
		/// The on receive data callback.
		/// Note : if socket receive more than one packet, this callback could be invoked multiple time in one frame.
		/// </summary>
		public event Action<NetworkDataPacket> OnReceiveData = null;
		

		/// <summary>
		/// Invoke on client flush received data.
		/// </summary>
		/// <param name="packets"></param>
		protected void InvokeOnFlushData(IEnumerable<NetworkDataPacket> packets)
		{
			try
			{
				OnFlushData?.Invoke(packets);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}


		/// <summary>
		/// Invoke on client receive a data packet.
		/// This event is call for every data packet received.
		/// </summary>
		/// <param name="packet"></param>
		protected void InvokeOnReceiveData(NetworkDataPacket packet)
		{
			try
			{
				OnReceiveData?.Invoke(packet);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

	}
}