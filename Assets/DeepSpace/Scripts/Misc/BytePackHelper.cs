using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace
{
	public class BytePackHelper
	{
		#region byte packing

		public static bool PackByte(ref byte[] bytes, ref int start, byte byteData)
		{
			// Check if enough space is available in the bytes array:
			if (start + 1 > bytes.Length)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				// If not, don't pack it...
				return false;
			}

			// Write data to bytes:
			bytes[start++] = byteData;

			return true;
		}

		public static bool PackChar(ref byte[] bytes, ref int start, char charData)
		{
			byte data = Convert.ToByte(charData);

			// Check if enough space is available in the bytes array:
			if (start + 1 > bytes.Length)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				// If not, don't pack it...
				return false;
			}

			// Write data to bytes:
			bytes[start++] = data;

			return true;
		}

		public static bool PackNullTerminatedString(ref byte[] bytes, ref int start, string stringData)
		{
			bool success;
			System.Text.Encoding iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");
			byte[] isoBytes = iso_8859_1.GetBytes(stringData);
			//char[] charArray = stringData.ToCharArray();

			for (int ii = 0; ii < stringData.Length; ++ii)
			{
				//success = PackChar(ref bytes, ref start, charArray[ii]);
				success = PackByte(ref bytes, ref start, isoBytes[ii]);

				if (success == false)
				{
					Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

					return false;
				}
			}

			// Add terminating zero:
			success = PackChar(ref bytes, ref start, '\0');

			return success;
		}

		public static bool PackString(ref byte[] bytes, ref int start, string stringData)
		{
			bool success = false;

			// ISO-8859-1 should be the default encoding, but just to be sure, it is encoded explicitly here! (1 Byte per char including German umlauts.)
			System.Text.Encoding iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");
			byte[] isoBytes = iso_8859_1.GetBytes(stringData);
			//char[] charArray = stringData.ToCharArray();

			for (int ii = 0; ii < isoBytes.Length; ++ii)
			{
				//success = PackChar(ref bytes, ref start, charArray[ii]);
				success = PackByte(ref bytes, ref start, isoBytes[ii]);

				if (success == false)
				{
					Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

					break;
				}
			}

			return success;
		}

		// Returns true if everything worked and data have been written to the correct position in bytes.
		// Returns false if operation was aborted. (bytes haven't been changed!)
		public static bool PackInt(ref byte[] bytes, ref int start, int intData)
		{
			byte[] data = BitConverter.GetBytes(intData);

			// Check if enough space is available in the bytes array:
			if (start + data.Length > bytes.Length)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				// If not, don't pack it...
				return false;
			}

			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(data);
			}

			// Write data to bytes:
			for (int i = 0; i < data.Length; ++i, ++start)
			{
				bytes[start] = data[i];
			}

			return true;
		}

		public static bool PackUint(ref byte[] bytes, ref int start, uint uintData)
		{
			byte[] data = BitConverter.GetBytes(uintData);

			// Check if enough space is available in the bytes array:
			if (start + data.Length > bytes.Length)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				// If not, don't pack it...
				return false;
			}

			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(data);
			}

			// Write data to bytes:
			for (int i = 0; i < data.Length; ++i, ++start)
			{
				bytes[start] = data[i];
			}

			return true;
		}

		public static bool PackFloat(ref byte[] bytes, ref int start, float floatData)
		{
			byte[] data = BitConverter.GetBytes(floatData);

			// Check if enough space is available in the bytes array:
			if (start + data.Length > bytes.Length)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				// If not, don't pack it...
				return false;
			}

			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(data);
			}

			// Write data to bytes:
			for (int i = 0; i < data.Length; ++i, ++start)
			{
				bytes[start] = data[i];
			}

			return true;
		}

		public static bool PackVector3(ref byte[] bytes, ref int start, Vector3 vecData)
		{
			bool result = false;

			result = PackFloat(ref bytes, ref start, vecData.x);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			result = PackFloat(ref bytes, ref start, vecData.y);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			result = PackFloat(ref bytes, ref start, vecData.z);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			return result;
		}

		public static bool PackQuaternion(ref byte[] bytes, ref int start, Quaternion quatData)
		{
			bool result = false;

			result = PackFloat(ref bytes, ref start, quatData.x);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			result = PackFloat(ref bytes, ref start, quatData.y);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			result = PackFloat(ref bytes, ref start, quatData.z);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			result = PackFloat(ref bytes, ref start, quatData.w);
			if (result == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");

				return false;
			}

			return result;
		}

		public static bool PackColor(ref byte[] bytes, ref int start, Color color)
		{
			byte r, g, b;

			r = System.Convert.ToByte(color.r * 255f);
			g = System.Convert.ToByte(color.g * 255f);
			b = System.Convert.ToByte(color.b * 255f);

			bool successR = PackByte(ref bytes, ref start, r);
			bool successG = PackByte(ref bytes, ref start, g);
			bool successB = PackByte(ref bytes, ref start, b);

			if ((successR && successG && successB) == false)
			{
				Debug.LogError("BytePackHelper Error: Not enough space available for packing the message.");
			}

			return (successR && successG && successB);
		}

		#endregion

		// -------------------------------

		#region byte unpacking

		public static char UnpackChar(byte[] bytes, ref int start)
		{
			return Convert.ToChar(bytes[start++]);
		}

		public static byte UnpackByte(byte[] bytes, ref int start)
		{
			return bytes[start++];
		}

		public static string UnpackNullTerminatedString(byte[] bytes, ref int start)
		{
			List<char> charList = new List<char>();
			char curChar = UnpackChar(bytes, ref start);
			while (curChar != '\0')
			{
				charList.Add(curChar);
				curChar = UnpackChar(bytes, ref start);
			}

			return new string(charList.ToArray());
		}

		public static string UnpackString(byte[] bytes, ref int start, int charAmount)
		{
			List<char> charList = new List<char>();
			char curChar;

			for (int ii = 0; ii < charAmount; ++ii)
			{
				curChar = UnpackChar(bytes, ref start);
				charList.Add(curChar);
			}

			return new string(charList.ToArray());
		}

		public static int UnpackInt(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for (int i = 0; i < 4; i++, start++)
				data[i] = bytes[start];
			if (!BitConverter.IsLittleEndian)
				data = SwapEndian(data);
			return BitConverter.ToInt32(data, 0);
		}

		public static uint UnpackUint(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for (int i = 0; i < 4; i++, start++)
				data[i] = bytes[start];
			if (!BitConverter.IsLittleEndian)
				data = SwapEndian(data);
			return BitConverter.ToUInt32(data, 0);
		}

		public static long UnpackLong(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for (int i = 0; i < 8; i++, start++)
				data[i] = bytes[start];
			if (!BitConverter.IsLittleEndian)
				data = SwapEndian(data);
			return BitConverter.ToInt64(data, 0);
		}

		public static float UnpackFloat(byte[] bytes, ref int start)
		{
			byte[] data = new byte[4];
			for (int i = 0; i < 4; i++, start++)
				data[i] = bytes[start];
			if (!BitConverter.IsLittleEndian)
				data = SwapEndian(data);
			return BitConverter.ToSingle(data, 0);
		}

		public static Vector3 UnpackVector3(byte[] bytes, ref int start)
		{
			Vector3 vec = new Vector3();

			vec.x = UnpackFloat(bytes, ref start);
			vec.y = UnpackFloat(bytes, ref start);
			vec.z = UnpackFloat(bytes, ref start);

			return vec;
		}

		public static Quaternion UnpackQuaternion(byte[] bytes, ref int start)
		{
			Vector4 vec = new Vector4();

			vec.x = UnpackFloat(bytes, ref start);
			vec.y = UnpackFloat(bytes, ref start);
			vec.z = UnpackFloat(bytes, ref start);
			vec.w = UnpackFloat(bytes, ref start);

			return new Quaternion(vec.x, vec.y, vec.z, vec.w);
		}

		public static double UnpackDouble(byte[] bytes, ref int start)
		{
			byte[] data = new byte[8];
			for (int i = 0; i < 8; i++, start++)
				data[i] = bytes[start];
			if (!BitConverter.IsLittleEndian)
				data = SwapEndian(data);
			return BitConverter.ToDouble(data, 0);
		}

		public static byte[] SwapEndian(byte[] data)
		{
			byte[] swapped = new byte[data.Length];
			for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
			{
				swapped[j] = data[i];
			}
			return swapped;
		}

		#endregion
	}
}