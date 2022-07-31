using UnityEngine;
using System.Collections;

namespace DeepSpace.JsonProtocol
{
	[System.Serializable]
	public class SettingMessageFloat : SettingMessage
	{
		[SerializeField]
		public float floatValue;

		public SettingMessageFloat(SettingType settingType, float floatValue)
			: base(ValueType.FLOAT, settingType)
		{
			this.floatValue = floatValue;
		}
	}
}