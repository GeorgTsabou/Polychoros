using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeepSpace
{
	// Source: http://wiki.unity3d.com/index.php?title=FramesPerSecond#CSharp_HUDFPS.cs
	// This version has small modifications compared to the source version.
	[RequireComponent(typeof(Text))]
	public class FpsCounter : MonoBehaviour
	{
		// Attach this to a UI Text to make a frames/second indicator.
		//
		// It calculates frames/second over each updateInterval,
		// so the display does not keep changing wildly.
		//
		// It is also fairly accurate at very low FPS counts (<10).
		// We do this not by simply counting frames per interval, but
		// by accumulating FPS for each frame. This way we end up with
		// correct overall FPS even if the interval renders something like
		// 5.5 frames.

		public float updateInterval = 0.5f;

		private float _accum = 0; // FPS accumulated over the interval
		private int _frames = 0; // Frames drawn over the interval
		private float _timeleft; // Left time for current interval
		private Text _uiText; // The text, where the FPS are presented.


		private void Awake()
		{
			if (_uiText == null)
			{
				_uiText = GetComponent<Text>();
			}
		}

		private void Start()
		{
			_timeleft = updateInterval;
		}

		private void Update()
		{
			_timeleft -= Time.deltaTime;
			_accum += Time.timeScale / Time.deltaTime;
			++_frames;

			// Interval ended - Update UI text and start new interval
			if (_timeleft <= 0.0f)
			{
				// Display two fractional digits (f2 format)
				float fps = _accum / _frames;
				string format = System.String.Format("<b>{0:F2} FPS</b>", fps);
				_uiText.text = format;

				if (fps < 30)
				{
					_uiText.color = Color.yellow;
				}
				else if (fps < 10)
				{
					_uiText.color = Color.red;
				}
				else
				{
					_uiText.color = Color.green;
				}

				_timeleft = updateInterval;
				_accum = 0.0f;
				_frames = 0;
			}
		}
	}
}