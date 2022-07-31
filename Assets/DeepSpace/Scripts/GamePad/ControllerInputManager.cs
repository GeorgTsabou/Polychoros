#undef VERBOSE // Use #define to print controller output
//#define VERBOSE

using UnityEngine;
using System.Collections;

namespace DeepSpace
{
	public class ControllerInputManager : MonoBehaviour
	{
		#region Singleton
		private static ControllerInputManager _instance;

		public static ControllerInputManager Instance
		{
			get
			{
				// If _instance hasn't been set yet, we grab it from the scene!
				// This will only happen the first time this reference is used.
				if (_instance == null)
				{
					_instance = GameObject.FindObjectOfType<ControllerInputManager>();
				}

				return _instance;
			}
		}
		#endregion

		public enum Button
		{
			A,
			B,
			X,
			Y,
			L1,
			R1,
			BACK,
			START,
			L3,
			R3
		}

		public delegate void OnButtonEvent(Button button);

		private event OnButtonEvent OnButtonDown;
		private event OnButtonEvent OnButtonHold;
		private event OnButtonEvent OnButtonUp;

		private readonly string _axisVerticalLeft = "DS_VerticalLeft";
		private readonly string _axisHorizontalLeft = "DS_HorizontalLeft";
		private readonly string _axisVerticalRight = "DS_VerticalRight";
		private readonly string _axisHorizontalRight = "DS_HorizontalRight";
		private readonly string _axisDPadVertical = "DS_DPadVertical";
		private readonly string _axisDPadHorizontal = "DS_DPadHorizontal";
		private readonly string _axisShoulderTriggerL2 = "DS_ShoulderTriggerL2";
		private readonly string _axisShoulderTriggerR2 = "DS_ShoulderTriggerR2";

		private bool _axisValidation = true;

		public void RegisterButtonDownCallback(OnButtonEvent callbackMethod)
		{
			OnButtonDown += callbackMethod;
		}

		public void RegisterButtonHoldCallback(OnButtonEvent callbackMethod)
		{
			OnButtonHold += callbackMethod;
		}

		public void RegisterButtonUpCallback(OnButtonEvent callbackMethod)
		{
			OnButtonUp += callbackMethod;
		}

		public void UnregisterButtonDownCallback(OnButtonEvent callbackMethod)
		{
			OnButtonDown -= callbackMethod;
		}

		public void UnregisterButtonHoldCallback(OnButtonEvent callbackMethod)
		{
			OnButtonHold -= callbackMethod;
		}

		public void UnregisterButtonUpCallback(OnButtonEvent callbackMethod)
		{
			OnButtonUp -= callbackMethod;
		}

		private void CallOnButtonDown(Button button)
		{
			if (OnButtonDown != null)
			{
				OnButtonDown(button);
			}
		}

		private void CallOnButtonHold(Button button)
		{
			if (OnButtonHold != null)
			{
				OnButtonHold(button);
			}
		}

		private void CallOnButtonUp(Button button)
		{
			if (OnButtonUp != null)
			{
				OnButtonUp(button);
			}
		}

		// Input Values:
		public Vector2 LeftJoystick
		{
			get;
			private set;
		}

		public Vector2 RightJoystick
		{
			get;
			private set;
		}

		public Vector2 DPad
		{
			get;
			private set;
		}

		public float ShoulderTriggerR2
		{
			get;
			private set;
		}

		public float ShoulderTriggerL2
		{
			get;
			private set;
		}

		// Check if Joystick Values are configured:
		private void Start()
		{
			CheckAxis(_axisVerticalLeft);
			CheckAxis(_axisHorizontalLeft);
			CheckAxis(_axisVerticalRight);
			CheckAxis(_axisHorizontalRight);
			CheckAxis(_axisDPadVertical);
			CheckAxis(_axisDPadHorizontal);
			CheckAxis(_axisShoulderTriggerL2);
			CheckAxis(_axisShoulderTriggerR2);
		}

