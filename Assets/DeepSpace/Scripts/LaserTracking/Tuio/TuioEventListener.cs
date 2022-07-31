using UnityEngine;
using System.Collections.Generic;
using TUIO;

namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// The TuioEventListener has a TuioClient which listens for TUIO tracking data.
	/// The received data are stored in a Queue as TuioEvents.
	/// </summary>
	public class TuioEventListener : TUIO.TuioListener
	{
		/// <summary>
		/// Specifies the type of the TuioEvent
		/// </summary>
		public enum ETuioEventType
		{
			ADD_OBJECT,
			UPDATE_OBJECT,
			REMOVE_OBJECT,
			ADD_CURSOR,
			UPDATE_CURSOR,
			REMOVE_CURSOR,
			ADD_BLOB,
			UPDATE_BLOB,
			REMOVE_BLOB
		}

		/// <summary>
		/// A helper struct that encapsulates a new TUIO object together with the desired operation
		/// </summary>
		public struct TuioEvent
		{
			private ETuioEventType _tuioEventType;
			private TuioContainer _tuioEntity;

			public TuioEvent(ETuioEventType eventType, TuioContainer entity)
			{
				_tuioEventType = eventType;
				_tuioEntity = entity;
			}

			public ETuioEventType TuioEventType
			{
				get { return _tuioEventType; }
			}

			public TuioContainer TuioEntity
			{
				get { return _tuioEntity; }
			}
		}

		/// <summary>
		/// The UDP Port on which the TUIO Client should listen for TUIO data.
		/// </summary>
		private int _udpPort = 3333;

		/// <summary>
		/// The TUIO client object responsible for receiving tracking data.
		/// </summary>
		private TuioClient _client;

		private readonly object _lockObj = new object();
		/// <summary>
		/// Empty lock object for thread safety.
		/// </summary>
		public object LockObj
		{
			get { return _lockObj; }
		}

		private Queue<TuioEvent> _eventQueue;
		/// <summary>
		/// Contains all registered TuioEvents
		/// </summary>
		public Queue<TuioEvent> EventQueue
		{
			get { return _eventQueue; }
		}

		#region constructors
		public TuioEventListener()
		{
			InitTracking();
		}

		public TuioEventListener(int udpPort)
		{
			_udpPort = udpPort;

			InitTracking();
		}
		#endregion

		#region private methods
		private void InitTracking()
		{
			if (_client != null && _client.isConnected())
			{
				Debug.LogWarning("Client is already connected. Abort opening another connection.");
				return;
			}

			_eventQueue = new Queue<TuioEvent>();

			// Create a new TUIO client and listen for data on the specified port
			_client = new TuioClient(_udpPort);
			_client.addTuioListener(this);
			_client.connect();

			if (!_client.isConnected())
			{
				Debug.LogError("Couldn't listen at port " + _udpPort + " for TUIO data. Check if port isn't already in use. (netstat -ano | find \"" + _udpPort + "\")\n(also be sure to kill adb.exe if its still running)");
				_client.removeTuioListener(this);
				_client = null;
			}
			else
			{
				Debug.Log("Connection establised: listening at port " + _udpPort + " for TUIO data.");
			}
		}
		#endregion

		#region public methods
		/// <summary>
		/// Returns if the Tuio Client is connected and listens for data.
		/// </summary>
		/// <returns>The listening status.</returns>
		public bool IsConnected()
		{
			if (_client != null)
			{
				return _client.isConnected();
			}

			Debug.LogWarning("No TuioClient object available.");

			return false;
		}

		/// <summary>
		/// Shuts down the reception of tracking data by disconnecting the TUIO client.
		/// </summary>
		public void Shutdown()
		{
			if (_client != null)
			{
				_client.removeTuioListener(this);

				if (_client.isConnected())
				{
					_client.disconnect();
				}

				_client = null;
				Debug.Log("Disconnected TUIO client: port is now free.");
			}
		}

		public bool HasTuioContainers()
		{
			return (HasTuioObjects() || HasTuioCursors() || HasTuioBlobs());
		}

		public bool HasTuioObjects()
		{
			return (_client.getTuioObjects().Count > 0);
		}

		public bool HasTuioCursors()
		{
			return (_client.getTuioCursors().Count > 0);
		}

		public bool HasTuioBlobs()
		{
			return (_client.getTuioBlobs().Count > 0);
		}
		#endregion

		#region TuioListener implementation
		public void addTuioObject(TuioObject tobj)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_OBJECT, tobj));
			}
		}

		public void updateTuioObject(TuioObject tobj)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_OBJECT, tobj));
			}
		}

		public void removeTuioObject(TuioObject tobj)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_OBJECT, tobj));
			}
		}

		public void addTuioCursor(TuioCursor tcur)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_CURSOR, tcur));
			}
		}

		public void updateTuioCursor(TuioCursor tcur)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_CURSOR, tcur));
			}
		}

		public void removeTuioCursor(TuioCursor tcur)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_CURSOR, tcur));
			}
		}

		public void addTuioBlob(TuioBlob tblb)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.ADD_BLOB, tblb));
			}
		}

		public void updateTuioBlob(TuioBlob tblb)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.UPDATE_BLOB, tblb));
			}
		}

		public void removeTuioBlob(TuioBlob tblb)
		{
			lock (_lockObj)
			{
				_eventQueue.Enqueue(new TuioEvent(ETuioEventType.REMOVE_BLOB, tblb));
			}
		}

		public void refresh(TuioTime ftime)
		{
			// Intentionally left empty. We don't need the extra update loop but TUIO forces us to implent it.
		}
		#endregion
	}
}

