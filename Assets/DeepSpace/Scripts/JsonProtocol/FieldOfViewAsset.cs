using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace.JsonProtocol
{
	public class FieldOfViewAsset : AssetIdType
	{
		[SerializeField]
		private float _zOffset = 0f;
		[SerializeField]
		private bool _reset = false;

		public FieldOfViewAsset()
		{
			EventIdentifier = EventIdentifierBase.ASSET_FOV;
		}

		public float ZOffset
		{
			get { return _zOffset; }
			set { _zOffset = value; }
		}

		public bool Reset
		{
			get { return _reset; }
			set { _reset = value; }
		}
	}
}