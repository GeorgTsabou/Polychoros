using UnityEngine;
using System.Collections;

namespace DeepSpace
{
	public class UdpCmdConfigMgr : CmdConfigManager
	{
		// Values for your own configuration...
		public string udpAddress = "127.0.0.1"; // The ip where data are sent to.
		public int udpReceivingPort = 5560; // The port where data will be recieved.
		public int udpSendingPort = 5560; // This port only needs to be different from udpReceivingPort if two applications run on the same system.
		public int networkFrameDelay = 2; // The delay is used for transform movement. The sender waits some frames after sending before doing the transformation locally.
		public string vrpnHost = "127.0.0.1"; // The ip where the VRPN host is running.

		private int _udpPort = 5560; // Use either this or udpReceivingPort and udpSendingPort.
		private bool _udpPortWasSet = false;
		private bool _udpSendingReceivingSet = false;

		protected override void ParseArgument(string key, string value)
		{
			key = key.ToLower();
			value = value.ToLower();

			if (key.Equals("-udpaddress"))
			{
				udpAddress = value;
			}
			else if (key.Equals("-udpport")) // Setting sending and receiving port to the given port (or port + 1) vice versa for floor and wall.
			{
				int resultUdp;
				if (ValueToInt(value, out resultUdp))
				{
					_udpPort = resultUdp;
					_udpPortWasSet = true;
				}
			}
			else if (key.Equals("-udpreceivingport")) // Setting only the receiving port (needed for testing Wall and Floor on the same machine)
			{
				int resultUdp;
				if (ValueToInt(value, out resultUdp))
				{
					udpReceivingPort = resultUdp;
					_udpSendingReceivingSet = true;
				}
			}
			else if (key.Equals("-udpsendingport")) // Setting only the sending port (needed for testing Wall and Floor on the same machine)
			{
				int resultUdp;
				if (ValueToInt(value, out resultUdp))
				{
					udpSendingPort = resultUdp;
					_udpSendingReceivingSet = true;
				}
			}
			else if (key.Equals("-framedelay"))
			{
				int resultDelay;
				if (ValueToInt(value, out resultDelay))
				{
					networkFrameDelay = resultDelay;
				}
			}
			else if (key.Equals("-vrpnhost"))
			{
				vrpnHost = value;
			}
			else
			{
				base.ParseArgument(key, value);
			}
		}

		protected override void FinishedParsingArguments()
		{
			// If -udpPort parameter was used, and -udpSendingPort & -udpReceivingPort have both not been used, configure ports automatically (backwards compatibility).
			// Else use the predefined ports (might be handy, if two seperate builds have been made for wall and floor).
			if(_udpPortWasSet == true && _udpSendingReceivingSet == false)
			{
				if(applicationType == AppType.WALL)
				{
					udpSendingPort = _udpPort;
					udpReceivingPort = _udpPort + 1;
				}
				else // if (applicationType == AppType.FLOOR)
				{
					udpSendingPort = _udpPort + 1;
					udpReceivingPort = _udpPort;
				}
			}
		}
	}
}