		// Read Joystick Values:
		void Update()
		{
			#region GetKeyDown
			if (Input.GetKeyDown(KeyCode.JoystickButton0))
			{
				CallOnButtonDown(Button.A);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> A");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton1))
			{
				CallOnButtonDown(Button.B);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> B");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton2))
			{
				CallOnButtonDown(Button.X);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> X");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton3))
			{
				CallOnButtonDown(Button.Y);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> Y");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton4))
			{
				CallOnButtonDown(Button.L1);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> L1");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton5))
			{
				CallOnButtonDown(Button.R1);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> R1");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton6))
			{
				CallOnButtonDown(Button.BACK);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> Back");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton7))
			{
				CallOnButtonDown(Button.START);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> Start");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton8))
			{
				CallOnButtonDown(Button.L3);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> L3");
#endif
			}
			if (Input.GetKeyDown(KeyCode.JoystickButton9))
			{
				CallOnButtonDown(Button.R3);
#if VERBOSE
			Debug.Log("InputManager: Button Down -> R3");
#endif
			}
			#endregion

			#region GetKey
			if (Input.GetKey(KeyCode.JoystickButton0))
			{
				CallOnButtonHold(Button.A);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> A");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton1))
			{
				CallOnButtonHold(Button.B);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> B");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton2))
			{
				CallOnButtonHold(Button.X);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> X");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton3))
			{
				CallOnButtonHold(Button.Y);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> Y");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton4))
			{
				CallOnButtonHold(Button.L1);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> L1");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton5))
			{
				CallOnButtonHold(Button.R1);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> R1");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton6))
			{
				CallOnButtonHold(Button.BACK);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> Back");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton7))
			{
				CallOnButtonHold(Button.START);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> Start");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton8))
			{
				CallOnButtonHold(Button.L3);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> L3");
#endif
			}
			if (Input.GetKey(KeyCode.JoystickButton9))
			{
				CallOnButtonHold(Button.R3);
#if VERBOSE
			Debug.Log("InputManager: Button Hold -> R3");
#endif
			}
			#endregion

			#region GetKeyUp
			if (Input.GetKeyUp(KeyCode.JoystickButton0))
			{
				CallOnButtonUp(Button.A);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> A");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton1))
			{
				CallOnButtonUp(Button.B);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> B");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton2))
			{
				CallOnButtonUp(Button.X);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> X");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton3))
			{
				CallOnButtonUp(Button.Y);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> Y");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton4))
			{
				CallOnButtonUp(Button.L1);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> L1");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton5))
			{
				CallOnButtonUp(Button.R1);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> R1");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton6))
			{
				CallOnButtonUp(Button.BACK);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> Back");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton7))
			{
				CallOnButtonUp(Button.START);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> Start");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton8))
			{
				CallOnButtonUp(Button.L3);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> L3");
#endif
			}
			if (Input.GetKeyUp(KeyCode.JoystickButton9))
			{
				CallOnButtonUp(Button.R3);
#if VERBOSE
			Debug.Log("InputManager: Button Up -> R3");
#endif
			}
			#endregion

			if (_axisValidation == false)
			{
				return;
			}

			#region JoystickAxes
			// Left Joystick Axis - Y Axis
			float leftJoystickVertical = Input.GetAxis(_axisVerticalLeft);
#if VERBOSE
		if (leftJoystickVertical.CompareTo(0) != 0) // Not 0.0f
		{
			Debug.Log("Left Vertical: " + leftJoystickVertical);
		}
#endif

			// Left Joystick Axis - X Axis
			float leftJoystickHorizontal = Input.GetAxis(_axisHorizontalLeft);
#if VERBOSE
		if (leftJoystickHorizontal.CompareTo(0) != 0) // Not 0.0f
		{
			Debug.Log("Left Horizontal: " + leftJoystickHorizontal);
		}
#endif

			LeftJoystick = new Vector2(leftJoystickHorizontal, leftJoystickVertical);

			// Right Joystick Axis - 4th Axis
			float rightJoystickHorizontal = Input.GetAxis(_axisHorizontalRight);
#if VERBOSE
		if (rightJoystickHorizontal.CompareTo(0) != 0) // Not 0.0f
		{
			Debug.Log("Right Horizontal: " + rightJoystickHorizontal);
		}
#endif

			// Right Joystick Axis - 5th Axis
			float rightJoystickVertical = Input.GetAxis(_axisVerticalRight);
#if VERBOSE
		if (rightJoystickVertical.CompareTo(0) != 0) // Not 0.0f
		{
			Debug.Log("Right Vertical: " + rightJoystickVertical);
		}
#endif

			RightJoystick = new Vector2(rightJoystickHorizontal, rightJoystickVertical);

			// DPad Axis - 6th Axis
			float dPadHorizontal = Input.GetAxis(_axisDPadHorizontal);
#if VERBOSE
			if (dPadHorizontal.CompareTo(0) != 0) // Not 0.0f
			{
				Debug.Log("D-Pad Horizontal: " + dPadHorizontal);
			}
#endif

			// Right Joystick Axis - 5th Axis
			float dPadVertical = Input.GetAxis(_axisDPadVertical);
#if VERBOSE
			if (dPadVertical.CompareTo(0) != 0) // Not 0.0f
			{
				Debug.Log("D-Pad Vertical: " + dPadVertical);
			}
#endif

			DPad = new Vector2(dPadHorizontal, dPadVertical);

			// L2 Shoulder Trigger Axis - 9th Axis
			float shoulderTriggerValueL2 = Input.GetAxis(_axisShoulderTriggerL2);
			ShoulderTriggerL2 = shoulderTriggerValueL2;
#if VERBOSE
		if (shoulderTriggerValueL2.CompareTo(0) != 0) // Not 0.0f
		{
			if (shoulderTriggerValueL2 > 0)
			{
				Debug.Log("L2: " + shoulderTriggerValueL2);
			}
		}
#endif

			// R2 Shoulder Trigger Axis - 10th Axis
			float shoulderTriggerValueR2 = Input.GetAxis(_axisShoulderTriggerR2);
			ShoulderTriggerR2 = shoulderTriggerValueR2;
#if VERBOSE
		if (shoulderTriggerValueR2.CompareTo(0) != 0) // Not 0.0f
		{
			if (shoulderTriggerValueR2 > 0)
			{
				Debug.Log("R2: " + shoulderTriggerValueR2);
			}
		}
#endif


			#endregion
		}

		private void CheckAxis(string axisName)
		{
			if (IsAxisAvailable(axisName) == false)
			{
				Debug.LogError("ControllerInputManager will not work: " + axisName + " is not defined in Input Project Settings.\n"
							 + "Please have a look into the documentation. Appendix A lists the needed InputManager.asset configuration.");
				_axisValidation = false;
			}
		}

		private bool IsAxisAvailable(string axisName)
		{
			try
			{
				Input.GetAxis(axisName);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}