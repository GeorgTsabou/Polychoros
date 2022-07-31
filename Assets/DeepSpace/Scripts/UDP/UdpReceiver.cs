using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DeepSpace.Udp
{
	public class UdpReceiver : UdpBase
	{
		public delegate void ReceivedMessageDelegate(byte[] messageBytes, IPAddress senderIP);

		private event ReceivedMessageDelegate OnReceivedMessage;

		// If this is true, the UDP listener will listen to any IP.
		// If this is false, the UDP listener will only listen to the "_remoteIpAddress".
		[SerializeField]
		private bool _listenToAllAdresses = true;
		[SerializeField, Tooltip("Disable this for local testing.")]
		private bool _exclusiveAddressUse = true;
		[SerializeField]
		private bool _listenToMulticast = false;

		private IPAddress _remoteIp = null;

		public bool ListenToAllAdresses
		{
			get { return _listenToAllAdresses; }
			set { _listenToAllAdresses = value; }
		}

		public bool ExclusiveAddressUse
		{
			get { return _exclusiveAddressUse; }
			set { _exclusiveAddressUse = value; }
		}

		public bool ListenToMulticast
		{
			get { return _listenToMulticast; }
			set { _listenToMulticast = value; }
		}

		// This should be called from outside. Without calling this, the receiver will not receive anything.
		public void ActivateReceiver()
		{
			EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_socket.Blocking = false;

			if (_exclusiveAddressUse == false)
			{
				// This is helpful for local testing, e.g. Pharus Client on the same machine as this application sends to the same port, where this application listens for it.
				_socket.ExclusiveAddressUse = false;
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			}

			_socket.Bind(ipEndPoint);

			if (_listenToMulticast == true)
			{
				// This is equivalent to UdpClient.JoinMulticastGroup(multicastGroupAddress);
				_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(IpAddress)));
				_socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
			}
		}

		public void ActivateReceiver(int port)
		{
			Port = port;

			ActivateReceiver();
		}

		public void ActivateReceiver(int port, string ipAddress, bool listenToAllAdresses = false)
		{
			Port = port;
			IpAddress = ipAddress;
			ListenToAllAdresses = listenToAllAdresses;

			ActivateReceiver();
		}

		public void SubscribeReceiveEvent(ReceivedMessageDelegate subscriber)
		{
			OnReceivedMessage += subscriber;
		}

		public void UnsubscribeReceiveEvent(ReceivedMessageDelegate subscriber)
		{
			OnReceivedMessage -= subscriber;
		}

		private void Update()
		{
			if (_socket != null)
			{
				while (_socket.Available > 0)
				{
					Receive();
				}
			}
		}

		private void Receive()
		{
			try
			{
				byte[] _receiveBuffer = new byte[_socket.Available]; // Read everything, that is available at once!
				EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
				_socket.ReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ref ipEndPoint);

				if (ListenToAllAdresses == false)
				{
					IPAddress connectedAddress = ((IPEndPoint)ipEndPoint).Address;

					if (_remoteIp == null) // Only happens the first time or if _listenToAllAdresses was set to false during runtime.
					{
						_remoteIp = IPAddress.Parse(_remoteIpAddress);
					}

					if (connectedAddress.Equals(_remoteIp) == false)
					{
						Debug.LogWarning("Got network package from unaccepted endpoint (" + connectedAddress.ToString() + "). Expected endpoint is " + _remoteIpAddress + "."
							+ "\nPackage is not going to be read!");
						return;
					}
				}

				if (_receiveBuffer != null && _receiveBuffer.Length > 0) // If we received some bytes:
				{
					// Call each subscriber that bytes have been received:
					foreach (ReceivedMessageDelegate ReceiveCallack in OnReceivedMessage.GetInvocationList())
					{
						try
						{
							ReceiveCallack(_receiveBuffer, ((IPEndPoint)ipEndPoint).Address);
						}
						catch (System.Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
			}
			catch (Exception receiveException)
			{
				Debug.LogException(receiveException);
			}
		}
	}
}