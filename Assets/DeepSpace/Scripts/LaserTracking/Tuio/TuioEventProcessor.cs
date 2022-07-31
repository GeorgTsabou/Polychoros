using System;
using TUIO;

namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// The UnityTuioEventProcessor picks up all the TuioEvent from the queue provided by the UnityTuioListener and informs its subscribers.
	/// </summary>
	public class TuioEventProcessor 
	{
		#region event args
		public class TuioEventObjectArgs : EventArgs
		{
			public readonly TuioObject tuioObject;
			public TuioEventObjectArgs(TuioObject theTuioObject)
			{
				tuioObject = theTuioObject;
			}
		}
		public class TuioEventCursorArgs : EventArgs
		{
			public readonly TuioCursor tuioCursor;
			public TuioEventCursorArgs(TuioCursor theTuioCursor)
			{
				tuioCursor = theTuioCursor;
			}
		}
		public class TuioEventBlobArgs : EventArgs
		{
			public readonly TuioBlob tuioBlob;
			public TuioEventBlobArgs(TuioBlob theTuioBlob)
			{
				tuioBlob = theTuioBlob;
			}
		}
		#endregion

		#region event handlers
		public event EventHandler<TuioEventObjectArgs> ObjectAdded;
		public event EventHandler<TuioEventObjectArgs> ObjectUpdated;
		public event EventHandler<TuioEventObjectArgs> ObjectRemoved;
		
		public event EventHandler<TuioEventCursorArgs> CursorAdded;
		public event EventHandler<TuioEventCursorArgs> CursorUpdated;
		public event EventHandler<TuioEventCursorArgs> CursorRemoved;
		
		public event EventHandler<TuioEventBlobArgs> BlobAdded;
		public event EventHandler<TuioEventBlobArgs> BlobUpdated;
		public event EventHandler<TuioEventBlobArgs> BlobRemoved;
		#endregion

		private TuioEventListener m_listener;

		#region constructor
		public TuioEventProcessor(TuioEventListener theUnityTuioListener)
		{
			m_listener = theUnityTuioListener;
		}
		#endregion

		#region finalizer
		~TuioEventProcessor()
		{
			ClearAllSubscribers();
		}
		#endregion

		#region public methods
		public void Process()
		{
			while (m_listener.EventQueue.Count > 0)
			{
				TuioEventListener.TuioEvent aEvent;
				lock (m_listener.LockObj)
				{
					aEvent = m_listener.EventQueue.Dequeue();
				}
				switch (aEvent.TuioEventType)
				{
					case TuioEventListener.ETuioEventType.ADD_OBJECT:
						if(ObjectAdded != null) ObjectAdded(this, new TuioEventObjectArgs((TuioObject)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.UPDATE_OBJECT:
						if(ObjectUpdated != null) ObjectUpdated(this, new TuioEventObjectArgs((TuioObject)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.REMOVE_OBJECT:
						if(ObjectRemoved != null) ObjectRemoved(this, new TuioEventObjectArgs((TuioObject)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.ADD_CURSOR:
						if(CursorAdded != null) CursorAdded(this, new TuioEventCursorArgs((TuioCursor)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.UPDATE_CURSOR:
						if(CursorUpdated != null) CursorUpdated(this, new TuioEventCursorArgs((TuioCursor)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.REMOVE_CURSOR:
						if(CursorRemoved != null) CursorRemoved(this, new TuioEventCursorArgs((TuioCursor)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.ADD_BLOB:
						if(BlobAdded != null) BlobAdded(this, new TuioEventBlobArgs((TuioBlob)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.UPDATE_BLOB:
						if(BlobUpdated != null) BlobUpdated(this, new TuioEventBlobArgs((TuioBlob)aEvent.TuioEntity));
						break;
					case TuioEventListener.ETuioEventType.REMOVE_BLOB:
						if(BlobRemoved != null) BlobRemoved(this, new TuioEventBlobArgs((TuioBlob)aEvent.TuioEntity));
						break;
				}
			}
		}

		public void ClearAllSubscribers()
		{
			ObjectAdded = null;
			ObjectUpdated = null;
			ObjectRemoved = null;
			CursorAdded = null;
			CursorUpdated = null;
			CursorRemoved = null;
			BlobAdded = null;
			BlobUpdated = null;
			BlobRemoved = null;
		}
		#endregion

	}
}