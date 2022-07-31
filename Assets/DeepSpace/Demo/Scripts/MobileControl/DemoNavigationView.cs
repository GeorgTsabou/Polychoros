using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeepSpace;
using DeepSpace.MobileControlMessages;

public class DemoNavigationView : MobileControlView
{
	public MobileControlTcpManager tcpManager;
	public Transform moveObject;

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

		ShowViewMessageOnMobile();
	}

	private void ShowViewMessageOnMobile()
	{
		// Send information for the required view to mobile device:
		ShowSimpleViewMessage navigationViewMsg = new ShowSimpleViewMessage(Command.SHOW_NAVIGATION_VIEW); // This shows a navigation view on the mobile device
		navigationViewMsg.namedButtonList.Add(new KeyValuePair<uint, string>((uint)ChoseOptionMessage.Select.RIGHT, "Right")); // Set Name
		navigationViewMsg.namedButtonList.Add(new KeyValuePair<uint, string>((uint)ChoseOptionMessage.Select.LEFT, "Left")); // Set Name
		navigationViewMsg.disabledButtonList.Add((uint)ChoseOptionMessage.Select.UP); // Disable the up button.
		navigationViewMsg.disabledButtonList.Add((uint)ChoseOptionMessage.Select.DOWN); // Disable the down button.

		// Send view message to the mobile control:
		_mobileControlMgr.tcpManager.SetViewMessage(navigationViewMsg);
	}

	public override void ProcessMessage(Message message)
	{
		// A message came from the mobile control:
		if (message.Command == Command.CHOSE_OPTION) // Check if it is a button press
		{
			ChoseOptionMessage choseMessage = new ChoseOptionMessage(message.Bytes);
			choseMessage.UnpackReceivedMessage();
			// Check which option (button) was pressed: (you only need to check for left and right, because these are the only enabled ones)
			if (choseMessage.ChosenOption == (uint)ChoseOptionMessage.Select.RIGHT)
			{
				// Right button was pressed, do whatever you want with this information...
				moveObject.Translate(Vector3.right, Space.World);
			}
			else if (choseMessage.ChosenOption == (uint)ChoseOptionMessage.Select.LEFT)
			{
				// Left button was pressed, do whatever you want with this information...
				moveObject.Translate(-Vector3.right, Space.World);
			}
		}
	}
}
