using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DeepSpace.MobileControlMessages;

namespace DeepSpace
{
	public abstract class MobileControlView : MonoBehaviour
	{
		protected int _viewID;
		public int ViewID
		{
			get
			{
				return _viewID;
			}
			set
			{
				_viewID = value;
			}
		}

		public string viewName;

		protected MobileControlManager _mobileControlMgr;

		protected bool _isActive = false; // Only one view can be active at a time!
		public bool IsActive
		{
			get
			{
				return _isActive;
			}
		}

		protected virtual void Awake()
		{
			_mobileControlMgr = MobileControlManager.Instance;
		}

		// This has to be implemented to interact with the Mobile Control.
		public abstract void ProcessMessage(Message message);

		public virtual void EnableView()
		{
			_isActive = true;
		}

		public virtual void DisableView()
		{
			_isActive = false;
		}

		public void RegisterThisView()
		{
			if (_mobileControlMgr == null) // Just in case this method gets called before the awake method was called...
			{
				_mobileControlMgr = MobileControlManager.Instance;
			}

			if (_mobileControlMgr != null)
			{
				_mobileControlMgr.RegisterView(this);
			}
			else
			{
				Debug.LogWarning("MobileControlManager.RegisterThisView(): Could not find MobileControlManager. View is not registered.", gameObject);
			}
		}

		public void UnregisterThisView()
		{
			if (_mobileControlMgr != null)
			{
				_mobileControlMgr.UnregisterView(this);
			}
			else
			{
				Debug.LogWarning("MobileControlManager.UnregisterThisView(): Could not find MobileControlManager. View has not been unregistered.", gameObject);
			}
		}
	}
}