using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;


namespace DeepSpace.Udp
{
	public class UdpBase : MonoBehaviour
	{
		[SerializeField]
		protected string _remoteIpAddress = "127.0.0.1";
		public string IpAddress
		{
			get
			{
				return _remoteIpAddress;
			}
			protected set
			{
				_remoteIpAddress = value;
			}
		}

		[SerializeField]
		protected int _localPort = 5560;
		public int Port
		{
			get
			{
				return _localPort;
			}
			protected set
			{
				_localPort = value;
			}
		}

		protected Socket _socket = null;
		public Socket Socket
		{
			get
			{
				return _socket;
			}
		}

		public void OnDestroy()
		{
			if (_socket != null)
			{
				_socket.Close();
				_socket = null;
			}
		}
	}
}