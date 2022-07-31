using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DemoEnableDisableViews : MonoBehaviour
{
	public Text buttonText1;
	public Text buttonText2;
	public Text buttonText3;

	public DemoNavigationView navigationView;
	public DemoTouchView touchView;
	public DemoLogPassiveView passiveView;

	private bool navigationEnabled = true;
	private bool touchEnabled = true;
	private bool passiveEnabled = true;

	private void Awake()
	{
		buttonText1.text = "Unregister NavigationView";
		buttonText2.text = "Unregister TouchView";
		buttonText3.text = "Unregister PassiveView";
	}

	public void EnableDisableNavigation()
	{
		if(navigationEnabled)
		{
			navigationView.UnregisterThisView();
			buttonText1.text = "Register NavigationView";
		}
		else
		{
			navigationView.RegisterThisView();
			buttonText1.text = "Unregister NavigationView";
		}
		navigationEnabled = !navigationEnabled;
	}

	public void EnableDisableTouch()
	{
		if (touchEnabled)
		{
			touchView.UnregisterThisView();
			buttonText2.text = "Register TouchView";
		}
		else
		{
			touchView.RegisterThisView();
			buttonText2.text = "Unregister TouchView";
		}
		touchEnabled = !touchEnabled;
	}

	public void EnableDisablePassive()
	{
		if (passiveEnabled)
		{
			passiveView.UnregisterThisView();
			buttonText3.text = "Register PassiveView";
		}
		else
		{
			passiveView.RegisterThisView();
			buttonText3.text = "Unregister PassiveView";
		}
		passiveEnabled = !passiveEnabled;
	}
}
