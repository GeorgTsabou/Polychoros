// Original code from: https://forum.unity.com/threads/controlling-scriptexecutionorder-through-class-attributes-code-provided.402868/
//
// ScriptExecutionOrderHandler
// By NoiseCrime 05.06.2016
//
// Editor class that will automatically set the scriptExecutionOrder based on using a ScriptExecutionAttribute.
// The class is called automatically everytime Unity recompiles the project - see [InitializeOnLoad]
//
// This script MUST be placed into an Editor Folder in your Unity Project.
// Requires Script names to match class name.
// Likely only works with a single class defined per file.
// Unfortunately the script will also execute everytime you run the project in the editor ( play )
//
// ENABLE_LOGGING:      If defined will log ( sorted by order ) to the console all found classes using the ScriptExecutionAttribute and their script order.


#define ENABLE_LOGGING

using UnityEngine;
using UnityEditor;
using System;


[InitializeOnLoad]
public class ScriptExecutionOrderHandler : MonoBehaviour
{
	static ScriptExecutionOrderHandler()
	{
		if (EditorApplication.isCompiling || EditorApplication.isPaused || EditorApplication.isPlaying)
		{
			return;
		}

		EditorApplication.LockReloadAssemblies();

		// Loop through monoscripts
		foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
		{
			Type monoClass = monoScript.GetClass();

			if (monoClass != null)
			{
				Attribute att = Attribute.GetCustomAttribute(monoClass, typeof(ScriptExecutionOrderAttribute));

				if (att != null)
				{
					int executionIndex = ((ScriptExecutionOrderAttribute)att).executionIndex;

					// It is important not to set the execution order if the value is already correct!
					if (MonoImporter.GetExecutionOrder(monoScript) != executionIndex)
					{
						MonoImporter.SetExecutionOrder(monoScript, executionIndex);
#if ENABLE_LOGGING
						Debug.Log("The script " + monoScript.name + " was not set to correct Script Execution Order. It has been fixed and set to " + executionIndex + ".");
#endif
					}
				}
			}
		}

		EditorApplication.UnlockReloadAssemblies();
	}
}
