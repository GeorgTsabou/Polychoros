using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace
{
	public abstract class ControllerFocusHandler : MonoBehaviour
	{
		public float valueMultiplicator = 1.0f;

		public abstract void HandleAxisX(float value);

		// Add additional methods here, if needed.
	}
}