using UnityEngine;
using System.Collections;

namespace DeepSpace
{
	// Parses config values from the command line arguments.
	// A call to his Unity Application might look like this:
	// app.exe -mode=WALL -udpAddress=192.168.0.1 -udpPort=1111
	[ScriptExecutionOrder(-100)] 
	public class CmdConfigManager : Singleton<CmdConfigManager>
	{
		private bool _configParsed = false;

		public enum AppType
		{
			WALL = 0,
			FLOOR = 1
		}

		public AppType applicationType = AppType.WALL;
		public bool invertStereo = false;

		private void Awake()
		{
			ParseConfiguration();
		}

		protected override void InstanceGotAssigned()
		{
			ParseConfiguration();
		}

		private void ParseConfiguration()
		{
			// Only parse the config once.
			if (_configParsed)
			{
				return;
			}
			_configParsed = true;

			// Just some preliminary command line parsing...
			// Use this as an example for your own command line arguments.
			string[] args = System.Environment.GetCommandLineArgs();
			char[] splitChar = { '=' };
			foreach (string argument in args)
			{
				if (argument.Contains("="))
				{
					string[] splitArgument = argument.Split(splitChar);

					string key = splitArgument[0];
					string value = splitArgument[1];

					// If there has been used an equal sign in the argument -> Add this to the value:
					if (splitArgument.Length > 2)
					{
						for (int ii = 2; ii < splitArgument.Length; ++ii)
						{
							value += splitArgument[ii];
						}
					}

					ParseArgument(key, value);
				}
			}

			FinishedParsingArguments();
		}

		// This will be called for each argument passed on from the commandline:
		protected virtual void ParseArgument(string key, string value)
		{
			// Make key and value lower case, so that it is not case sensitive.
			key = key.ToLower();
			value = value.ToLower();

			if (key.Equals("-mode"))
			{
				if (value == "wall")
				{
					applicationType = AppType.WALL;
				}
				else if (value == "floor")
				{
					applicationType = AppType.FLOOR;
				}
			}
			else if(key.Equals("-invertstereo"))
			{
				invertStereo = ValueToBool(value);
			}
		}

		protected virtual void FinishedParsingArguments()
		{
			// Derive from this class and override this method if you need a callback after all parameters have been parsed.
		}

		public bool ValueToInt(string val, out int intVal, bool logException = true)
		{
			bool result = false;
			intVal = 0;
			try
			{
				intVal = System.Convert.ToInt32(val);
				result = true;
			}
			catch (System.Exception e)
			{
				if (logException)
				{
					Debug.LogException(e);
				}
			}

			return result;
		}

		public bool ValueToBool(string value)
		{
			bool result = false;
			try
			{
				result = System.Convert.ToBoolean(value);
			}
			catch (System.FormatException formatException)
			{
				// Could not convert value to a bool
				int checkInt = 0;
				if(ValueToInt(value, out checkInt, false))
				{
					result = checkInt != 0;
				}
				else
				{
					Debug.LogException(formatException);
				}
			}

			return result;
		}

		public float ValueToFloat(string value)
		{
			float result = 0f;
			try
			{
				result = System.Convert.ToSingle(value, new System.Globalization.CultureInfo("en-US")); // valid float: "12.34"  
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			return result;
		}
	}
}