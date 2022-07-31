using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace.MobileControlMessages
{
	public class MobileControlPackHelper : BytePackHelper
	{
		#region byte packing

		public static char GetToken(Command command, bool start)
		{
			// Could do different Guard-Characters for different commands.
			return (start == true ? 'T' : 't');
		}

		public static int StartStream(ref byte[] bytes, Command command)
		{
			int index = 0; // First byte in the bytes-message (guard)
			PackChar(ref bytes, ref index, GetToken(command, true));
			return index;
		}

		public static void EndStream(ref byte[] bytes, Command command)
		{
			int index = bytes.Length - 1; // Last byte (guard)
			PackChar(ref bytes, ref index, GetToken(command, false));
		}

		#endregion

		// -------------------------------

		#region byte unpacking

		public static bool CheckStartToken(byte[] bytes, ref int start, Command command)
		{
			char receivedToken = UnpackChar(bytes, ref start);
			char expectedToken = GetToken(command, true);
			if (receivedToken != expectedToken)
			{
				Debug.LogWarning("Unexpected header byte, received unclean packet!\n"
					+ "Expected " + expectedToken + " but got " + receivedToken);
				return false;
			}

			return true;
		}

		public static bool CheckEndToken(byte[] bytes, ref int start, Command command)
		{
			char receivedToken = UnpackChar(bytes, ref start);
			char expectedToken = GetToken(command, false);
			if (receivedToken != expectedToken)
			{
				Debug.LogWarning("Unexpected tailing byte, received unclean packet!\n"
					+ "Expected " + expectedToken + " but got " + receivedToken);
				return false;
			}

			return true;
		}

#endregion
	}
}