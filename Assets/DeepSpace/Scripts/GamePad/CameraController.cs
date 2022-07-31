using UnityEngine;
using System.Collections;
using DeepSpace.Udp;

namespace DeepSpace
{
	public class CameraController : MonoBehaviour
	{
		public enum ControlMode
		{
			WORLD,
			UI
		}

		public float movementSpeed = 0.3f;

		public float rotationSpeed = 0.75f;
		[SerializeField, Range(0f, 0.99f)]
		private float _rotationDragFactor = 0.9f; // 0 disables drag completely, drag needs to be smaller than 1.
		private Vector2 _curRotationSpeed = Vector2.zero;
		[SerializeField, Range(0.01f, 1f)]
		private float _rotControllerApplyness = 0.1f; // How fast the camera rotation follows the controller input (1 direct input, 0.01 hardly following).

		public float zoomSpeedFactor = 0.01f;

		public AnimationCurve _rotationCorrectionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		private ControllerInputManager _controllerInput;
		[SerializeField]
		private Transform _networkTransform; // Moving this (type of UdpTransform) moves the local and remote instances of the camera, that are connected with this GameObject.

		private ControlMode _mode = ControlMode.WORLD;

#pragma warning disable 0649 // Ignore Unassigned Variable Warning
		[SerializeField]
		private DebugStereoManager _stereoMgr;
		[SerializeField]
		private UdpFieldOfViewSync _udpFovSync;
#pragma warning restore 0649

		private float _dPadInputStutter = 0.5f;
		private float _dPadStutterTemp = 0.5f;
		private float _curDPadInputStutter = 0f;

		private void Awake()
		{
			if (_rotationDragFactor >= 1f)
			{
				_rotationDragFactor = 0.99f;
			}
			else if (_rotationDragFactor < 0f)
			{
				_rotationDragFactor = 0f;
			}
		}

		private void Start()
		{
			if(_networkTransform == null)
			{
				_networkTransform = transform;
			}

			_controllerInput = ControllerInputManager.Instance;
			_controllerInput.RegisterButtonDownCallback(OnButtonDown);
		}

