using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeepSpace.JsonProtocol;

namespace DeepSpace.Udp
{
	public class UdpFieldOfViewSync : MonoBehaviour
	{
		public UdpManager udpManager;
		public FloorNetworkHandler floorNetworkHandler;
		public OffAxisFieldOfViewManager _fieldOfViewManager;
		public string networkId; // TODO: Get this from an ID Manager, etc.


		private UdpCmdConfigMgr _configMgr;

		private void Awake()
		{
			_configMgr = UdpCmdConfigMgr.Instance as UdpCmdConfigMgr;
		}

		private void Start()
		{
			if(_configMgr.applicationType == CmdConfigManager.AppType.FLOOR)
			{
				floorNetworkHandler.RegisterFieldOfViewForSync(this);
			}
		}

		private void ChangeFovOverNetwork(FieldOfViewAsset fovAsset)
		{
			// Wall only:
			if (_configMgr.applicationType == CmdConfigManager.AppType.WALL)
			{
				// Sending the sync data via UDP:
				fovAsset.Asset_Id = networkId;
				udpManager.SenderToFloor.AddMessage(JsonUtility.ToJson(fovAsset));

				// Apply the field of view delayed:
				StartCoroutine(ApplyFovAfter(_configMgr.networkFrameDelay, fovAsset));
			}
			else // Floor only:
			{
				// Apply the field of view:
				ApplyFovLocally(fovAsset);
			}
		}

		private IEnumerator ApplyFovAfter(int frameAmount, FieldOfViewAsset fovAsset)
		{
			for (int ii = 0; ii < frameAmount; ++ii)
			{
				yield return null; // Wait for one frame.
			}

			ApplyFovLocally(fovAsset);
		}

		// This method changes the FOV on the local host:
		public void ApplyFovLocally(FieldOfViewAsset fovAsset)
		{
			if (fovAsset.Reset == true)
			{
				_fieldOfViewManager.ResetFOV();
			}
			else if (fovAsset.ZOffset != 0f)
			{
				_fieldOfViewManager.ChangeFOV(fovAsset.ZOffset);
			}
		}

		public void ChangeFOV(float changeValue)
		{
			FieldOfViewAsset fovAsset = new FieldOfViewAsset();
			fovAsset.ZOffset = changeValue;

			ChangeFovOverNetwork(fovAsset);
		}

		public void ResetFOV()
		{
			FieldOfViewAsset fovAsset = new FieldOfViewAsset();
			fovAsset.Reset = true;

			ChangeFovOverNetwork(fovAsset);
		}
	}
}