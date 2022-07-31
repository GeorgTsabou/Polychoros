using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeepSpace
{
	public class OffAxisFieldOfViewManager : MonoBehaviour
	{
		private Transform _planePivotContainer;
		private float _defaultScaleZ;
		private bool _resetInProgress = false;

		[SerializeField]
		private float _minScale = 0.3f;
		[SerializeField]
		private float _maxScale = 1.0f;

		private void Awake()
		{
			_planePivotContainer = transform;
			_defaultScaleZ = _planePivotContainer.localScale.z;
		}

		public void ChangeFOV(float changeValue)
		{
			// Don't do this, if reset is in progress.
			if (_resetInProgress == false)
			{
				float scaleZ = Mathf.Clamp(_planePivotContainer.localScale.z + changeValue, _minScale, _maxScale);
				_planePivotContainer.localScale = new Vector3(scaleZ, scaleZ, scaleZ);
			}
		}

		public void ResetFOV(float time = 1.0f)
		{
			// If reset is not in progress already and the plane does not have the default FOV already:
			if (_resetInProgress == false && _planePivotContainer.localScale.z.CompareTo(_defaultScaleZ) != 0f)
			{
				StartCoroutine(ResetFovOverTime(time));
			}
		}

		private IEnumerator ResetFovOverTime(float duration)
		{
			_resetInProgress = true;
			
			float startScaleZ = _planePivotContainer.localScale.z;
			float curTime = 0f;

			while (curTime < duration)
			{
				curTime += Time.smoothDeltaTime;
				float percent = Mathf.Clamp01(curTime / duration);

				float curScaleZ = Mathf.Lerp(startScaleZ, _defaultScaleZ, percent);
				_planePivotContainer.localScale = new Vector3(curScaleZ, curScaleZ, curScaleZ);

				yield return null;
			}

			_planePivotContainer.localScale = new Vector3(_defaultScaleZ, _defaultScaleZ, _defaultScaleZ);

			_resetInProgress = false;

			yield break;
		}
	}
}