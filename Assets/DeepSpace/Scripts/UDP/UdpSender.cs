using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DeepSpace.Udp
{
	public class UdpSender : UdpBase
	{
		// If this is configured to be more than one, all messages are sent to the following port.
		// The first client gets his messages on the specified port.
		// e.g. if port is 5560, the second client gets the same messages on port 5561, the third one on 5562, and so on.
		//[SerializeField]
		//private int _receivingClientAmount = 1;

		//public int ReceivingClientAmount
		//{
		//	get
		//	{
		//		return _receivingClientAmount;
		//	}
		//	set
		//	{
		//		_receivingClientAmount = value;
		//		InitEndPoints();
		//	}
		//}

		[SerializeField]
		private int _sendRate = 0; // 0 means every frame, else _sendRate calc...

		private const int _maxUDPSize = 0x10000;

		public int SendRatePerSecond
		{
			get
			{
				return _sendRate;
			}
			set
			{
				_sendRate = value;
				InitWaitTillSend();
			}
		}

		private bool _initialized = false;

		public bool IsInitialized
		{
			get { return _initialized; }
		}

		private float _sendTime = 0f;
		private float _waitTillSend = 0f;


		private EndPoint _ipEndPoint;

		private Queue<string> _messages = new Queue<string>();

		private byte[] _dequeuedButNotSent = null;

		// This should be called from outside. Without calling this, the sender will not send anything.
		public void ActivateSender(string ipAddress, int port)
		{
			IpAddress = ipAddress;
			Port = port;

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
			_socket.Blocking = false;

			InitWaitTillSend();

			InitEndPoints();

			_initialized = true;
		}

		private void InitWaitTillSend()
		{
			if (_sendRate == 0)
			{
				_waitTillSend = 0f;
			}
			else
			{
				_waitTillSend = 1f / (float)_sendRate;
			}
		}

		private void InitEndPoints()
		{
			IPAddress ipAddress = IPAddress.Parse(IpAddress);
			_ipEndPoint = new IPEndPoint(ipAddress, Port);
		}

		private void Update()
		{
			if (IsInitialized)
			{
				_sendTime += Time.fixedDeltaTime;

				if (_sendTime >= _waitTillSend)
				{
					_sendTime = 0f; // Reset sendTime.

					Send(); // Send everything in the queue.
				}
			}
		}

		private void Send()
		{
			int currentSendAmount = 0;

			if (_dequeuedButNotSent != null)
			{
				currentSendAmount += _dequeuedButNotSent.Length;
				SendSingleBuffer(_dequeuedButNotSent);
				_dequeuedButNotSent = null;
			}

			while (_messages.Count > 0)
			{
				string message = _messages.Dequeue();

				//Debug.Log("Sending JSON: " + message);

				byte[] sendBuffer = System.Text.Encoding.UTF8.GetBytes(message);

				if ((currentSendAmount + sendBuffer.Length) > _maxUDPSize)
				{
					// Maximal bytes for this frame are reached.
					Debug.Log("[" + gameObject.name + "] UDP Sendbuffer is full. Sending rest of messages next frame. (" + _messages.Count + 1 + " messages left.)");
					_dequeuedButNotSent = sendBuffer;
					break;
				}

				currentSendAmount += sendBuffer.Length;
				SendSingleBuffer(sendBuffer);
			}
		}

		private void SendSingleBuffer(byte[] sendBuffer)
		{
			try
			{
				_socket.SendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, _ipEndPoint);
			}
			catch (Exception sendException)
			{
				Debug.LogException(sendException);
			}
		}

		// Adds a message to the send queue:
		public void AddMessage(string msg)
		{
			_messages.Enqueue(msg);
		}
	}
}