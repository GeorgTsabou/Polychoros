namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// Denotes the track's state
	/// </summary>
	public enum TrackState
	{
		/// <summary>
		/// The track has been made public for the first iteration
		/// </summary>
		TRACK_ADDED = 0,
		/// <summary>
		/// The track is already known - this is a position update
		/// </summary>
		TRACK_UPDATE = 1,
		/// <summary>
		/// The track has disappeared - this is the last notification of it
		/// </summary>
		TRACK_REMOVED = 2
	}
}

