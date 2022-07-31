using UnityEngine;
using System;

namespace DeepSpace.JsonProtocol
{
	// This class is used to check the "event_identifier" of a received JSON string.
	[Serializable]
	public class EventType
	{
		[SerializeField]
		private uint event_identifier;

		public uint EventIdentifier
		{
			get { return event_identifier; }
			set { event_identifier = value; }
		}
	}
}