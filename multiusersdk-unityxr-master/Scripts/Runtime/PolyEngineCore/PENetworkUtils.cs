using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Net.NetworkInformation;
using System.Linq;


namespace Ximmerse.XR.UnityNetworking
{
    /// <summary>
    /// Network utils.
    /// </summary>
    public static class PENetworkUtils
    {
        /// <summary>
        /// Sets socket I/O control.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="onOff"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="retryInterval"></param>
        public static void SetSocketKeepAlive(this Socket socket, bool onOff, float keepAliveTime, float retryInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff ? 1 : 0).CopyTo(buffer, 0);
            int keepAliveTime_Millis = (int)TimeSpan.FromSeconds(keepAliveTime).TotalMilliseconds;
            BitConverter.GetBytes(keepAliveTime_Millis).CopyTo(buffer, 4);

            int retryInterval_Millis = (int)TimeSpan.FromSeconds(retryInterval).TotalMilliseconds;
            BitConverter.GetBytes(retryInterval_Millis).CopyTo(buffer, 8);
            socket.IOControl(IOControlCode.KeepAliveValues, buffer, null);
        }


        /// <summary>
        /// Sets socket I/O control.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="onOff"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="retryInterval"></param>
        public static void SetSocketKeepAlive(this TcpClient tcpClient, bool onOff, float keepAliveTime, float retryInterval)
        {
            try
            {
                byte[] buffer = new byte[12];
                BitConverter.GetBytes(onOff ? 1 : 0).CopyTo(buffer, 0);
                int keepAliveTime_Millis = (int)TimeSpan.FromSeconds(keepAliveTime).TotalMilliseconds;
                BitConverter.GetBytes(keepAliveTime_Millis).CopyTo(buffer, 4);

                int retryInterval_Millis = (int)TimeSpan.FromSeconds(retryInterval).TotalMilliseconds;
                BitConverter.GetBytes(retryInterval_Millis).CopyTo(buffer, 8);
                tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, buffer, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Sets socket I/O control.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="onOff"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="retryInterval"></param>
        public static void SetSocketKeepAlive(this TcpListener listener, bool onOff, float keepAliveTime, float retryInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff ? 1 : 0).CopyTo(buffer, 0);
            int keepAliveTime_Millis = (int)TimeSpan.FromSeconds(keepAliveTime).TotalMilliseconds;
            BitConverter.GetBytes(keepAliveTime_Millis).CopyTo(buffer, 4);

            int retryInterval_Millis = (int)TimeSpan.FromSeconds(retryInterval).TotalMilliseconds;
            BitConverter.GetBytes(retryInterval_Millis).CopyTo(buffer, 8);
            listener.Server.IOControl(IOControlCode.KeepAliveValues, buffer, null);
        }

        /// <summary>
        /// Get local IP address
        /// </summary>
        /// <returns></returns>
        public static bool GetLocalIP(out IPAddress localIP)
        {
            int selfTestPort = 65505;
            const string selfTestIP = "8.8.8.8";
            int errorCount = 0;
        retry:
            selfTestPort += 1;
            if (errorCount < 3)
            {
                try
                {
                    //gets from router:
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect(selfTestIP, selfTestPort);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        localIP = endPoint.Address;
                        //Debug.Log(localIP);
                        return true;
                    }
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Debug.LogException(e);
                    errorCount++;
                    //为了避免port 被占用的问题， 当error count < 3 的时候， 重试
                    if (errorCount < 3)
                    {
                        Debug.LogFormat("Retry router get local IP : {0}", errorCount);
                        goto retry;
                    }
                }
                catch (System.Exception exc)
                {
                    Debug.LogException(exc);
                }
            }

            List<IPAddress> addressIPv4 = new List<IPAddress>();

            try
            {
                IPHostEntry host = null;
                string localIPAddress = string.Empty;
                host = Dns.GetHostEntry(Dns.GetHostName());
                for (int i = 0, hostAddressListLength = host.AddressList.Length; i < hostAddressListLength; i++)
                {
                    var ip = host.AddressList[i];
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addressIPv4.Add(ip);
                    }
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
                localIP = IPAddress.None;
                return false;
            }

            if (addressIPv4.Count == 0)
            {
                localIP = IPAddress.None;
                return false;
            }
            //only 1 internetwork IP:
            if(addressIPv4.Count == 1)
            {
                localIP = addressIPv4[0];
                return true;
            }
            //Ugly hard code :
            //For iOS, it could return more than 1 ipv4 internetwork address,
            //which could have invalid address like 169.254.29.25,
            //so we do a filter, and return most common "192.xxx" IP at first
            //if the common 192.xx ip not exists, than return following the order.
            IPAddress ip192 = addressIPv4.FirstOrDefault(x => x.ToString().StartsWith("192", StringComparison.OrdinalIgnoreCase));
            if (ip192 != null)
            {
                localIP = ip192;
                return true;
            }
            else
            {
                localIP = addressIPv4[0];
                return true;
            }
        }
        /// <summary>
        /// Get subnet broadcasting IP address.
        /// </summary>
        /// <param name="localIP"></param>
        /// <param name="subnetBroadcastIP"></param>
        public static void GetSubnetBroadcastAddress(IPAddress localIP, out IPAddress subnetBroadcastIP)
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in interfaces)
                {
                    UnicastIPAddressInformationCollection unicastAddress = adapter.GetIPProperties().UnicastAddresses;
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in unicastAddress)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (localIP.Equals(unicastIPAddressInformation.Address))
                            {
                                subnetBroadcastIP = unicastIPAddressInformation.IPv4Mask;
                            }
                        }
                    }
                }

                subnetBroadcastIP = IPAddress.None;
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);

                //直接 Hard code subnet mask的最后一位:
                byte[] bytes = localIP.GetAddressBytes();
                bytes[bytes.Length - 1] = 255;//最后一位设置为 255, 假设 subnet mask = 255.255.255.0
                subnetBroadcastIP = new IPAddress(bytes);
            }

        }
    }
}
