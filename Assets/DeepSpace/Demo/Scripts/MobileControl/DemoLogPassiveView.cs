using UnityEngine;
using System.Collections;
using DeepSpace;
using DeepSpace.MobileControlMessages;

public class DemoLogPassiveView : MobileControlPassiveView
{
	public override void GotMessage(DeepSpace.MobileControlMessages.Message message)
	{
		// Sending and Receiving message have the same Command value, but the enum shall be printed correctly for receiving commands:
		string result;
		switch (message.Command)
		{
			// Receiving:
			case Command.GET_WINDOWS:
				result = "GET_WINDOWS";
				break;
			case Command.GET_WINDOW_BY_NUMBER:
				result = "GET_WINDOW_BY_NUMBER";
				break;
			case Command.CHOSE_OPTION:
				result = "CHOSE_OPTION";
				break;
			case Command.SET_SLIDER_VALUE:
				result = "SET_SLIDER_VALUE";
				break;
			case Command.PRESS_OPTION:
				result = "PRESS_OPTION";
				break;
			case Command.RELEASE_OPTION:
				result = "RELEASE_OPTION";
				break;
			case Command.FINGER_POSITIONS:
				result = "FINGER_POSITIONS";
				break;
			case Command.CHOSE_LIST_ENTRY:
				result = "CHOSE_LIST_ENTRY";
				break;
			case Command.GET_SCENE_MENU:
				result = "GET_SCENE_MENU";
				break;
			case Command.USE_MICRO_STORY_CONTROL:
				result = "USE_MICRO_STORY_CONTROL";
				break;
			// Bidirectional:
			case Command.CHOSE_SCENE:
				result = "CHOSE_SCENE";
				break;
			case Command.HEART_BEAT:
				result = "HEART_BEAT";
				break;
			default:
				result = "UNKNOWN";
				break;
		}
		Debug.Log("DemoLogPassiveView received message " + result);
	}
}
