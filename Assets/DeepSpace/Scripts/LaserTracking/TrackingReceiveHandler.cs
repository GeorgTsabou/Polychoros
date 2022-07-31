using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeepSpace.LaserTracking
{
	public class TrackingReceiveHandler : MonoBehaviour
	{
		protected List<ITrackingReceiver> _trackingReceiverList = new List<ITrackingReceiver>();
		protected Dictionary<int, TrackRecord> _trackDict = new Dictionary<int, TrackRecord>();

		public virtual TrackingSettings TrackingSettings { get; }

		protected virtual void OnDestroy()
		{
			RemoveAllTrackRecords();

			_trackingReceiverList.Clear();
		}

		public void RegisterTrackingReceiver(ITrackingReceiver newReceiver)
		{
			if (_trackingReceiverList.Contains(newReceiver))
			{
				return;
			}
			_trackingReceiverList.Add(newReceiver);
		}

		public void UnregisterTrackingReceiver(ITrackingReceiver oldReceiver)
		{
			_trackingReceiverList.Remove(oldReceiver);
		}

		protected T LoadSettings<T>(string jsonConfigFileName) where T : TrackingSettings
		{
			T resultSettings = null;
			string filePath = Path.Combine(Application.streamingAssetsPath, jsonConfigFileName);

			if (File.Exists(filePath))
			{
				// Read the json from the file into a string
				string dataAsJson = File.ReadAllText(filePath);
				// Pass the json to JsonUtility, and tell it to create an object from it
				resultSettings = JsonUtility.FromJson<T>(dataAsJson);
			}
			else
			{
				Debug.LogWarning("Could not find the Tracklink Settings file at the specified path: " + filePath
					+ "\nUsing the default values that have been set in the editor instead.");
			}

			return resultSettings;
		}

		protected void SaveSettings<T>(T trackingSettings, string jsonConfigFileName) where T : TrackingSettings
		{
			string dataAsJson = JsonUtility.ToJson(trackingSettings, true);

			string filePath = Path.Combine(Application.streamingAssetsPath, jsonConfigFileName);
			File.WriteAllText(filePath, dataAsJson);
			Debug.Log("Saved json to " + filePath);
		}

		protected void RemoveAllTrackRecords()
		{
			foreach (KeyValuePair<int, TrackRecord> entry in _trackDict)
			{
				TrackRecord track = entry.Value;
				foreach (ITrackingReceiver receiver in _trackingReceiverList)
				{
					track.state = TrackState.TRACK_REMOVED;
					receiver.OnTrackLost(track);
				}
			}
		}
	}
}