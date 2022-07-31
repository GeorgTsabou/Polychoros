using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DeepSpace
{
	[ScriptExecutionOrder(-50)]
	public class WallFloorStarter : MonoBehaviour
	{
		public bool isMouseVisible = false;

		public GameObject wallCameraContainer;
		public GameObject floorCameraContainer;

		private CmdConfigManager _configManager;

		protected virtual void Awake()
		{
			// Hide MouseCursor:
#if !UNITY_EDITOR
			Cursor.visible = isMouseVisible;
#endif
			_configManager = CmdConfigManager.Instance;

			if (_configManager.applicationType == CmdConfigManager.AppType.WALL) // Wall
			{
				StartWall();
			}
			else // Floor
			{
				StartFloor();
			}
		}

		protected virtual void StartWall()
		{
			// Disable FloorCam:
			floorCameraContainer.gameObject.SetActive(false);

			// Correct ViewRectPort for WallCamera:
			if (wallCameraContainer != null)
			{
				Camera[] wallCameras = wallCameraContainer.GetComponentsInChildren<Camera>();
				foreach (Camera cam in wallCameras)
				{
					MakeCameraRectFullsize(cam);
				}
			}
			else
			{
				Debug.LogWarning("StartWall: No wall camera container has been defined!", gameObject);
			}

			// Disable everything that is not needed or shall not be seen on the floor.
		}

		protected virtual void StartFloor()
		{
			// Disable WallCam:
			wallCameraContainer.gameObject.SetActive(false);

			// Correct ViewRectPort for FloorCamera:
			if (floorCameraContainer != null)
			{
				Camera[] floorCameras = floorCameraContainer.GetComponentsInChildren<Camera>();
				foreach (Camera cam in floorCameras)
				{
					MakeCameraRectFullsize(cam);
				}
			}
			else
			{
				Debug.LogWarning("StartFloor: No floor camera container has been defined!", gameObject);
			}

			// Disable everything that is not needed or shall not be seen on the wall.
		}

		private void MakeCameraRectFullsize(Camera cam)
		{
			if (cam != null)
			{
				Rect viewPortRect = cam.rect;
				viewPortRect.y = 0.0f;
				viewPortRect.height = 1.0f;
				cam.rect = viewPortRect;
			}
		}
	}
}