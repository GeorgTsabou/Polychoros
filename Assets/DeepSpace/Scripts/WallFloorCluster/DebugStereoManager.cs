using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace DeepSpace
{
	public class DebugStereoManager : MonoBehaviour
	{
		[SerializeField]
		private KeyCode _toggleDebugMenuKey = KeyCode.Escape;

		public Canvas canvas;
		//public Toggle useMirrorMode; // This does not work at all, last test with Unity 5.4.5 and 5.5.3
		public InputField inputEyeDistWorld;
		public InputField inputUiCanvasScale;

		//[SerializeField]
		//private CameraOffAxisProjection _observer;
		[SerializeField]
		private CameraOffAxisProjection[] _worldCameras = null;
		[SerializeField]
		//private Camera[] _uiCameras = null;
		private StereoCanvasScaler[] _stereoCanvasScaler = null;

		private bool isUiVisible = true;

		private CmdConfigManager _config;

		[SerializeField]
		private GameObject _controllerFocusObject = null;

		public bool IsUiVisible
		{
			get { return isUiVisible; }
			private set { isUiVisible = value; }
		}

		private void Start()
		{
			_config = CmdConfigManager.Instance;

			if (_worldCameras.Length == 0)
			{
				Debug.LogWarning("No World Camera have been linked as reference.", gameObject);
			}
			else
			{
				// For Wall, use the first camera (it is a wall one), for floor, use the last one (it is a floor one).
				int camIndex = (_config.applicationType == CmdConfigManager.AppType.WALL ? 0 : (_worldCameras.Length - 1));
				inputEyeDistWorld.text = _worldCameras[camIndex].StereoSeparation.ToString();
				//// Just in case they are not equal at the beginning: Don't do this... Double invertation follows because of this...
				//for (int ii = 1; ii < _worldCameras.Length; ++ii)
				//{
				//	_worldCameras[ii].StereoSeparation = _worldCameras[0].StereoSeparation;
				//}
			}

			if (_stereoCanvasScaler.Length == 0)
			{
				Debug.LogWarning("No Stereo Canvas Scaler have been linked as reference.", gameObject);
			}
			else
			{
				// For Wall, use the first camera (it is a wall one), for floor, use the last one (it is a floor one).
				int canvasScalerIndex = (_config.applicationType == CmdConfigManager.AppType.WALL ? 0 : (_stereoCanvasScaler.Length - 1));
				inputUiCanvasScale.text = _stereoCanvasScaler[canvasScalerIndex].DefaultStereoScale.ToString();
				//// Just in case they are not equal at the beginning: This is not necessary.
				//for (int ii = 1; ii < _stereoCanvasScaler.Length; ++ii)
				//{
				//	_stereoCanvasScaler[ii].SetCanvasStereoScale(_stereoCanvasScaler[0].DefaultStereoScale);
				//}
			}

			// Default: Turn off the Debug UI:
#if !UNITY_EDITOR
			Cursor.visible = false;
#endif
			canvas.enabled = false;
			isUiVisible = false;

			inputEyeDistWorld.onEndEdit.AddListener(delegate { OnChangeEyeSeperationWorld(inputEyeDistWorld.text); });
			inputUiCanvasScale.onEndEdit.AddListener(delegate { OnChangeConvergenceDistScale(inputUiCanvasScale.text); });
		}

		private void Update()
		{
			// Press the specified key to enable or disable the debug menu:
			if (Input.GetKeyDown(_toggleDebugMenuKey))
			{
				EnableUi(!isUiVisible);
			}
		}

		public void EnableUi(bool enable)
		{
			if (isUiVisible != enable)
			{
				Cursor.visible = enable;
				canvas.enabled = enable;
				isUiVisible = enable;

				GameObject focusObject = null;
				if (enable == true)
				{
					focusObject = _controllerFocusObject;

					// Refresh Text-Fields in case that the values have been changed to display the correct information:
					// TODO: Duplicated code (see above).
					if (_worldCameras.Length > 0)
					{// For Wall, use the first camera (it is a wall one), for floor, use the last one (it is a floor one).
						int camIndex = (_config.applicationType == CmdConfigManager.AppType.WALL ? 0 : (_worldCameras.Length - 1));
						inputEyeDistWorld.text = _worldCameras[camIndex].StereoSeparation.ToString();
					}

					if (_stereoCanvasScaler.Length > 0)
					{
						// For Wall, use the first camera (it is a wall one), for floor, use the last one (it is a floor one).
						int canvasScalerIndex = (_config.applicationType == CmdConfigManager.AppType.WALL ? 0 : (_stereoCanvasScaler.Length - 1));
						inputUiCanvasScale.text = _stereoCanvasScaler[canvasScalerIndex].DefaultStereoScale.ToString();
					}
					// End of TODO.
				}

				EventSystem.current.SetSelectedGameObject(focusObject);
			}
		}

		public void OnChangeEyeSeperationWorld(string changedValue) // InputField inputField
		{
			//if (inputField.text.Length > 0)
			//{
			//	string changedValue = inputField.text;

			try
			{
				changedValue = changedValue.Replace(',', '.');
				Debug.Log(changedValue);
				float value = float.Parse(changedValue);
				for (int ii = 0; ii < _worldCameras.Length; ++ii)
				{
					_worldCameras[ii].StereoSeparation = value;
				}
				Debug.Log("OnChangeEyeSeperationWorld: " + value);
				inputEyeDistWorld.text = value.ToString();
			}
			catch
			{
				Debug.Log("Exception in OnChangeEyeSeperationWorld: " + changedValue);
				inputEyeDistWorld.text = "INVALID: " + changedValue;
			}
			//}
			//else
			//{
			//	Debug.Log("InputField is empty.");
			//}
		}

		public void OnChangeConvergenceDistScale(string changedValue)
		{
			try
			{
				changedValue = changedValue.Replace(',', '.');
				Debug.Log(changedValue);
				float value = float.Parse(changedValue);
				for (int ii = 0; ii < _stereoCanvasScaler.Length; ++ii)
				{
					_stereoCanvasScaler[ii].SetCanvasStereoScale(value);
				}
				Debug.Log("OnChangeConvergenceDistScale: " + value);
				inputUiCanvasScale.text = value.ToString();
			}
			catch
			{
				Debug.Log("Exception in OnChangeConvergenceDistScale: " + changedValue);
				inputEyeDistWorld.text = "INVALID: " + changedValue;
			}
		}

		// Getting a x-axis, so left or right.
		public void HandleAxisX(float dpadValue)
		{
			GameObject curSelectedObj = EventSystem.current.currentSelectedGameObject;
			if (curSelectedObj != null)
			{
				ControllerFocusHandler focusHandler = curSelectedObj.GetComponent<ControllerFocusHandler>();
				if (focusHandler != null)
				{
					focusHandler.HandleAxisX(dpadValue);
				}
			}
		}
	}
}