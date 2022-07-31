using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace.LaserTracking
{
	public class TuioReceiveHandler : TrackingReceiveHandler
	{
		[SerializeField]
		private string _jsonConfigFileName = "DeepSpaceConfig\\tuioConfig.json";
		[SerializeField]
		private TuioSettings _tuioSettings = new TuioSettings();

		private TuioEventListener _listener;
		private TuioEventProcessor _eventProcessor;

		private List<int> _updatedTrackIds = new List<int>();

		private bool _isRegisteredForTuioEvents = false;

		public override TrackingSettings TrackingSettings
		{
			get { return _tuioSettings; }
		}

		private void Start()
		{
			TuioSettings loadedTuioSettings = LoadSettings<TuioSettings>(_jsonConfigFileName);
			if(loadedTuioSettings != null)
			{
				_tuioSettings = loadedTuioSettings;
			}

			_listener = new TuioEventListener(_tuioSettings.UdpPort);
			_eventProcessor = new TuioEventProcessor(_listener);

			RegisterForTuioEvents();
		}

		private void Update()
		{
			// Development only: Use this, if you changed the TracklinkSettings class and want to generate a new json file based on the new class layout.
			//if (Input.GetKeyUp(KeyCode.S))
			//{
			//	SaveSettings<TuioSettings>(_tuioSettings, _jsonConfigFileName);
			//}
			// End of Development only.

			//Lister for Tuio Data if enabled
			if (_eventProcessor != null)
			{
				_eventProcessor.Process();
			}

			// Iterate through all tracks, that got updated:
			foreach (int trackId in _updatedTrackIds)
			{
				TrackRecord track = GetTrackById(trackId);

				// Send events to subsribers:
				foreach (ITrackingReceiver receiver in _trackingReceiverList)
				{
					// track is unknown yet AND is not about to die
					if (track.state == TrackState.TRACK_ADDED)
					{
						receiver.OnTrackNew(track);
					}
					// standard track update
					else if (track.state == TrackState.TRACK_UPDATE)
					{
						receiver.OnTrackUpdate(track);
					}
					// track is known and this is his funeral
					else if (track.state == TrackState.TRACK_REMOVED)
					{
						receiver.OnTrackLost(track);
					}
				}

				// After sending, if it is a removed track message, remove it from the dict:
				if (track.state == TrackState.TRACK_REMOVED)
				{
					if (_trackDict.ContainsKey(track.trackID))
					{
						_trackDict.Remove(track.trackID);
					}
				}
			}

			// Clear the Updated tracks list afterwards:
			_updatedTrackIds.Clear();
		}

		protected override void OnDestroy()
		{
			UnregisterForTuioEvents();

			base.OnDestroy();
		}

		void OnEnable()
		{
			RegisterForTuioEvents();
		}

		void OnDisable()
		{
			UnregisterForTuioEvents();
		}

		private void RegisterForTuioEvents()
		{
			if (_eventProcessor != null && _isRegisteredForTuioEvents == false)
			{
				// Cursors: This represents tracks in Pharus:
				_eventProcessor.CursorAdded += OnCursorAdded;
				_eventProcessor.CursorUpdated += OnCursorUpdated;
				_eventProcessor.CursorRemoved += OnCursorRemoved;

				// Objects: This represents echos in Pharus: 
				// NOTE: Sending TUIO Objects might be disabled in Pharus. If you do not receive this, it needs to be enabled within Pharus.
				// NOTE: Echos (in form of TUIO Objects) are just added and removed, they do not have an persistant ID and they are not updated.
				_eventProcessor.ObjectAdded += OnObjectAdded;
				//_eventProcessor.ObjectUpdated += OnObjectUpdated; // This is not used by Pharus
				_eventProcessor.ObjectRemoved += OnObjectRemoved;

				// NOTE: Blobs are currently not used in Pharus.

				_isRegisteredForTuioEvents = true;
			}
		}

		private void UnregisterForTuioEvents()
		{
			if (_eventProcessor != null && _isRegisteredForTuioEvents == true)
			{
				// Cursors: This represents tracks in Pharus:
				_eventProcessor.CursorAdded -= OnCursorAdded;
				_eventProcessor.CursorUpdated -= OnCursorUpdated;
				_eventProcessor.CursorRemoved -= OnCursorRemoved;

				// Objects: This represents echos in Pharus: 
				_eventProcessor.ObjectAdded -= OnObjectAdded;
				//_eventProcessor.ObjectUpdated -= OnObjectUpdated; // This is not used by Pharus
				_eventProcessor.ObjectRemoved -= OnObjectRemoved;

				_isRegisteredForTuioEvents = false;
			}
		}

		#region tuio event handlers
		// Receiving Cursors:
		void OnCursorAdded(object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			OnReceivedTrackInformation(e.tuioCursor, TrackState.TRACK_ADDED);
		}

		void OnCursorUpdated(object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			OnReceivedTrackInformation(e.tuioCursor, TrackState.TRACK_UPDATE);
		}

		void OnCursorRemoved(object sender, TuioEventProcessor.TuioEventCursorArgs e)
		{
			OnReceivedTrackInformation(e.tuioCursor, TrackState.TRACK_REMOVED);
		}

		// Receiving Objects:
		void OnObjectAdded(object sender, TuioEventProcessor.TuioEventObjectArgs e)
		{
			OnUpdateEchoInformation(e.tuioObject, TrackState.TRACK_ADDED);
		}

		//void OnObjectUpdated(object sender, TuioEventProcessor.TuioEventObjectArgs e)
		//{
		//	// Currently not in use.
		//}

		void OnObjectRemoved(object sender, TuioEventProcessor.TuioEventObjectArgs e)
		{
			OnUpdateEchoInformation(e.tuioObject, TrackState.TRACK_REMOVED);
		}
		#endregion

		private void OnReceivedTrackInformation(TUIO.TuioCursor tuioCursor, TrackState state)
		{
			TrackRecord track = GetTrackById(tuioCursor.CursorID);
			
			track.state = state;
			Vector2 absPos = TrackingSettings.GetScreenPositionFromRelativePosition(tuioCursor.Position.X, tuioCursor.Position.Y);
			track.currentPos.x = absPos.x;
			track.currentPos.y = absPos.y;
			track.expectPos.x = tuioCursor.Position.X + (tuioCursor.XSpeed * Time.deltaTime);
			track.expectPos.y = tuioCursor.Position.Y + (tuioCursor.YSpeed * Time.deltaTime);
			Vector2 orientation = (new Vector2(tuioCursor.XSpeed, tuioCursor.YSpeed)).normalized;
			track.orientation.x = orientation.x;
			track.orientation.y = orientation.y;
			track.speed = tuioCursor.XSpeed + tuioCursor.YSpeed;
			track.relPos.x = tuioCursor.Position.X;
			track.relPos.y = tuioCursor.Position.Y;

			SaveTrackToDict(track, createIfNotYetExisting: true);

			_updatedTrackIds.Add(track.trackID);
		}

		private void OnUpdateEchoInformation(TUIO.TuioObject tuioObject, TrackState state)
		{
			TrackRecord track = GetTrackById(tuioObject.SymbolID); // Symbol ID is the ID of the track, to which the echo belongs.
			
			if(state == TrackState.TRACK_ADDED)
			{
				track.echoes.Add(new Vector2(tuioObject.Position.X, tuioObject.Position.Y));
			}
			else if(state == TrackState.TRACK_REMOVED)
			{
				// Actually I can find the one specific, by saving and comparing Session IDs or by comparing the positions, but this is not necessary in this case.
				// Pharus sends Tuio Remove messages for all echos. Then it sends add messages for all echos. If one is removed, all are being removed.
				track.echoes.Clear();
			}

			SaveTrackToDict(track, createIfNotYetExisting: false);
		}

		private TrackRecord GetTrackById(int trackId)
		{
			bool unknownTrack = !_trackDict.ContainsKey(trackId);

			TrackRecord track;

			if (unknownTrack)
			{
				track = new TrackRecord();
				track.trackID = trackId;
				track.echoes = new List<Vector2>();
				_trackDict.Add(track.trackID, track);
			}
			else
			{
				track = _trackDict[trackId];
			}

			return track;
		}

		private void SaveTrackToDict(TrackRecord track, bool createIfNotYetExisting)
		{
			if(_trackDict.ContainsKey(track.trackID))
			{
				_trackDict[track.trackID] = track;
			}
			else
			{
				_trackDict.Add(track.trackID, track);
			}
		}
	}
}