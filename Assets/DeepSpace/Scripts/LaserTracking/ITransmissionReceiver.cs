namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// Interface for everything that wants to receive track updates from Pharus.
	/// </summary>
	public interface ITrackingReceiver
	{
		void OnTrackNew(TrackRecord track);
		void OnTrackUpdate(TrackRecord track);
		void OnTrackLost(TrackRecord track);
	}
}