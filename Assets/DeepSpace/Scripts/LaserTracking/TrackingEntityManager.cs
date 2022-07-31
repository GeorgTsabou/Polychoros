using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DeepSpace.LaserTracking
{
	public class TrackingEntityManager : MonoBehaviour, ITrackingReceiver
	{
		[SerializeField]
		private TrackingReceiveHandler _trackingReceiveHandler = null;

		protected Dictionary<int, TrackingEntity> _trackingEntityDict = new Dictionary<int, TrackingEntity>();

		[Tooltip("This transform can be null, if no parent is wanted.")]
		public Transform trackSpawnParent = null;
		[Tooltip("This prefab will be spawned for each track.")]
		public GameObject TrackingEntityPrefab = null;
		public bool addUnknownTrackOnUpdate = true;
		public Vector2 gridOffset = new Vector2(0.0f, 0.0f);

		public List<TrackingEntity> TrackingEntityList
		{
			get { return new List<TrackingEntity>(_trackingEntityDict.Values); }
		}

		void OnEnable()
		{
			if (_trackingReceiveHandler != null)
			{
				_trackingReceiveHandler.RegisterTrackingReceiver(this);
			}
		}

		void OnDisable()
		{
			if (_trackingReceiveHandler != null)
			{
				_trackingReceiveHandler.UnregisterTrackingReceiver(this);
			}
		}

		#region tracklink event handlers
		public void OnTrackNew(TrackRecord track)
		{
			TrackAdded(track);
		}

		public void OnTrackUpdate(TrackRecord track)
		{
			TrackUpdated(track);
		}

		public void OnTrackLost(TrackRecord track)
		{
			TrackRemoved(track.trackID);
		}
		#endregion

		#region tracking entity management
		public virtual void TrackAdded(TrackRecord trackRecord)
		{
			Vector2 position = _trackingReceiveHandler.TrackingSettings.GetScreenPositionFromRelativePosition(trackRecord.relPos.x, trackRecord.relPos.y);
			GameObject trackInstance = GameObject.Instantiate(TrackingEntityPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity) as GameObject;
			trackInstance.transform.SetParent(trackSpawnParent);

            //Peristrofh 90 moires ston X gia na fainetai or8io
            trackInstance.transform.rotation = Quaternion.AngleAxis(-90, transform.right);

            trackInstance.name = string.Format("PharusTrack_{0}", trackRecord.trackID);

			TrackingEntity trackingEntity = trackInstance.GetComponent<TrackingEntity>();
			trackingEntity.TrackID = trackRecord.trackID;

			ApplyTrackData(trackingEntity, trackRecord);

			_trackingEntityDict.Add(trackingEntity.TrackID, trackingEntity);
		}

		public virtual void TrackUpdated(TrackRecord trackRecord)
		{
			TrackingEntity trackingEntity = null;
			if (_trackingEntityDict.TryGetValue(trackRecord.trackID, out trackingEntity))
			{
				ApplyTrackData(trackingEntity, trackRecord);

				trackingEntity.SetPosition(_trackingReceiveHandler.TrackingSettings.GetScreenPositionFromRelativePosition(trackRecord.relPos.x, trackRecord.relPos.y));
			}
			else
			{
				if (addUnknownTrackOnUpdate)
				{
					TrackAdded(trackRecord);
				}
			}
		}

		public virtual void TrackRemoved(int trackID)
		{
			TrackingEntity trackingEntity = null;
			if (_trackingEntityDict.TryGetValue(trackID, out trackingEntity))
			{
				_trackingEntityDict.Remove(trackID);

				if(trackingEntity != null)
				{
					Destroy(trackingEntity.gameObject);
				}
			}
		}

		protected virtual void ApplyTrackData(TrackingEntity trackingEntity, TrackRecord trackRecord)
		{
			trackingEntity.AbsolutePosition = new Vector2(trackRecord.currentPos.x - gridOffset.x, trackRecord.currentPos.y - gridOffset.y);
			trackingEntity.NextExpectedAbsolutePosition = new Vector2(trackRecord.expectPos.x - gridOffset.x, trackRecord.expectPos.y - gridOffset.y);
			trackingEntity.RelativePosition = new Vector2(trackRecord.relPos.x, trackRecord.relPos.y);

            trackingEntity.Orientation = new Vector2(trackRecord.orientation.x, trackRecord.orientation.y);

            trackingEntity.Speed = trackRecord.speed;
			trackingEntity.Echoes.Clear();
			trackRecord.echoes.AddRange(trackingEntity.Echoes);
		}
		#endregion
	}
}