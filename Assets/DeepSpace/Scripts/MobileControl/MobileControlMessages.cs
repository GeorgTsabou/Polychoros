using System;
using UnityEngine;
using System.Collections.Generic;

namespace DeepSpace.MobileControlMessages
{
	// Add as many as you like, max. 255.
	public enum Command
	{
		UNKNOWN = 0,
		// Receiving Message Types:
		GET_WINDOWS = 1,
		GET_WINDOW_BY_NUMBER = 2,
		CHOSE_OPTION = 3,
		SET_SLIDER_VALUE = 4,
		PRESS_OPTION = 5,
		RELEASE_OPTION = 6,
		FINGER_POSITIONS = 7,
		CHOSE_LIST_ENTRY = 8,
		GET_SCENE_MENU = 9,
		USE_MICRO_STORY_CONTROL = 10,
		// Sending Message Types:
		WINDOW_LIST = 1,
		SHOW_NAVIGATION_VIEW = 2,
		SHOW_VIDEO_VIEW = 3, // Do not use this view.
		SHOW_SLIDER_VIEW = 4,
		SHOW_TOUCH_VIEW = 5,
		SHOW_LIST_VIEW = 6,
		SHOW_QUIZ_VIEW = 7, // Do not use this view.
		SCENE_MENU = 8,
		// Bidirectional Message Types:
		CHOSE_SCENE = 254,
		HEART_BEAT = 255
	}

	public class Message
	{
		protected Command _command; // The type of message, that is sent.
									//protected uint _netID; // The ID of the Object, that shall receive the message.
									//protected int _msgID; // TODO: The ID of the message, that is sent. The higher the msgID, the later the message was sent.
		protected byte[] _bytes;

		public Command Command
		{
			get
			{
				return _command;
			}
		}

		public byte ByteCommand
		{
			get
			{
				return (byte)_command;
			}
		}

		public byte[] Bytes
		{
			get
			{
				return _bytes;
			}
		}

		//public uint NetworkID
		//{
		//	get
		//	{
		//		return _netID;
		//	}
		//}

		// Copy constructor
		public Message(Message message)
		{
			_bytes = message._bytes;
			_command = message._command;
		}

		public Message(byte[] bytes, Command command)
		{
			_bytes = bytes;
			_command = command;
			//_netID = netID;
		}

		// Returns a message in bytes, that contains all information of the member variables in this class:
		public virtual byte[] GetMessageBytes()
		{
			throw new NotImplementedException("Please only use a derived type for GetMessageBytes()");
		}

		// Returns the amount of read bytes for this message:
		public virtual int UnpackReceivedMessage()
		{
			throw new NotImplementedException("Please only use a derived type for UnpackReceivedMessage()");
		}

		public static Command ReadMessageCommand(byte[] bytes)
		{
			int index = 0; // 0 -> Command Byte.
			Command command = Command.UNKNOWN;

			try
			{
				command = (Command)MobileControlPackHelper.UnpackByte(bytes, ref index); // Message Type
			}
			catch (System.Exception)
			{
				command = Command.UNKNOWN;
			}

			return command;
		}

		// This method has to be updated for each newly added Receiving & Bidirectional Message class.
		// This method returns a Type-Correct message for a message-byte-stream.
		public static Message GetReceivedMessage(byte[] bytes)
		{
			Command command = ReadMessageCommand(bytes);
			Message result = null;

			if (command != Command.UNKNOWN)
			{
				switch (command)
				{
					// Receiving:
					case Command.GET_WINDOWS:
						result = new GetWindowsMessage(bytes);
						break;
					case Command.GET_WINDOW_BY_NUMBER:
						result = new GetWindowByNumberMessage(bytes);
						break;
					case Command.CHOSE_OPTION:
						result = new ChoseOptionMessage(bytes);
						break;
					case Command.SET_SLIDER_VALUE:
						result = new SetSliderValueMessage(bytes);
						break;
					case Command.PRESS_OPTION:
						result = new PressOptionMessage(bytes);
						break;
					case Command.RELEASE_OPTION:
						result = new ReleaseOptionMessage(bytes);
						break;
					case Command.FINGER_POSITIONS:
						result = new FingerPositionsMessage(bytes);
						break;
					case Command.CHOSE_LIST_ENTRY:
						result = new ChoseListEntryMessage(bytes);
						break;
					case Command.GET_SCENE_MENU:
						result = new GetSceneMenuMessage(bytes);
						break;
					case Command.USE_MICRO_STORY_CONTROL:
						result = new UseMicroStoryControlMessage(bytes);
						break;
					// Bidirectional:
					case Command.CHOSE_SCENE:
						result = new ChoseSceneMessage(bytes);
						break;
					case Command.HEART_BEAT:
						result = new HeartBeatMessage(bytes);
						break;
					default:
						break;
				}
			}

			return result;
		}

