using UnityEngine;

namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// Overall TUIO Settings
	/// </summary>
	[System.Serializable]
	public class TuioSettings : TrackingSettings
	{
		[SerializeField]
		private int udpPort = 3333;

		public int UdpPort
		{
			get { return udpPort; }
			set { this.udpPort = value; }
		}
	}
}