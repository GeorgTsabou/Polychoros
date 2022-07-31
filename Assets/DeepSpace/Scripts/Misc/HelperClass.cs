using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace
{
	public static class Helper
	{
		// Helper method to only log a specified key once (e.g. if something would be logged 60 times per second but shall only be printed once).
		private static HashSet<string> _loggedKeys = new HashSet<string>();
		public static void LogOnce(string logKey, string message, Object context = null)
		{
			if (_loggedKeys.Contains(logKey) == false)
			{
				_loggedKeys.Add(logKey);
				Debug.Log(message, context);
			}
		}
	}
}