using System;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.Udp;

namespace DeepSpace.LaserTracking
{
    public class TracklinkReceiveHandler : TrackingReceiveHandler
    {
        [SerializeField]
        private UdpReceiver _udpReceiver = null;

        [SerializeField]
        private string _jsonConfigFileName = "DeepSpaceConfig\\tracklinkConfig.json";
        [SerializeField]
        private TracklinkSettings _tracklinkSettings = new TracklinkSettings();

        public override TrackingSettings TrackingSettings
        {
            get { return _tracklinkSettings; }
        }

        private void Awake()
        {
            if (_udpReceiver == null)
            {
                _udpReceiver = GetComponent<UdpReceiver>();
            }
        }

        private void Start()
        {
            _udpReceiver.ListenToMulticast = _tracklinkSettings.IsMulticastAddress;
            _udpReceiver.SubscribeReceiveEvent(OnReceivedMessage);

            TracklinkSettings loadedTracklinkSettings = LoadSettings<TracklinkSettings>(_jsonConfigFileName);
            if (loadedTracklinkSettings != null)
            {
                _tracklinkSettings = loadedTracklinkSettings;
            }
            _udpReceiver.ActivateReceiver(_tracklinkSettings.UdpPort, _tracklinkSettings.UdpAddress, true);


        }

        private void Update()
        {
            // Development only: Use this, if you changed the TracklinkSettings class and want to generate a new json file based on the new class layout.
            //if (Input.GetKeyUp(KeyCode.S))
            //{
            //	SaveSettings<TracklinkSettings>(_tracklinkSettings, _jsonConfigFileName);
            //}
            // End of Development only.
        }

        private void OnReceivedMessage(byte[] messageBytes, System.Net.IPAddress senderIP)
        {
            int messageBytesLength = messageBytes.Length;
            if (messageBytes != null && messageBytesLength > 0)
            {
                int i = 0;
                while (i < messageBytesLength)
                {
                    if (Convert.ToChar(messageBytes[i++]) != 'T')
                    {
                        Console.WriteLine("TransmissionClient: Unexpected header byte, skipping packet.");
                        i = messageBytesLength;
                        continue;
                    }

                    // get the tracks's id
                    int tid;
                    tid = BytePackHelper.UnpackInt(messageBytes, ref i);

                    // is this track known? if so, update, else add:
                    bool unknownTrack = !_trackDict.ContainsKey(tid);

                    TrackRecord track;
                    if (unknownTrack)
                    {
                        track = new TrackRecord();
                        track.echoes = new List<Vector2>();
                        track.trackID = tid;
                        _trackDict.Add(track.trackID, track);
                    }
                    else
                    {
                        track = _trackDict[tid];
                    }

                    track.state = (TrackState)BytePackHelper.UnpackInt(messageBytes, ref i);
                    track.currentPos.x = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.currentPos.y = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.expectPos.x = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.expectPos.y = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.orientation.x = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.orientation.y = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.speed = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.relPos.x = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.relPos.y = BytePackHelper.UnpackFloat(messageBytes, ref i);
                    track.echoes.Clear();
                    while (Convert.ToChar(messageBytes[i]) == 'E') // peek if echo(es) available
                    {
                        ++i; // yep, then skip 'E'
                        Vector2 echo = new Vector2();
                        echo.x = BytePackHelper.UnpackFloat(messageBytes, ref i);
                        echo.y = BytePackHelper.UnpackFloat(messageBytes, ref i);
                        track.echoes.Add(echo);
                        ++i; // 'e'
                    }

                    if (Convert.ToChar(messageBytes[i++]) != 't')
                    {
                        Console.WriteLine("TransmissionClient: Unexpected tailing byte, skipping packet.");
                        i = messageBytesLength;
                        continue;
                    }

                    //notify callbacks
                    foreach (ITrackingReceiver receiver in _trackingReceiverList)
                    {
                        // track is unknown yet AND is not about to die
                        if (unknownTrack && track.state != TrackState.TRACK_REMOVED)
                        {
                            receiver.OnTrackNew(track);
                        }
                        // standard track update
                        else if (!unknownTrack && track.state != TrackState.TRACK_REMOVED)
                        {
                            receiver.OnTrackUpdate(track);
                        }
                        // track is known and this is his funeral
                        else if (!unknownTrack && track.state == TrackState.TRACK_REMOVED)
                        {
                            receiver.OnTrackLost(track);
                        }
                    }

                    // remove track from dictionary
                    if (track.state == TrackState.TRACK_REMOVED)
                    {
                        _trackDict.Remove(track.trackID);
                    }
                }
            }
        }
    }
}
