using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.JsonProtocol;
using DeepSpace.Udp;

using Sender = DeepSpace.Udp.UdpManager.ReceivedMessage.Sender;

namespace DeepSpace
{
	// This class handles all network actions, that are sent from Wall to Floor:
	public class FloorNetworkHandler : MonoBehaviour
	{
		[SerializeField]
		private DebugStereoManager _debugStereoMgr = null;

		private Dictionary<string, UdpTransform> _udpTransformDict = new Dictionary<string, UdpTransform>();

		private Dictionary<string, UdpFieldOfViewSync> _udpFieldOfViewSyncDict = new Dictionary<string, UdpFieldOfViewSync>();


		protected virtual void Start()
		{
			if(_debugStereoMgr == null)
			{
				Debug.LogWarning("DebugStereoManager was not linked to FloorManager. This might lead to null reference exceptions later.");
			}
		}

		protected virtual void OnEnable()
		{
			JsonConverter.ReceivedTransformAsset += HandleTransformAsset;
			//JsonConverter.ReceivedSpawnAsset += HandleSpawnAsset; // Currently not implemented.
			JsonConverter.ReceivedFieldOfViewMessage += HandleFieldOfViewMessage;
			JsonConverter.ReceivedSettingMessage += HandleSettingMessage;
		}

		protected virtual void OnDisable()
		{
			JsonConverter.ReceivedTransformAsset -= HandleTransformAsset;
			//JsonConverter.ReceivedSpawnAsset -= HandleSpawnAsset;
			JsonConverter.ReceivedFieldOfViewMessage -= HandleFieldOfViewMessage;
			JsonConverter.ReceivedSettingMessage -= HandleSettingMessage;
		}

		public void HandleTransformAsset(TransformAsset transformAsset, Sender sender)
		{
			UdpTransform udpTransform;
			if (_udpTransformDict.TryGetValue(transformAsset.Asset_Id, out udpTransform))
			{
				udpTransform.MoveTransform(transformAsset);
			}
		}

		public void RegisterTransformForMovement(UdpTransform udpTransform)
		{
			_udpTransformDict.Add(udpTransform.networkId, udpTransform);
		}

		public void RegisterFieldOfViewForSync(UdpFieldOfViewSync udpFovSync)
		{
			_udpFieldOfViewSyncDict.Add(udpFovSync.networkId, udpFovSync);
		}

		private void HandleFieldOfViewMessage(FieldOfViewAsset fovAsset, Sender sender)
		{
			UdpFieldOfViewSync udpFovSync;
			if (_udpFieldOfViewSyncDict.TryGetValue(fovAsset.Asset_Id, out udpFovSync))
			{
				udpFovSync.ApplyFovLocally(fovAsset);
			}
		}

		private void HandleSettingMessage(SettingMessage settingMessage, Sender sender)
		{
			switch (settingMessage.settingType)
			{
				case SettingMessage.SettingType.EYE_DISTANCE:
					ChangeStereoEyeDist((settingMessage as SettingMessageFloat).floatValue);
					break;
				case SettingMessage.SettingType.CONVERGENCE_DISTANCE:
					ChangeStereoConvDist((settingMessage as SettingMessageFloat).floatValue);
					break;
			}
		}

		public void ChangeStereoEyeDist(float value)
		{
			_debugStereoMgr.OnChangeEyeSeperationWorld(value.ToString());
		}

		public void ChangeStereoConvDist(float value)
		{
			_debugStereoMgr.OnChangeConvergenceDistScale(value.ToString());
		}
	}
}