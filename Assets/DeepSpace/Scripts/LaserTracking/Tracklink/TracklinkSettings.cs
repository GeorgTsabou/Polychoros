using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace.LaserTracking
{
    /// <summary>
    /// Overall PharusTransmission Settings
    /// </summary>
    [System.Serializable]
    public class TracklinkSettings : TrackingSettings
    {
        [SerializeField, Tooltip("Can be a unicast or multicast IP.")]
        private string udpAddress = "239.1.1.1";
        [SerializeField, Tooltip("Port to listen for tracklink data.")]
        private int udpPort = 44345;
        [SerializeField, Tooltip("Is the given IP address a multicast or unicast address?")]
        private bool isMulticastAddress = false;

        public string UdpAddress
        {
            get { return this.udpAddress; }
            set { this.udpAddress = value; }
        }

        public int UdpPort
        {
            get { return this.udpPort; }
            set { this.udpPort = value; }
        }

        public bool IsMulticastAddress
        {
            get { return this.isMulticastAddress; }
            set { this.isMulticastAddress = value; }
        }
    }
}