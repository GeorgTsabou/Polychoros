using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.JsonProtocol;

namespace DeepSpace.Udp
{
	public class UdpTransform : MonoBehaviour
	{
		public UdpManager udpManager;
		public FloorNetworkHandler floorNetworkHandler;
		public Transform moveTrans; // Transform that shall be moved over network.
		public string networkId; // TODO: Get this from an ID Manager, etc.

		//public int updateEachFrame = 2; // e.g. 2 means, only send an update over network each 2nd frame.

		private Transform targetTrans; // Transform that is moved locally to controll moveTrans position after Network-Communication.

		//private float timeTillUpdate;
		//private float curDeltaTime = 0.0f;

		private UdpCmdConfigMgr _configMgr;
		
		private void Awake()
		{
			targetTrans = transform;

			if (moveTrans == null)
			{
				moveTrans = targetTrans.parent;
			}

			// If the transform is set as child object, this algorithm wont work. (Except, by temporary saving this transform and setting it back after moving the parent)
			targetTrans.parent = null;

			_configMgr = CmdConfigManager.Instance as UdpCmdConfigMgr;

			//QualitySettings.vSyncCount = 2; // Make vSync 2 times before rendering -> Results in 30 FPS rendering.
			//Application.targetFrameRate = 30; // Only works, if vSync is off (vSyncCount = 0).
			//timeTillUpdate = (1f / 60f) * updateEachFrame;
		}

		private void Start()
		{
			if(_configMgr.applicationType == CmdConfigManager.AppType.FLOOR)
			{
				floorNetworkHandler.RegisterTransformForMovement(this);
			}
		}

		private void Update()
		{
			if (_configMgr.applicationType == CmdConfigManager.AppType.WALL)
			{
				// Sending the position data via UDP:
				TransformAsset transformAsset = new TransformAsset();
				transformAsset.Asset_Id = networkId;
				transformAsset.Position = targetTrans.position;
				transformAsset.Rotation = targetTrans.rotation;
				transformAsset.Scale = targetTrans.localScale;
				udpManager.SenderToFloor.AddMessage(JsonUtility.ToJson(transformAsset));

				if (_configMgr.networkFrameDelay > 0)
				{
					StartCoroutine(UpdatePositionAfter(_configMgr.networkFrameDelay, transformAsset));
				}
				else
				{
					MoveTransform(transformAsset);
				}
			}
		}

		IEnumerator UpdatePositionAfter(int frameAmount, TransformAsset transformAsset)
		{
			for (int ii = 0; ii < frameAmount; ++ii)
			{
				yield return null; // Wait for one frame.
			}

			MoveTransform(transformAsset);
		}

		// This is called from the Wall directly during the Update() Routine (with a configured delay)
		// and from the FloorManager, that receives the sent TransformAsset Messages.
		public void MoveTransform(TransformAsset transformAsset)
		{
			moveTrans.position = transformAsset.Position;
			moveTrans.rotation = transformAsset.Rotation;
			moveTrans.localScale = transformAsset.Scale;
		}
	}
}