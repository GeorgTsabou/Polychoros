using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoCanvasScaler : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private float _noStereoScale = 1.0f;
	[SerializeField]
	private float _minStereoScale = 0.2f; // Going out stereo
	[SerializeField]
	private float _maxStereoScale = 50f; // Going into depth stereo
	[SerializeField]
	private float _defaultStereoScale = 0.7f;

	public float NoStereoScale
	{
		get { return _noStereoScale; }
	}

	public float MinStereoScale
	{
		get { return _minStereoScale; }
	}

	public float MaxStereoScale
	{
		get { return _maxStereoScale; }
	}

	public float DefaultStereoScale
	{
		get { return _defaultStereoScale; }
		set { _defaultStereoScale = value; }
	}

	private void Awake()
	{
		SetCanvasStereoScale(_defaultStereoScale);
	}

	public void SetCanvasStereoScale(float scale)
	{
		if (scale < _minStereoScale)
		{
			scale = _minStereoScale;
		}
		else if (scale > _maxStereoScale)
		{
			scale = _maxStereoScale;
		}

		Debug.Log("SetCanvasStereoScale: " + scale);

		transform.localScale = new Vector3(scale, scale, scale);
	}

	public float GetOutcommingStereoFromPercent(float percent)
	{
		percent = Mathf.Clamp01(percent);

		return Mathf.Lerp(_noStereoScale, _minStereoScale, percent);
	}

	public float GetIngoingStereoFromPercent(float percent)
	{
		percent = Mathf.Clamp01(percent);

		return Mathf.Lerp(_noStereoScale, _maxStereoScale, percent);
	}

	public float GetCurrentScale()
	{
		return transform.localScale.x;
	}
}
