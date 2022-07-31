using UnityEngine;
using System.Collections;
using DeepSpace;
using DeepSpace.MobileControlMessages;

public class DemoTouchView : TouchView
{
	public Transform touchObject;
	public float minScaleFactor = 0.5f;
	public float maxScaleFactor = 2.0f;

	private Vector3 _startScale = default(Vector3);
	private Vector3 _currentScale;

	protected override void Awake()
	{
		base.Awake();

		_startScale = touchObject.localScale;
		_currentScale = _startScale;
	}

	public void Start()
	{
		// If you want to use this view, you have to register it at the MobileControlManager.
		// You can do it here like this, or from elsewhere.
		_mobileControlMgr.RegisterView(this);

		// If you do not want you view to be active anymore, you can unregister it by:
		//_mobileControlMgr.UnregisterView(this);
	}

	public override void EnableView()
	{
		base.EnableView();

		_startScale = touchObject.localScale;
		_currentScale = _startScale;
	}

	public override void DisableView()
	{
		base.DisableView();
	}

	protected override void OneFingerChanged(Finger finger1)
	{
		// Rotate cube via input:
		if (finger1.GetDelta() > float.Epsilon)
		{
			float rotationAngleX = -(finger1.LastPos.x - finger1.CurPos.x) * 150f;
			touchObject.Rotate(new Vector3(0f, 1.0f, 0f), -rotationAngleX, Space.World);

			float rotationAngleY = -(finger1.LastPos.y - finger1.CurPos.y) * 150f;
			touchObject.Rotate(new Vector3(1.0f, 0f, 0f), -rotationAngleY, Space.World);
		}
	}

	protected override void TwoFingersChanged(Finger finger1, Finger finger2)
	{
		// process finger infos -> Scale:
		if (finger1.GetDelta() > float.Epsilon || finger2.GetDelta() > float.Epsilon)
		{
			float lastDist = Vector2.Distance(finger1.LastPos, finger2.LastPos);
			float curDist = Vector2.Distance(finger1.CurPos, finger2.CurPos);

			//Debug.Log("LastDist: " + lastDist + ", CurDist: " + curDist);

			_currentScale += _startScale * (curDist - lastDist);

			if (curDist > lastDist) // Size up
			{
				if (_currentScale.magnitude > (_startScale * maxScaleFactor).magnitude) // Check max value
				{
					_currentScale = _startScale * maxScaleFactor;
				}
			}
			else if (curDist < lastDist) // Size down
			{
				if (_currentScale.magnitude < (_startScale * minScaleFactor).magnitude) // Check min value
				{
					_currentScale = _startScale * minScaleFactor;
				}
			}

			// Scale:
			touchObject.localScale = _currentScale;
		}
	}
}
