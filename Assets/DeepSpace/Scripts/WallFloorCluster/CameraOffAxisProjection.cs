using System;
using UnityEngine;
using UnityEngine.Serialization;

// Known Bug: Actually everything should be implemented to not only support separate, but also both-eye cameras.
// However, the stereo effect is wrong (no correct focus plane) when using a single both-eye camera.
// It works with separated camers though.

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraOffAxisProjection : MonoBehaviour
{
	[Serializable]
	public struct Frustum
	{
		[ReadOnly]
		public float left;
		[ReadOnly]
		public float right;
		[ReadOnly]
		public float bottom;
		[ReadOnly]
		public float top;
		[ReadOnly]
		public float clipNear;
		[ReadOnly]
		public float clipFar;

		[ReadOnly]
		public Vector3 side;
		[ReadOnly]
		public Vector3 up;
		[ReadOnly]
		public Vector3 fwd;
	};

	[SerializeField]
	[FormerlySerializedAs("_screen")]
	private Transform _focusPlane = null;

	[SerializeField]
	private float _stereoSeparation = 0.065f;

#if UNITY_EDITOR
	[SerializeField]
	private bool _drawDebug = true;
	[SerializeField]
	private Color _color = Color.green;

	[SerializeField]
	private CameraOffAxisProjection _clippingPlaneSyncCamera = null;

#pragma warning disable 414 // Ignore warning "assigned but not used".
	[SerializeField, ReadOnly]
	private float _fovHor = 0.0f;
	[SerializeField, ReadOnly]
	private float _fovVer = 0.0f;
#pragma warning restore 414 // Restore warnings.
#endif // UNITY_EDITOR

	private Camera _cam = null;

	[SerializeField, ReadOnly]
	private Frustum _frustum;

	public float StereoSeparation
	{
		get { return _stereoSeparation; }
		set
		{
			_stereoSeparation = value;
			UpdateStereoSeparation();
		}
	}

	public Frustum CamFrustum
	{
		get { return _frustum; }
	}

	private float _lastKnownEyeSeparation = 0f;

	private void Awake()
	{
		_cam = GetComponent<Camera>();

		// Separation is done via slight position offsets, the _cam.stereoSeparation is "disabled" by setting it 0, because it interferes with our own calculations.
		_cam.stereoSeparation = 0f;

#if UNITY_EDITOR // If in editor, check if it is playing. If not playing, do not execute this code in editor (ExecuteInEditMode would run in too, if I don't check it.) But in a build, always do it.
		if (UnityEditor.EditorApplication.isPlaying == true)
#endif
		{
			DeepSpace.CmdConfigManager configMgr = DeepSpace.CmdConfigManager.Instance;

			if (configMgr != null)
			{
				_stereoSeparation *= (configMgr.invertStereo ? -1f : 1f);
			}

			UpdateStereoSeparation();
		}
	}

	private void LateUpdate()
	{
		bool projectionMatrixUpdated = false;

		// If VR is enabled and if the camera is targeting to both eyes, we need to do some extra work.
		// This class is designed to handle one camera, for the left or right eye, but to support the Unity design, that a camera can do both, we are updating the stereo projection matrices seperately.
		// This means: We shift the camera correctly (first to the right, then to the left), calc the resulting frustums, and set the Projection Matrix for each eye.
		string loadedDeviceName = GetLoadedDeviceName();
		if (loadedDeviceName.Equals("stereo")) // -vrmode stereo
		{
			if (_cam.stereoTargetEye == StereoTargetEyeMask.Both)
			{
				UpdateStereoSeparation(Camera.StereoscopicEye.Right);
				UpdateProjectionMatrix(Camera.StereoscopicEye.Right);

				UpdateStereoSeparation(Camera.StereoscopicEye.Left);
				UpdateProjectionMatrix(Camera.StereoscopicEye.Left);

				projectionMatrixUpdated = true;
			}
		}

		// If the upper part has not been used (for seperated cameras or mono applications), we just need to update the projection matrix.
		// The camera has already been adjusted beforehand.
		if(projectionMatrixUpdated == false)
		{
			UpdateProjectionMatrix();
		}

#if UNITY_EDITOR
		if (_clippingPlaneSyncCamera != null) // Optional Clippling plane sync:
		{
			_cam.nearClipPlane = -_clippingPlaneSyncCamera.CamFrustum.bottom;
			_cam.farClipPlane = -_clippingPlaneSyncCamera.CamFrustum.bottom * _clippingPlaneSyncCamera.CamFrustum.clipFar / _clippingPlaneSyncCamera.CamFrustum.clipNear;
		}
#endif

#if UNITY_EDITOR
		if (_drawDebug == true)
		{
			DrawFrustum(_frustum, _color);
		}
#endif
	}

	private void UpdateProjectionMatrix(Camera.StereoscopicEye? specifyBothEyeTarget = null)
	{
		bool success = CalcFrustum(ref _frustum, _cam, _focusPlane);
		if (success)
		{
			// Create projection matrix out of frustum:
			Matrix4x4 projectionMatrix = CreateProjectionMatrix(_frustum);

			// Set projection matrix:
			SetProjectionMatrix(_cam, projectionMatrix, specifyBothEyeTarget);
		}
		else
		{
			DeepSpace.Helper.LogOnce(_cam.name + "_UpdateProjectionMatrix", "UpdateProjectionMatrix was not successful, the projection matrix was not set. Did you forget to setup or link the focus plane?");
		}
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		UpdateStereoSeparation();
	}
#endif

	// The specifyBothEyeTarget parameter is only needed for the camera.targetEye both setup, to distinct if the camera shall be prepared for the left or right eye.
	private void UpdateStereoSeparation(Camera.StereoscopicEye? specifyBothEyeTarget = null)
	{
		if (_cam != null)
		{
			if (_cam.stereoTargetEye == StereoTargetEyeMask.Left || _cam.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				// For seperate cameras, shifting only needs to be done once. Changes only need to be done if the eye separation changes.
				if (_stereoSeparation != _lastKnownEyeSeparation)
				{
					_lastKnownEyeSeparation = _stereoSeparation;

					ShiftCamera(_cam.stereoTargetEye);
				}
			}
			else if(_cam.stereoTargetEye == StereoTargetEyeMask.Both)
			{
				// For both eye cameras, this will be called twice every frame, to setup the camera for the left and right eye each:
				if(specifyBothEyeTarget != null)
				{
					if(specifyBothEyeTarget.Value == Camera.StereoscopicEye.Left)
					{
						ShiftCamera(StereoTargetEyeMask.Left);
					}
					else if (specifyBothEyeTarget.Value == Camera.StereoscopicEye.Right)
					{
						ShiftCamera(StereoTargetEyeMask.Right);
					}
				}
			}
		}
	}

	// Shifts the camera to the according side (depending on eye and if eyes are inverted) by the half eye distance.
	// If shiftEyeSide is set to Both or None, the camera is placed in the center.
	private void ShiftCamera(StereoTargetEyeMask shiftEyeSide)
	{
		float shiftMul = 0f;
		if (shiftEyeSide == StereoTargetEyeMask.Right)
		{
			shiftMul = 1.0f;
		}
		else if (shiftEyeSide == StereoTargetEyeMask.Left)
		{
			shiftMul = -1.0f;
		}

		_cam.transform.localPosition = Vector3.right * _stereoSeparation * 0.5f * shiftMul;
	}

	private bool CalcFrustum(ref Frustum frustum, Camera cam, Transform focusPlane)
	{
		frustum.side = cam.transform.localToWorldMatrix.GetColumn(0).normalized;
		frustum.up = cam.transform.localToWorldMatrix.GetColumn(1).normalized;
		frustum.fwd = cam.transform.localToWorldMatrix.GetColumn(2).normalized;

		if (focusPlane != null)
		{
			Vector3 viewerDist = transform.position - focusPlane.position;
			Vector2 floorSize = new Vector2(focusPlane.lossyScale.x, focusPlane.lossyScale.y);

			float distSide = Vector3.Dot(viewerDist, frustum.side);
			float distUp = Vector3.Dot(viewerDist, frustum.up);
			float distFwd = Vector3.Dot(viewerDist, frustum.fwd);

			frustum.top = -(floorSize.y * 0.5f - distUp) / distFwd * cam.nearClipPlane;
			frustum.bottom = (floorSize.y * 0.5f + distUp) / distFwd * cam.nearClipPlane;
			frustum.left = (floorSize.x * 0.5f + distSide) / distFwd * cam.nearClipPlane;
			frustum.right = -(floorSize.x * 0.5f - distSide) / distFwd * cam.nearClipPlane;
			frustum.clipNear = cam.nearClipPlane;
			frustum.clipFar = cam.farClipPlane;

#if UNITY_EDITOR
			_fovHor = Mathf.Rad2Deg * (Mathf.Atan(-frustum.left / frustum.clipNear) + Mathf.Atan(frustum.right / frustum.clipNear));
			_fovVer = Mathf.Rad2Deg * (Mathf.Atan(frustum.top / frustum.clipNear) + Mathf.Atan(-frustum.bottom / frustum.clipNear));
#endif

			return true;
		}

		return false;
	}

	// @param specifyBothEyeTarget is only needed to specify the target eye of the projectionMatrix if stereo is enabled and camera shall rander to both eyes.
	private void SetProjectionMatrix(Camera cam, Matrix4x4 projectionMatrix, Camera.StereoscopicEye? specifyBothEyeTarget = null)
	{
#if UNITY_EDITOR
		cam.projectionMatrix = projectionMatrix;
#else // #elif UNITY_STANDALONE

		string loadedDeviceName = GetLoadedDeviceName();
		if (loadedDeviceName.Equals("stereo")) // -vrmode stereo
		{
			if (cam.stereoTargetEye == StereoTargetEyeMask.Left)
			{
				cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, projectionMatrix);
			}
			else if (cam.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, projectionMatrix);
			}
			else // None or both:
			{
				if (specifyBothEyeTarget != null)
				{
					// Setting left or right eye for "both-eye" camera, depending on the specifyBothEyeTarget.Value
					cam.SetStereoProjectionMatrix(specifyBothEyeTarget.Value, projectionMatrix);
				}
				else
				{
					DeepSpace.Helper.LogOnce(cam.name + "_BothTargetEye", "CameraOffAxisProjection did not update the projection matrix for camera \"" + cam.name + "\", " +
						"because it is configured to render to both eyes but SetProjectionMatrix did not get a specification for the target eye. Please use the specifyBothEyeTarget parameter to prevent this issue.");
				}
			}
		}
		else if (string.IsNullOrEmpty(loadedDeviceName)) // -vrmode none
		{
			if (cam.stereoTargetEye == StereoTargetEyeMask.Left)
			{
				cam.stereoTargetEye = StereoTargetEyeMask.Both;
				cam.ResetStereoProjectionMatrices();
				Debug.LogWarning("CameraOffAxisProjection changed the left eye camera to both eyes, because the application was started in -vrmode none.");
			}
			else if (cam.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				cam.stereoTargetEye = StereoTargetEyeMask.None;
				cam.ResetStereoProjectionMatrices();
				cam.gameObject.SetActive(false);
				Debug.LogWarning("CameraOffAxisProjection disabled the right eye camera, because the application was started in -vrmode none.");
			}

			cam.projectionMatrix = projectionMatrix;
		}
		else
		{
			DeepSpace.Helper.LogOnce(cam.name + "_loadedXrDevice", "CameraOffAxisProjection cannot handle the projection matrix in vrmode \"" + loadedDeviceName + "\"");
		}

		//DeepSpace.Helper.LogOnce(cam.name + "_stereo", "Stereo enabled: " + cam.stereoEnabled);
