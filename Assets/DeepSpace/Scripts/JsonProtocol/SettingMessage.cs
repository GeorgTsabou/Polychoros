using UnityEngine;
using System.Collections;

namespace DeepSpace.JsonProtocol
{
	[System.Serializable]
	public class SettingMessage : EventType
	{
		public enum ValueType
		{
			FLOAT,
		}

		public enum SettingType
		{
			EYE_DISTANCE,
			CONVERGENCE_DISTANCE,
		}

		[SerializeField]
		public ValueType valueType;
		[SerializeField]
		public SettingType settingType;

		public SettingMessage(ValueType valueType, SettingType settingType)
		{
			this.valueType = valueType;
			this.settingType = settingType;

			EventIdentifier = EventIdentifierBase.SETTING_MESSAGE;
		}
	}
}