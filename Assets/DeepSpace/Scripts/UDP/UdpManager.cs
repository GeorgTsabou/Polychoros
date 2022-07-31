using UnityEngine;
using System.Collections.Generic;
using System.Net;

namespace DeepSpace.Udp
{
	// Takes care that json data can be shared between wall and floor via network.
	public class UdpManager : MonoBehaviour
	{
		public class ReceivedMessage
		{
			public enum Sender
			{
				WALL,
				FLOOR,
			}

			public ReceivedMessage(string jsonMsg, Sender sender)
			{
				this.jsonMsg = jsonMsg;
				this.sender = sender;
			}

			public string jsonMsg; // The received message.
			public Sender sender; // Where the received message came from.
		}

		[SerializeField]
		private UdpSender _senderToWall;
		[SerializeField]
		private UdpSender _senderToFloor;
		[SerializeField]
		private UdpReceiver _receiver;

		private string _uncompletedMessage = string.Empty; // Used for received json messages, that are not yet delivered completely (e.g. because they are too long).
		private List<ReceivedMessage> _receivedMessages = new List<ReceivedMessage>();

		public UdpSender SenderToWall
		{
			get { return _senderToWall; }
			private set { _senderToWall = value; }
		}

		public UdpSender SenderToFloor
		{
			get { return _senderToFloor; }
			private set { _senderToFloor = value; }
		}

		public UdpReceiver Receiver
		{
			get { return _receiver; }
			private set { _receiver = value; }
		}

		private void Start()
		{
			CmdConfigManager config = CmdConfigManager.Instance;
			if (config == null)
			{
				Debug.LogWarning("Missing CmdConfigManager. Is Main Scene loaded?"
					+ "\nUsing default values from UDP Sender and Receiver components instead of configured values.");
			}

			UdpCmdConfigMgr configMgr = UdpCmdConfigMgr.Instance as UdpCmdConfigMgr;

			// Depending on the own state, this manager does not need a sender to itself, so it will be destroyed.
			// Additionally, the receiver is beeing configured.
			switch (configMgr.applicationType)
			{
				case CmdConfigManager.AppType.WALL:
					int wallUdpPort = configMgr.udpReceivingPort; // This port should be configured depending on application type.
					Receiver.ActivateReceiver(wallUdpPort);
					Destroy(SenderToWall.gameObject);
					SenderToWall = null;
					break;
				case CmdConfigManager.AppType.FLOOR:
					int floorUdpPort = configMgr.udpReceivingPort; // This should be configured depending on application type.
					Receiver.ActivateReceiver(floorUdpPort);
					Destroy(SenderToFloor.gameObject);
					SenderToFloor = null;
					break;
			}

			// All not disabled senders need to be configured and activated:
			if (SenderToWall != null)
			{
				string wallIp = configMgr.udpAddress; // SenderToWall.IpAddress
				int wallUdpPort = configMgr.udpSendingPort; // SenderToWall.Port;
				SenderToWall.ActivateSender(wallIp, wallUdpPort);
			}
			if (SenderToFloor != null)
			{
				string floorIp = configMgr.udpAddress; // SenderToFloor.IpAddress
				int floorUdpPort = configMgr.udpSendingPort; // SenderToFloor.Port
				SenderToFloor.ActivateSender(floorIp, floorUdpPort);
			}

			// Subscribe receiving events:
			Receiver.SubscribeReceiveEvent(OnReceivedMessage);
		}

		private void OnReceivedMessage(byte[] messageBytes, IPAddress senderIP)
		{
			// Check, where the message is from:
			string remoteAddress = senderIP.ToString();
			//Debug.Log("Received Message from Address " + remoteAddress);

			List<string> jsonMessages = GetValidJsonFromBytes(messageBytes);

			List<ReceivedMessage> receivedMessages = new List<ReceivedMessage>();

			if (SenderToWall != null && remoteAddress == SenderToWall.IpAddress)
			{
				// Received message from wall...
				foreach (string msg in jsonMessages)
				{
					receivedMessages.Add(new ReceivedMessage(msg, ReceivedMessage.Sender.WALL));
				}
			}
			else if (SenderToFloor != null && SenderToFloor.IpAddress == remoteAddress)
			{
				// Received message from floor...
				foreach (string msg in jsonMessages)
				{
					receivedMessages.Add(new ReceivedMessage(msg, ReceivedMessage.Sender.FLOOR));
				}
			}
			else
			{
				Debug.LogError("Message came from unknown source. Message will be ignored!");
				return;
			}

			_receivedMessages.AddRange(receivedMessages);
		}

		private List<string> GetValidJsonFromBytes(byte[] jsonBytes)
		{
			List<string> jsonMessages = new List<string>();

			if (jsonBytes != null && jsonBytes.Length > 0)
			{
				string receivedJson = System.Text.Encoding.UTF8.GetString(jsonBytes);
				if (receivedJson.Trim()[0] != '{') // This is not perfect but can prevent making an initial data loss becomming a catastrophy.
				{
					receivedJson = _uncompletedMessage + receivedJson;
				}
				_uncompletedMessage = string.Empty;

				// Check if it is only one or more JSON Messages and check if each JSON Message is valid:
				int startIndex = 0;
				int bracketCount = 0;
				bool toggleQuote = false; // Used to ignore curly braces inside of quotes, e.g. "{" or "}".
				for (int ii = 0; ii < receivedJson.Length; ++ii)
				{
					if (receivedJson[ii] == '"')
					{
						// Check the amount of backslashes, because the quote might be escaped, e.g. "\""
						int backSlashCount = 0;
						while (ii - (backSlashCount + 1) >= 0)
						{
							if (receivedJson[ii - (backSlashCount + 1)] == '\\')
							{
								backSlashCount++;
							}
							else
							{
								break;
							}
						}

						if (backSlashCount % 2 == 0) // For even backslashes (0, 2, 4) -> change quote toggle.
						{
							toggleQuote = !toggleQuote;
						}
					}
					if (toggleQuote == false) // Only count curcly braces, if they are not under quotes:
					{
						if (receivedJson[ii] == '{')
						{
							bracketCount++;
						}
						else if (receivedJson[ii] == '}')
						{
							bracketCount--;
						}
					}

					if (bracketCount == 0)
					{
						string jsonStr = receivedJson.Substring(startIndex, (ii - startIndex + 1));
						if (jsonStr[0] == '{' && jsonStr[jsonStr.Length - 1] == '}')
						{
							jsonMessages.Add(jsonStr);
						}
						startIndex = ii + 1;
					}
				}
				if (bracketCount != 0)
				{
					_uncompletedMessage = receivedJson.Substring(startIndex, receivedJson.Length - startIndex);
					Debug.LogWarning("Receiver incomplete JSON string: " + _uncompletedMessage);
				}
			}

			return jsonMessages;
		}

		public List<ReceivedMessage> GetReceivedMessages()
		{
			return _receivedMessages; // Reference!
		}
	}
}