using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DeepSpace.LaserTracking
{
	/// <summary>
	/// Derive your tracking driven entities from this.
	/// </summary>
	public class TrackingEntity : MonoBehaviour
	{
		private int _trackID;
		private Vector2 _absolutePosition;
		private Vector2 _nextExpectedAbsolutePosition;
		private Vector2 _relativePosition;
		private Vector2 _orientation;
		private float _speed;
		private List<Vector2> _echoes = new List<Vector2>();


		/// <summary>
		/// TrackID corresponds to the entityId of the tracking service (TUIO sessionID / Pharus trackID).
		/// </summary>
		public int TrackID
		{
			get { return _trackID; }
			set { _trackID = value; }
		}

		/// <summary>
		/// The track's current position in meters (Pharus only)
		/// </summary>
		public Vector2 AbsolutePosition
		{
			get { return _absolutePosition; }
			set { _absolutePosition = value; }
		}

		/// <summary>
		/// The position the track will be expected in the next frame (Pharus only)
		/// </summary>
		public Vector2 NextExpectedAbsolutePosition
		{
			get { return _nextExpectedAbsolutePosition; }
			set { _nextExpectedAbsolutePosition = value; }
		}

		/// <summary>
		/// The track's current position in relative (0 - 1) coordinates
		/// </summary>
		public Vector2 RelativePosition
		{
			get { return _relativePosition; }
			set { _relativePosition = value; }
		}

		/// <summary>
		/// The track's current heading (normalized). Valid if speed is above 0.25 m/s. (Pharus only)
		/// </summary>
		public Vector2 Orientation
		{
			get { return _orientation; }
			set { _orientation = value; }
		}

		/// <summary>
		/// The track's current speed in meters per second (Pharus only)
		/// </summary>
		public float Speed
		{
			get { return _speed; }
			set { _speed = value; }
		}

		/// <summary>
		/// A list of the track's echoes (feet) as Vector2
		/// </summary>
		public List<Vector2> Echoes
		{
			get { return _echoes; }
			set { _echoes = value; }
		}

		public virtual void SetPosition(Vector2 position)
		{
			this.transform.position = position;
		}


		//private void OnDrawGizmos()
		//{
		//	Gizmos.color = Color.red;

		//	// DEBUG DRAW ECHOES
		//	foreach (Vector2 echo in Echoes)
		//	{
		//		Gizmos.DrawWireSphere(UnityTracking.TrackingAdapter.GetScreenPositionFromRelativePosition(echo.x, echo.y), 12f);
		//	}
		//}
	}
}