		protected int GetBaseSize()
		{
			// Sizes of: Command + NetworkID
			// return sizeof(byte) + sizeof(uint); // Extend this, if more base variables are added.

			return sizeof(byte); // Command
		}

		protected void PackDuties(ref int index)
		{
			MobileControlPackHelper.PackByte(ref _bytes, ref index, ByteCommand); // Message Type (1)
																				  //MobileControlPackHelper.PackUint(ref message, ref index, NetworkID); // Network ID (4)
		}

		protected void UnpackDuties(ref int index)
		{
			Command cmd = (Command)MobileControlPackHelper.UnpackByte(_bytes, ref index); // Message Type
																						  //_netID = MobileControlPackHelper.UnpackUint(bytes, ref index); // Network ID

			if (cmd != Command)
			{
				Debug.LogError("Received Command is not the expected one!\n"
					+ "Expected: " + Enum.GetName(typeof(Command), _command) + ", Received: " + Enum.GetName(typeof(Command), cmd));
			}
		}
	}

	// ------------- Receiving Messages -------------

	public class GetWindowsMessage : Message
	{
		public GetWindowsMessage(byte[] bytes)
			: base(bytes, Command.GET_WINDOWS)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			return index;
		}
	}

	public class GetWindowByNumberMessage : Message
	{
		private byte _windowNumber;

		public uint WindowNumber
		{
			get
			{
				return Convert.ToUInt32(_windowNumber);
			}
			private set
			{
				_windowNumber = Convert.ToByte(value);
			}
		}

		public GetWindowByNumberMessage(byte[] bytes)
			: base(bytes, Command.GET_WINDOW_BY_NUMBER)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			_windowNumber = MobileControlPackHelper.UnpackByte(_bytes, ref index); // Window number

			return index;
		}
	}

	// A chosen option Message is context sensitive and depends on the current state of the Unity Application.
	// Zero (0) for example might be up in a control, play a video, or the first list entry.
	public class ChoseOptionMessage : Message
	{
		public enum Select : uint
		{
			UP = 0,
			RIGHT = 1,
			DOWN = 2,
			LEFT = 3,
		}

		public enum Video : uint
		{
			PLAY = 0,
			PAUSE = 1,
			RESET = 2,
		}

		protected byte _chosenOption;

		public uint ChosenOption
		{
			get
			{
				return Convert.ToUInt32(_chosenOption);
			}
			set
			{
				_chosenOption = Convert.ToByte(value);
			}
		}

		public ChoseOptionMessage(byte[] bytes)
			: base(bytes, Command.CHOSE_OPTION)
		{
		}

		protected ChoseOptionMessage(byte[] bytes, Command cmd)
			: base(bytes, cmd)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			_chosenOption = MobileControlPackHelper.UnpackByte(_bytes, ref index); // chosenOption

			return index;
		}
	}

	public class PressOptionMessage : ChoseOptionMessage
	{
		public PressOptionMessage(byte[] bytes)
			: base(bytes, Command.PRESS_OPTION)
		{
		}
	}

	public class ReleaseOptionMessage : ChoseOptionMessage
	{
		public ReleaseOptionMessage(byte[] bytes)
			: base(bytes, Command.RELEASE_OPTION)
		{
		}
	}

	public class SetSliderValueMessage : Message
	{
		public int sliderIndex;
		public float curSliderValue;

		public SetSliderValueMessage(byte[] bytes)
			: base(bytes, Command.SET_SLIDER_VALUE)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			sliderIndex = Convert.ToInt32(MobileControlPackHelper.UnpackByte(_bytes, ref index)); // Index of the changed slider
			curSliderValue = MobileControlPackHelper.UnpackFloat(_bytes, ref index); // Current Slider Value

			return index;
		}
	}

	public class FingerPositionsMessage : Message
	{
		public class FingerPosition
		{
			public int fingerID;
			public float xPos;
			public float yPos;
		}

		public List<FingerPosition> fingerPositions;

		public FingerPositionsMessage(byte[] bytes)
			: base(bytes, Command.FINGER_POSITIONS)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			int fingerAmount = Convert.ToInt32(MobileControlPackHelper.UnpackByte(_bytes, ref index)); // Amount of fingers on the touch display

			fingerPositions = new List<FingerPosition>(fingerAmount);
			for (int ii = 0; ii < fingerAmount; ++ii)
			{
				FingerPosition fingerPos = new FingerPosition();
				fingerPos.fingerID = Convert.ToInt32(MobileControlPackHelper.UnpackByte(_bytes, ref index)); // Current Finger ID
				fingerPos.xPos = MobileControlPackHelper.UnpackFloat(_bytes, ref index); // Current Finger xPos
				fingerPos.yPos = MobileControlPackHelper.UnpackFloat(_bytes, ref index); // Current Finger yPos

				fingerPositions.Add(fingerPos);
			}

			return index;
		}
	}

	public class ChoseListEntryMessage : Message
	{
		public int chosenIndex;

		public ChoseListEntryMessage(byte[] bytes)
			: base(bytes, Command.CHOSE_LIST_ENTRY)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			chosenIndex = MobileControlPackHelper.UnpackInt(_bytes, ref index); // Index of the chosen list entry.

			return index;
		}
	}

	public class GetSceneMenuMessage : Message
	{
		public GetSceneMenuMessage(byte[] bytes)
			: base(bytes, Command.GET_SCENE_MENU)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			return index;
		}
	}

	public class UseMicroStoryControlMessage : Message
	{
		public UseMicroStoryControlMessage(byte[] bytes)
			: base(bytes, Command.USE_MICRO_STORY_CONTROL)
		{
		}

		public override byte[] GetMessageBytes()
		{
			throw new NotImplementedException("This Message Type is only beeing received, not sended!");
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			return index;
		}
	}

	// ------------- Sending Messages -------------

	public class WindowListMessage : Message
	{
		byte currentIndex;
		List<string> windowList;

		public WindowListMessage(List<string> windowList, uint currentIndex)
			: base(null, Command.WINDOW_LIST)
		{
			this.windowList = windowList;
			this.currentIndex = Convert.ToByte(currentIndex);
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + 1 + sizeof(int) + windowList.Count; // BaseSize + byte + list entries + 1 byte for each string length ...
			foreach (string str in windowList)
			{
				msgLen += str.Length; // ... + 1 byte for each character in each string.
			}

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, currentIndex); // ID of the currently shown window
			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(windowList.Count)); // Amount of list entries

			// For each list entry:
			for (int ii = 0; ii < windowList.Count; ++ii)
			{
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(windowList[ii].Length)); // Amound of letters for this element
				MobileControlPackHelper.PackString(ref _bytes, ref index, windowList[ii]); // list entry (letter for letter, not null terminated)
			}

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	// A simple view is a view, that is only communicated by the view type byte (Command) and some disabled button information.
	// Currently, the only simple view is the SHOW_NAVIGATION_VIEW.
	public class ShowSimpleViewMessage : Message
	{
		// e.g. disabledButtonList.Add((uint)ChoseOptionMessage.Select.LEFT);
		public List<uint> disabledButtonList = new List<uint>();
		// e.g. namedButtonList.Add(new KeyValuePair<uint, string>((uint)ChoseOptionMessage.Select.RIGHT, "Enter"));
		public List<KeyValuePair<uint, string>> namedButtonList = new List<KeyValuePair<uint, string>>();

		public ShowSimpleViewMessage(Command cmd)
			: base(null, cmd)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + 1 + disabledButtonList.Count + 1 + namedButtonList.Count; // BaseSize + 1 amount of optional bytes + optional bytes (disabled and named amount)
																								   // + 1 amount of named buttons + amount of strings.

			foreach (KeyValuePair<uint, string> entry in namedButtonList)
			{
				msgLen += entry.Value.Length + 1; // ... + x bytes (for all characters of each "named button" string) + 1 byte (for amount of chars).
			}

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(disabledButtonList.Count)); // Amount of optional disabled button bytes

			foreach (uint disabledOption in disabledButtonList)
			{
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(disabledOption)); // For each disabled button, add the button id.
			}

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(namedButtonList.Count)); // Amount of optional named buttons

			foreach (KeyValuePair<uint, string> entry in namedButtonList)
			{
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(entry.Key)); // For each named button, add the button id.
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(entry.Value.Length)); // Amound of letters for this element
				MobileControlPackHelper.PackString(ref _bytes, ref index, entry.Value); // And afterwards the string (button name).
			}

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class ShowVideoViewMessage : Message
	{
		// Needed to show a Play or Pause symbol on the iPod. (If Video is playing, a pause symbol has to be shown, and vice versa.)
		// Play = 0; Pause = 1;
		// -> ChoseOptionMessage.Video
		public bool videoIsPaused = true;

		public ShowVideoViewMessage()
			: base(null, Command.SHOW_VIDEO_VIEW)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + sizeof(byte); // BaseSize + current play state

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(videoIsPaused));

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class ShowSliderViewMessage : Message
	{
		public class SliderDetails
		{
			public SliderDetails(string name, float minValue, float maxValue, float curValue)
			{
				this.name = name;
				this.minValue = minValue;
				this.maxValue = maxValue;
				this.curValue = curValue;
			}

			public string name;
			public float minValue;
			public float maxValue;
			public float curValue;
		}

		public List<SliderDetails> sliders = new List<SliderDetails>();

		public ShowSliderViewMessage()
			: base(null, Command.SHOW_SLIDER_VIEW)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + 1 + sliders.Count * 3 * sizeof(float) + sliders.Count; // BaseSize + Amount of Sliders + <AmountOfSlider> * 3 * float + 1 Byte for each string (string amount before str)
			foreach (SliderDetails sd in sliders)
			{
				msgLen += sd.name.Length; // ... + 1 byte for each character in each string.
			}

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(sliders.Count)); // Amount of sliders

			foreach (SliderDetails sd in sliders) // For each slider:
			{
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(sd.name.Length)); // Amound of letters for this name
				MobileControlPackHelper.PackString(ref _bytes, ref index, sd.name); // name (letter for letter, not null terminated)
				MobileControlPackHelper.PackFloat(ref _bytes, ref index, sd.minValue); // Minimum Slider Value
				MobileControlPackHelper.PackFloat(ref _bytes, ref index, sd.maxValue); // Maximum Slider Value
				MobileControlPackHelper.PackFloat(ref _bytes, ref index, sd.curValue); // Current Slider Value
			}

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class ShowTouchViewMessage : Message
	{
		public ShowTouchViewMessage()
			: base(null, Command.SHOW_TOUCH_VIEW)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize(); // BaseSize

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class ShowListViewMessage : Message
	{
		List<string> menuList;

		public ShowListViewMessage(List<string> menuList)
			: base(null, Command.SHOW_LIST_VIEW)
		{
			this.menuList = menuList;
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + sizeof(int) + menuList.Count; // BaseSize + list entries (int) + 1 byte for each string length ...
			foreach (string str in menuList)
			{
				msgLen += str.Length; // ... + 1 byte for each character in each string.
			}

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackInt(ref _bytes, ref index, menuList.Count); // Amount of list entries

			// For each list entry:
			for (int ii = 0; ii < menuList.Count; ++ii)
			{
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(menuList[ii].Length)); // Amound of letters for this element
				MobileControlPackHelper.PackString(ref _bytes, ref index, menuList[ii]); // list entry (letter for letter, not null terminated)
			}

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class ShowQuizViewMessage : Message
	{
		public enum QuizButton
		{
			RESOLVE = 0,
			FINISH = 1,
		}

		public QuizButton quizButton;

		public ShowQuizViewMessage(QuizButton quizButton)
			: base(null, Command.SHOW_QUIZ_VIEW)
		{
			this.quizButton = quizButton;
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + 1; // BaseSize + 1 byte (state)

			_bytes = new byte[msgLen];

			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(quizButton)); // quizButton State

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	public class SceneMenuMessage : Message
	{
		public class Category
		{
			public class Section
			{
				public int sectionID;
				public string sectionName;
			}

			public int categoryID;
			public string categoryName;
			public List<Section> sectionList = new List<Section>();
		}

		public List<Category> categorySectionList = new List<Category>();

		public SceneMenuMessage()
			: base(null, Command.SCENE_MENU)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize();

			// Figure out size of the data:
			msgLen += 1; // Amount of Categories

			for (int categoryIndex = 0; categoryIndex < categorySectionList.Count; ++categoryIndex)
			{
				Category category = categorySectionList[categoryIndex];

				msgLen += 1 + 1 + category.categoryName.Length + 1; // Category ID +  Amount of Chars (Category Name) + Category Name + Amount of Scenes

				for (int sceneIndex = 0; sceneIndex < category.sectionList.Count; ++sceneIndex)
				{
					Category.Section section = category.sectionList[sceneIndex];

					msgLen += 1 + 1 + section.sectionName.Length; // Scene ID + Amount of Chars (Category Name) + Scene Name
				}
			}

			// Now start packing:
			_bytes = new byte[msgLen];
			int index = 0;

			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(categorySectionList.Count)); // Amount of Categories

			for (int categoryIndex = 0; categoryIndex < categorySectionList.Count; ++categoryIndex)
			{
				Category category = categorySectionList[categoryIndex];

				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(category.categoryID)); // Category ID
				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(category.categoryName.Length)); // Category Name String Length
				MobileControlPackHelper.PackString(ref _bytes, ref index, category.categoryName); // Category Name


				MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(category.sectionList.Count)); // Amount of Scenes

				for (int sceneIndex = 0; sceneIndex < category.sectionList.Count; ++sceneIndex)
				{
					Category.Section section = category.sectionList[sceneIndex];

					MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(section.sectionID)); // Scene ID
					MobileControlPackHelper.PackByte(ref _bytes, ref index, Convert.ToByte(section.sectionName.Length)); // Section Name String Length
					MobileControlPackHelper.PackString(ref _bytes, ref index, section.sectionName); // Section Name
				}
			}

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			throw new NotImplementedException("This Message Type is only beeing sended, not received!");
		}
	}

	// ------------- Bidirectional Messages -------------

	public class HeartBeatMessage : Message
	{
		public HeartBeatMessage(byte[] bytes)
			: base(bytes, Command.HEART_BEAT)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize(); // BaseSize

			_bytes = new byte[msgLen];

			int index = 0;
			PackDuties(ref index); // Msg Type

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;
			UnpackDuties(ref index); // Message Type

			return index;
		}
	}

	public class ChoseSceneMessage : Message
	{
		private byte _categoryID; // Microstory ID
		private byte _sectionID;

		public uint CategoryID
		{
			get
			{
				return Convert.ToUInt32(_categoryID);
			}
			set
			{
				_categoryID = Convert.ToByte(value);
			}
		}

		public uint SectionID
		{
			get
			{
				return Convert.ToUInt32(_sectionID);
			}
			set
			{
				_sectionID = Convert.ToByte(value);
			}
		}

		public ChoseSceneMessage(byte[] bytes)
			: base(bytes, Command.CHOSE_SCENE)
		{
		}

		public override byte[] GetMessageBytes()
		{
			int msgLen = GetBaseSize() + 1 + 1; // BaseSize + CategoryID + SectionID

			_bytes = new byte[msgLen];

			int index = 0;
			PackDuties(ref index); // Msg Type

			MobileControlPackHelper.PackByte(ref _bytes, ref index, _categoryID);
			MobileControlPackHelper.PackByte(ref _bytes, ref index, _sectionID);

			return _bytes;
		}

		public override int UnpackReceivedMessage()
		{
			int index = 0;

			UnpackDuties(ref index); // Message Type

			_categoryID = MobileControlPackHelper.UnpackByte(_bytes, ref index); // ID of the category (microstory)
			_sectionID = MobileControlPackHelper.UnpackByte(_bytes, ref index); // ID of the scene (Microstory-Part)

			return index;
		}
	}

	// ------------- Template Message -------------

	// Template: Rename and use:
	public class DemoMessage : Message
	{
		// Put your public attributes here!

		public DemoMessage(byte[] bytes)
			: base(bytes, Command.UNKNOWN) // TODO: Replace with your Command (Create one above!)
		{
		}

		public override byte[] GetMessageBytes()
		{
			// Implement for sending:
			throw new NotImplementedException("This is only a template for implementing real messages!");
		}

		public override int UnpackReceivedMessage()
		{
			// Implement for receiving:
			throw new NotImplementedException("This is only a template for implementing real messages!");
		}
	}
}