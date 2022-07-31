using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DeepSpace.MobileControlMessages;

namespace DeepSpace
{
	public class MobileControlTcpManager : MonoBehaviour
	{
		private class UnsentMessages
		{
			// This is the last view message, that has been sent:
			private Message currentViewMessage = null;

			// A generic queue for messages that will be sent in FIFO order:
			public List<Message> MessageList = new List<Message>();

			public void SetShowViewMessage(Message message)
			{
				// Add Message to Queue:
				MessageList.Add(message);

				// Remember it additionally in the currentViewMessage:
				currentViewMessage = message;
			}

			public Message GetLastSentViewMessage()
			{
				return currentViewMessage;
			}

			public bool HasMessagesToSend()
			{
				return (MessageList.Count > 0);
			}

			public void ClearMessage()
			{
				MessageList.Clear();
			}
		}

		public int port = 15551;

		public float timeout = 15.0f;
		private float lastMsgReceived = 0f; // Time since the last time, a message has been received.

		[System.ComponentModel.DefaultValue(false)]
		public bool IsConnected
		{
			get;
			private set;
		}

		private TcpListener _tcpListener = null;
		private Socket _socket = null;

		private UnsentMessages _unsentMessages = new UnsentMessages();
		private Queue<Message> _receivedMessages = new Queue<Message>();

		public Message CurrentViewMessage
		{
			get
			{
				return _unsentMessages.GetLastSentViewMessage();
			}
		}

		private void Start()
		{
			_tcpListener = new TcpListener(IPAddress.Any, port);

			_tcpListener.Start();

			//Debug.Log("Waiting for connection!");
			StartCoroutine(WaitForConnection());
		}

		IEnumerator WaitForConnection()
		{
			while (!_tcpListener.Pending())
			{
				yield return null;
			}

			try
			{
				_socket = _tcpListener.AcceptSocket();

				IsConnected = true;
			}
			catch (System.Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		private void Update()
		{
			if (_socket != null)
			{
				if (SocketConnected(_socket))
				{
					lastMsgReceived += Time.deltaTime;

					//Debug.Log("Before Reading -> Socket.Available: " + _socket.Available);

					// Read all, that waits on the pipeline and parse out all containing messages in this stream:
					List<byte> readByteList = new List<byte>();
					while (_socket.Available > 0)
					{
						byte[] buffer = new byte[1024];
						int bytesRead = _socket.Receive(buffer, buffer.Length, SocketFlags.None);

						System.Array.Resize(ref buffer, bytesRead);

						readByteList.AddRange(buffer);
					}
					if (readByteList.Count > 0)
					{
						// A message has been received, reset lastMsgReceived-timeout:
						lastMsgReceived = 0;

						byte[] buffer = readByteList.ToArray();

						// This buffer might consist of more than one request / message:
						while (buffer != null && buffer.Length > 0)
						{
							//Debug.Log("Buffer size is " + buffer.Length);

							// Read the first message from the buffer:
							Message receivedMsg = Message.GetReceivedMessage(buffer);
							if (receivedMsg != null)
							{
								int msgLength = receivedMsg.UnpackReceivedMessage();

								// Create request bytes that only contain this message
								byte[] request = new byte[msgLength];
								System.Array.Copy(buffer, request, msgLength);

								// Cut away the first message from the buffer:
								List<byte> bufferList = new List<byte>(buffer);
								bufferList.RemoveRange(0, msgLength);
								if (bufferList.Count > 0)
								{
									buffer = bufferList.ToArray();
								}
								else
								{
									buffer = null;
								}

								// Create answer depending to request...
								ProcessRequest(request);
							}
							else
							{
								Debug.LogError("No message could get out of this message stream: " + GetReadableBuffer(buffer)
									+ "\nDid you forget to add a new Receiving or Bidirectional Message Type to the MobileMessages.GetReceivedMessage() Method?");
								buffer = null;
							}
						}
					}

					//Debug.Log("After Reading -> Socket.Available: " + _socket.Available);

					if (_unsentMessages.HasMessagesToSend())
					{
						RemoveSpecifiedDoubles(ref _unsentMessages.MessageList);

						while (_unsentMessages.MessageList.Count > 0)
						{
							Message msg = _unsentMessages.MessageList[0]; // Peek
							_unsentMessages.MessageList.RemoveAt(0); // Dequeue

							_socket.Send(msg.GetMessageBytes(), SocketFlags.None);
						}
					}
				}
				else
				{
					//Debug.Log("Client disconnected... Waiting for new connection!");
					_socket = null;
					IsConnected = false;

					StartCoroutine(WaitForConnection());
				}
			}

			if (lastMsgReceived > timeout)
			{
				// No message has been received for timeout seconds - kill the connection:
				if (_socket != null)
				{
					_socket = null;
					IsConnected = false;

					StartCoroutine(WaitForConnection());
				}
			}

			// Do not buffer sended messages, if no client is connected:
			// Only exception: _unsentMessages keeps the knowledge about the last sent ViewMessage.
			if (IsConnected == false)
			{
				_unsentMessages.ClearMessage();
			}
		}

		// HACK: The Mobile device seems to have problems when more than one Window List Messages are sent.
		// That for, this method removes the doubles:
		private void RemoveSpecifiedDoubles(ref List<Message> messageList)
		{
			// Remove doubles of WindowList Messages -> Only the last shall be sent:
			List<Message> windowListMessages = messageList.FindAll(delegate(Message msg)
			{
				return msg.Command == Command.WINDOW_LIST;
			});

			if (windowListMessages.Count > 1)
			{
				for (int i = 0; i < windowListMessages.Count - 1; ++i)
				{
					messageList.Remove(windowListMessages[i]);
				}
			}
		}

		private string GetReadableBuffer(byte[] buffer)
		{
			string result = "";
			foreach (byte b in buffer)
			{
				result += b.ToString() + ".";
			}
			return result;
		}

		private void ProcessRequest(byte[] request)
		{
			Command command = Message.ReadMessageCommand(request);

			if (command == Command.HEART_BEAT)
			{
				// Answer to heartbeat:
				_unsentMessages.MessageList.Add(new HeartBeatMessage(request));

				//Debug.Log("HeartBeat!");
			}
			else
			{
				Message message = new Message(request, command);
				_receivedMessages.Enqueue(message);
			}
		}

		// From: http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
		// s.Poll returns true if
		//	- connection is closed, reset, terminated or pending (meaning no active connection)
		//	- connection is active and there is data available for reading
		// s.Available returns number of bytes available for reading
		// if both are true:
		//  - there is no data available to read so connection is not active
		private static bool SocketConnected(Socket s)
		{
			bool part1 = s.Poll(1000, SelectMode.SelectRead);
			bool part2 = (s.Available == 0);

			return !(part1 && part2);
		}

		public void AddMessage(Message message)
		{
			_unsentMessages.MessageList.Add(message);
		}

		public void SetViewMessage(Message message)
		{
			_unsentMessages.SetShowViewMessage(message);
		}

		public Message GetNextReceivedMessage()
		{
			if (_receivedMessages.Count > 0)
			{
				return _receivedMessages.Dequeue();
			}

			return null;
		}

		private void OnDestroy()
		{
			_tcpListener.Stop();

			if (_socket != null)
			{
				_socket.Close();
			}
		}
	}
}