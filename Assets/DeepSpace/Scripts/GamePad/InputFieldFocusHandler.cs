using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSpace
{
	public class InputFieldFocusHandler : ControllerFocusHandler
	{
		[SerializeField]
		protected InputField _inputField;

		[SerializeField]
		protected bool _hasMinValue;
		[SerializeField]
		protected float _minValue;
		[SerializeField]
		protected bool _hasMaxValue;
		[SerializeField]
		protected float _maxValue;

		public override void HandleAxisX(float value)
		{
			// Get the current value from the text field:
			float curVal = float.Parse(_inputField.text);

			// Add / Sub the value from the controller input:
			curVal += value * valueMultiplicator;

			// Check for min and max borders:
			if (_hasMinValue && curVal < _minValue)
			{
				curVal = _minValue;
			}
			if (_hasMaxValue && curVal > _maxValue)
			{
				curVal = _maxValue;
			}

			// Write the value back to the text field. 
			string curValStr = curVal.ToString();
			_inputField.text = curValStr;

			// This will trigger the OnEndEdit callback of this input field, if a method is connected, so that the new input is applied:
			InputField.SubmitEvent submitEvent = _inputField.onEndEdit;
			if (submitEvent.GetPersistentEventCount() > 0)
			{
				submitEvent.Invoke(curValStr);
			}
			// This will trigger the OnValueChanged callback of this input field, if a method is connected, so that the new input is applied:
			InputField.OnChangeEvent changeEvent = _inputField.onValueChanged;
			if (changeEvent.GetPersistentEventCount() > 0)
			{
				changeEvent.Invoke(curValStr);
			}
		}
	}
}