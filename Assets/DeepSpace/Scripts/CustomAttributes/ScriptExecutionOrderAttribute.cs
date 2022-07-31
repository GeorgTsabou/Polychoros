// Original Code from https://forum.unity.com/threads/controlling-scriptexecutionorder-through-class-attributes-code-provided.402868/
//
// ScriptExecutionOrderAttribute
// By NoiseCrime 05.06.2016
//
// Support Attribute class that works with the editor script 'ScriptExecutionOrderHandler' to automatically populate the ScriptExecutionOrder.
// Simply add [ScriptExecutionOrderAttribute( value )] prior to your class declaration, where value indicates the script order you want ( -9999 to +9999 )
// This script must NOT be placed in an Editor Folder.

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ScriptExecutionOrderAttribute : Attribute
{
	// A value between -9999 and +9999
	public readonly int executionIndex;

	public ScriptExecutionOrderAttribute(int index)
	{
		this.executionIndex = index;
	}
}