#endif // End of UNITY_STANDALONE only
	}

	private static string GetLoadedDeviceName()
	{
#if UNITY_5 || UNITY_2017_1
		return UnityEngine.VR.VRSettings.loadedDeviceName;
#else // #elif UNITY_2017_2_OR_NEWER
		return UnityEngine.XR.XRSettings.loadedDeviceName;
#endif // End of version difference.
	}

	private static Matrix4x4 CreateProjectionMatrix(Frustum frustum)
	{
		Matrix4x4 m = new Matrix4x4();

		m.SetRow(0, new Vector4(2.0f * frustum.clipNear / (frustum.right - frustum.left), 0f, (frustum.right + frustum.left) / (frustum.right - frustum.left), 0f));
		m.SetRow(1, new Vector4(0f, 2.0f * frustum.clipNear / (frustum.top - frustum.bottom), (frustum.top + frustum.bottom) / (frustum.top - frustum.bottom), 0f));
		m.SetRow(2, new Vector4(0f, 0f, (frustum.clipFar + frustum.clipNear) / (frustum.clipNear - frustum.clipFar), 2.0f * (frustum.clipFar * frustum.clipNear) / (frustum.clipNear - frustum.clipFar)));
		m.SetRow(3, new Vector4(0f, 0f, -1.0f, 0f));

		return m;
	}

#if UNITY_EDITOR
	private void DrawFrustum(Frustum frustum, Color color)
	{
		float s = frustum.clipFar / frustum.clipNear;

		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.top,
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.top,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.bottom,
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.bottom,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.top,
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.bottom,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.top,
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.bottom,
			color
			);

		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.top,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.top * s,
			color);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.top,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.top * s,
			color);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.right + frustum.up * frustum.bottom,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.bottom * s,
			color);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipNear + frustum.side * frustum.left + frustum.up * frustum.bottom,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.bottom * s,
			color);

		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.top * s,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.top * s,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.bottom * s,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.bottom * s,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.top * s,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.left * s + frustum.up * frustum.bottom * s,
			color
			);
		Debug.DrawLine(
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.top * s,
			transform.position + frustum.fwd * frustum.clipFar + frustum.side * frustum.right * s + frustum.up * frustum.bottom * s,
			color
			);
	}
#endif
}