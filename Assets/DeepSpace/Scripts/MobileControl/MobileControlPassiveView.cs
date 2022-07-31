using UnityEngine;
using System.Collections;
using DeepSpace.MobileControlMessages;

namespace DeepSpace
{
	public abstract class MobileControlPassiveView : MonoBehaviour
	{
		protected MobileControlManager _mobileControlMgr;

		protected virtual void Awake()
		{
			_mobileControlMgr = MobileControlManager.Instance;

			RegisterThisView();
		}

		public abstract void GotMessage(Message message);

		public void RegisterThisView()
		{
			_mobileControlMgr.RegisterPassiveView(this);
		}

		public void UnregisterThisView()
		{
			_mobileControlMgr.UnregisterPassiveView(this);
		}
	}
}
