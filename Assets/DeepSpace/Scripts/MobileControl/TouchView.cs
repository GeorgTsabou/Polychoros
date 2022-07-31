using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.MobileControlMessages;

namespace DeepSpace
{
	public class TouchView : MobileControlView
	{
		protected float _lastMessageUpdate = 0f;
		protected float _maxMessagePause = 0.2f;

		public class Finger
		{
			public Finger(int id, Vector2 pos)
			{
				_id = id;
				_curPos = _lastPos = pos;
			}

			private int _id;
			private Vector2 _curPos;
			private Vector2 _lastPos;

			public int ID
			{
				get
				{
					return _id;
				}
				private set
				{
					_id = value;
				}
			}

			public Vector2 CurPos
			{
				get
				{
					return _curPos;
				}
				set
				{
					_lastPos = _curPos;
					_curPos = value;
				}
			}

			public Vector2 LastPos
			{
				get
				{
					return _lastPos;
				}
				private set
				{
					_lastPos = value;
				}
			}

			public float GetDelta()
			{
				return Vector2.Distance(CurPos, LastPos);
			}

			public Vector2 GetDirection()
			{
				return CurPos - LastPos;
			}
		}

		protected readonly int fingerAmount = 5;
		protected Finger[] fingers = null;

		protected void Init()
		{
			fingers = new Finger[fingerAmount];
			for (int ii = 0; ii < fingerAmount; ++ii)
			{
				fingers[ii] = new Finger(-1, Vector2.zero);
			}
		}

		protected override void Awake()
		{
			base.Awake();

			Init();
		}

		protected void Update()
		{
			if (_lastMessageUpdate > _maxMessagePause)
			{
				if (fingers[0].ID != -1) // If the first fingers ID is -1 (not in use), all other are not in use too.
				{
					// Clear touches:
					for (int ii = 0; ii < fingerAmount; ++ii)
					{
						fingers[ii] = new Finger(-1, Vector2.zero);
					}
				}
			}

			_lastMessageUpdate += Time.deltaTime;
		}

		public override void EnableView()
		{
			base.EnableView();

			Init(); // EnableView might be called before Awake was called...

			// Send iPod information for the required view:
			ShowTouchViewMessage touchViewMsg = new ShowTouchViewMessage(); // This view shows a view for finger touches on the mobile device

			_mobileControlMgr.tcpManager.SetViewMessage(touchViewMsg);
		}

		public override void DisableView()
		{
			base.DisableView();
		}

		public override void ProcessMessage(Message message)
		{
			if(fingers == null)
			{
				Debug.LogWarning("PrecessMessage was called, but fingers are not yet initialized. Fingers are not going to be updated!"
				+ "\nThis might happen, if you do not enable this View and call ProcessMessage() out of an Awake method!");
				return;
			}

			if (message.Command == Command.FINGER_POSITIONS)
			{
				// Display finger positions in the view:
				FingerPositionsMessage fingerPosMsg = new FingerPositionsMessage(message.Bytes);
				fingerPosMsg.UnpackReceivedMessage();

				// Recognize the finger update:
				if (fingerPosMsg.fingerPositions.Count > 0)
				{
					_lastMessageUpdate = 0.0f; // Reset

					// Update finger info:
					for (int ii = 0; ii < fingerPosMsg.fingerPositions.Count && ii < fingerAmount; ++ii)
					{
						if (fingers[ii].ID != fingerPosMsg.fingerPositions[ii].fingerID)
						{
							fingers[ii] = new Finger(fingerPosMsg.fingerPositions[ii].fingerID, new Vector2(fingerPosMsg.fingerPositions[ii].xPos, fingerPosMsg.fingerPositions[ii].yPos));
						}
						else
						{
							fingers[ii].CurPos = new Vector2(fingerPosMsg.fingerPositions[ii].xPos, fingerPosMsg.fingerPositions[ii].yPos);
						}
					}

					// Clear unused finger touches:
					for (int ii = fingerPosMsg.fingerPositions.Count; ii < fingerAmount; ++ii)
					{
						fingers[ii] = new Finger(-1, Vector2.zero);
					}

					switch (fingerPosMsg.fingerPositions.Count)
					{
						case 1:
							OneFingerChanged(fingers[0]);
							break;
						case 2:
							TwoFingersChanged(fingers[0], fingers[1]);
							break;
						case 3:
							ThreeFingersChanged(fingers[0], fingers[1], fingers[2]);
							break;
						case 4:
							FourFingersChanged(fingers[0], fingers[1], fingers[2], fingers[3]);
							break;
						case 5:
							FiveFingersChanged(fingers[0], fingers[1], fingers[2], fingers[3], fingers[4]);
							break;
						default:
							Debug.LogWarning("Only 5 fingers are implemented. The current FingerCount is " + fingerPosMsg.fingerPositions.Count + "."
								+ "\nFiveFingersChanged() is beeing called, but your results may be unexpected!");
							FiveFingersChanged(fingers[0], fingers[1], fingers[2], fingers[3], fingers[4]);
							break;
					}
				}
			}
		}

		protected virtual void OneFingerChanged(Finger finger1)
		{
			// Can be implemented by derived class. (e.g. for rotating)
		}

		protected virtual void TwoFingersChanged(Finger finger1, Finger finger2)
		{
			// Can be implemented by derived class. (e.g. for scaling)
		}

		protected virtual void ThreeFingersChanged(Finger finger1, Finger finger2, Finger finger3)
		{
			// Can be implemented by derived class. (e.g. for moving)
		}

		protected virtual void FourFingersChanged(Finger finger1, Finger finger2, Finger finger3, Finger finger4)
		{
			// Can be implemented by derived class.
		}

		protected virtual void FiveFingersChanged(Finger finger1, Finger finger2, Finger finger3, Finger finger4, Finger finger5)
		{
			// Can be implemented by derived class.
		}
	}
}