		private void Update()
		{
			if (_stereoMgr.IsUiVisible == true && _mode != ControlMode.UI)
			{
				_mode = ControlMode.UI;
			}
			else if (_stereoMgr.IsUiVisible == false && _mode != ControlMode.WORLD)
			{
				_mode = ControlMode.WORLD;
			}

			if (_mode == ControlMode.WORLD)
			{
				Vector2 leftJoystick = _controllerInput.LeftJoystick;
				Vector2 rightJoystick = _controllerInput.RightJoystick;
				float shoulderValueL2 = _controllerInput.ShoulderTriggerL2;
				float shoulderValueR2 = _controllerInput.ShoulderTriggerR2;
				Vector2 dPad = _controllerInput.DPad;

				// TODO: Implement Drag for Movement, as it is implemented for rotation.
				// Move Forward / Backward / Sidewards with the left Joystick
				if (leftJoystick.x.CompareTo(0f) != 0f)
				{
					_networkTransform.Translate(Vector3.right * leftJoystick.x * movementSpeed, Space.Self);
				}
				if (leftJoystick.y.CompareTo(0f) != 0f)
				{
					_networkTransform.Translate(Vector3.forward * leftJoystick.y * movementSpeed, Space.Self);
				}

				// Look around (Left, Right, Up, Down) with the right Joystick
				if (rightJoystick.x.CompareTo(0f) != 0)
				{
					if (_rotationDragFactor.CompareTo(0f) == 0) // In case of no drag (_rotationDragFactor == 0f), apply controller input directly:
					{
						_networkTransform.Rotate(Vector3.up, rightJoystick.x * rotationSpeed, Space.World);
					}
					else
					{
						//_curRotationSpeed.x = rightJoystick.x * rotationSpeed;

						if (rightJoystick.x > 0f)
						{
							float controlledMaxSpeed = rightJoystick.x * rotationSpeed;
							if (_curRotationSpeed.x < controlledMaxSpeed)
							{
								_curRotationSpeed.x += controlledMaxSpeed * _rotControllerApplyness;
								_curRotationSpeed.x = Mathf.Clamp(_curRotationSpeed.x, 0f, controlledMaxSpeed);
							}
						}
						else // rightJoystick.x < 0f
						{
							float controlledMinSpeed = rightJoystick.x * rotationSpeed;
							if (_curRotationSpeed.x > controlledMinSpeed)
							{
								_curRotationSpeed.x += controlledMinSpeed * _rotControllerApplyness;
								_curRotationSpeed.x = Mathf.Clamp(_curRotationSpeed.x, controlledMinSpeed, 0f);
							}
						}
					}
				}
				if (rightJoystick.y.CompareTo(0f) != 0)
				{
					if (_rotationDragFactor.CompareTo(0f) == 0) // In case of no drag (_rotationDragFactor == 0f), apply controller input directly:
					{
						_networkTransform.Rotate(Vector3.right, -rightJoystick.y * rotationSpeed, Space.Self);
					}
					else
					{
						//_curRotationSpeed.y = -rightJoystick.y * rotationSpeed;

						if (-rightJoystick.y > 0f)
						{
							float controlledMaxSpeed = -rightJoystick.y * rotationSpeed;
							if (_curRotationSpeed.y < controlledMaxSpeed)
							{
								_curRotationSpeed.y += controlledMaxSpeed * _rotControllerApplyness;
								_curRotationSpeed.y = Mathf.Clamp(_curRotationSpeed.y, 0f, controlledMaxSpeed);
							}
						}
						else // -rightJoystick.y < 0f
						{
							float controlledMinSpeed = -rightJoystick.y * rotationSpeed;
							if (_curRotationSpeed.y > controlledMinSpeed)
							{
								_curRotationSpeed.y += controlledMinSpeed * _rotControllerApplyness;
								_curRotationSpeed.y = Mathf.Clamp(_curRotationSpeed.y, controlledMinSpeed, 0f);
							}
						}
					}
				}

				// Apply rotation drag:
				if (_rotationDragFactor > 0f && (_curRotationSpeed.x.CompareTo(0f) != 0 || _curRotationSpeed.y.CompareTo(0f) != 0))
				{
					_networkTransform.Rotate(Vector3.up, _curRotationSpeed.x, Space.World);
					_networkTransform.Rotate(Vector3.right, _curRotationSpeed.y, Space.Self);

					_curRotationSpeed *= _rotationDragFactor;
					if (_curRotationSpeed.x > -0.001f && _curRotationSpeed.x < 0.001f)
					{
						_curRotationSpeed.x = 0f;
					}
					if (_curRotationSpeed.y > -0.001f && _curRotationSpeed.y < 0.001f)
					{
						_curRotationSpeed.y = 0f;
					}
				}

				// Move Up / Down with the Shoulder Triggers
				if (shoulderValueR2 > 0f)
				{
					_networkTransform.Translate(Vector3.up * shoulderValueR2 * movementSpeed, Space.World);
				}
				if (shoulderValueL2 > 0f)
				{
					_networkTransform.Translate(Vector3.up * -shoulderValueL2 * movementSpeed, Space.World);
				}

				// Change field of view:
				if (dPad.y.CompareTo(0f) != 0f)
				{
					if (_udpFovSync != null)
					{
						_udpFovSync.ChangeFOV(-dPad.y * zoomSpeedFactor);
					}
				}
				if (dPad.x.CompareTo(0f) != 0f)
				{
					if (_udpFovSync != null)
					{
						if (dPad.x > 0.9f)
						{
							_udpFovSync.ResetFOV();
						}
					}
				}
			}
			else if (_mode == ControlMode.UI)
			{
				Vector2 dPad = _controllerInput.DPad;
				Vector2 leftJoystick = _controllerInput.LeftJoystick;

				float xAxisValue = 0f;

				if (dPad.x.CompareTo(0f) != 0f)
				{
					xAxisValue = dPad.x;
				}
				else if (leftJoystick.x.CompareTo(0f) != 0f)
				{
					xAxisValue = (leftJoystick.x > 0 ? 1f : -1f);
				}

				if (xAxisValue.CompareTo(0f) != 0f)
				{
					_curDPadInputStutter -= Time.deltaTime;
					if (_curDPadInputStutter <= 0f)
					{
						_stereoMgr.HandleAxisX(xAxisValue);

						_dPadStutterTemp *= 0.9f;
						_curDPadInputStutter = _dPadStutterTemp;
					}
				}
				else
				{
					_curDPadInputStutter = _dPadInputStutter;
					_dPadStutterTemp = _dPadInputStutter;
					_curDPadInputStutter = 0f;
				}
			}
		}

		private void OnButtonDown(ControllerInputManager.Button button)
		{
			if (_mode == ControlMode.WORLD)
			{
				// Rotate to normal horizontal with R3:
				if (button == ControllerInputManager.Button.R3)
				{
					StartCoroutine(RotateToHorizon(0.5f));
				}

				// Switch between ViewPoints with the R1 / L1 Buttons
				// TODO...

				// TODO: Use a button to switch between day and night... (rotate Directional light).
			}
			else if (_mode == ControlMode.UI)
			{
				// TODO: Maybe some ui input want to work with buttons (e.g. a bool toggle, press x to enable and disable it).
			}

			// Change controller mode:
			if (button == ControllerInputManager.Button.START)
			{
				if (_mode == ControlMode.WORLD)
				{
					_mode = ControlMode.UI;

					_stereoMgr.EnableUi(true);
				}
				else if (_mode == ControlMode.UI)
				{
					_mode = ControlMode.WORLD;

					_stereoMgr.EnableUi(false);
				}
			}
		}

		private IEnumerator RotateToHorizon(float time)
		{
			float curTime = 0.0f;

			Vector3 forward = _networkTransform.forward;
			forward.y = 0.0f;
			forward.Normalize();

			Quaternion startRotation = _networkTransform.localRotation;
			Quaternion aimRotation = Quaternion.LookRotation(forward, Vector3.up);

			while (curTime <= time)
			{
				_networkTransform.localRotation = Quaternion.Slerp(startRotation, aimRotation, _rotationCorrectionCurve.Evaluate(curTime / time));

				curTime += Time.smoothDeltaTime;
				yield return null;
			}

			// fix rotation finally...
			_networkTransform.localRotation = aimRotation;
		}
	}
}