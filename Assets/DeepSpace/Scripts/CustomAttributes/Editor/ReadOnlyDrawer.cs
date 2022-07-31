﻿using UnityEngine;
using UnityEditor;


// This makes it possible to use the [ReadOnly] Attribute. (Together with ReadOnlyAttribute.cs)
// Source: http://answers.unity3d.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property,
											GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position,
							   SerializedProperty property,
							   GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}
