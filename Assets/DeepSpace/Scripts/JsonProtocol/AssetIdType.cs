using UnityEngine;
using System.Collections;

namespace DeepSpace.JsonProtocol
{
	[System.Serializable]
	public class AssetIdType : EventType
	{
		// Serialized Attributes:
		[SerializeField]
		protected string asset_id;

		public string Asset_Id
		{
			get { return asset_id; }
			set { asset_id = value; }
		}
	}
}