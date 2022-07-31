using UnityEngine;
using System.Collections;

namespace DeepSpace.JsonProtocol
{
	public class TransformAsset : AssetIdType
	{
		// Serialized Attributes:
		[SerializeField]
		private Vector3 _position;
		[SerializeField]
		private Quaternion _rotation;
		[SerializeField]
		private Vector3 _scale;

		public TransformAsset()
		{
			EventIdentifier = EventIdentifierBase.ASSET_TRANSFORM;
		}

		public Vector3 Position
		{
			get { return _position; }
			set { _position = value; }
		}

		public Quaternion Rotation
		{
			get { return _rotation; }
			set { _rotation = value; }
		}

		public Vector3 Scale
		{
			get { return _scale; }
			set { _scale = value; }
		}
	}
}