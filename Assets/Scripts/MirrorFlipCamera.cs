using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class MirrorFlipCamera : MonoBehaviour
{
    Camera _camera;
    public Camera Camera
    {
        get
        {
            if (_camera == null)
            {
                _camera = this.GetComponent<Camera>();
            }
            return _camera;
        }
    }

    [SerializeField]
    protected bool flipHorizontal;
    public bool FlipHorizontal
    {
        get => flipHorizontal;
        set
        {
            flipHorizontal = value;
            UpdateCameraMatrix();
        }
    }

    [SerializeField]
    protected bool flipVertical;
    public bool FlipVertical
    {
        get => flipVertical;
        set
        {
            flipVertical = value;
            UpdateCameraMatrix();
        }
    }

    protected float aspectRatio = 0;

    void Awake()
    {
        RenderPipelineManager.beginCameraRendering += beginCameraRendering;
        RenderPipelineManager.endCameraRendering += endCameraRendering;
    }

    void beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // Flip only if ONE of the two axis is mirrored but not if both are mirrored.
        if (this == null || !this.gameObject.activeInHierarchy)
            return;

        GL.invertCulling = flipHorizontal ^ flipVertical;

        // update is aspect ratio changed
        if (Mathf.Abs(aspectRatio - Camera.aspect) > 0.01f)
        {
            aspectRatio = Camera.aspect;
            UpdateCameraMatrix();
        }
    }

    void endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (this == null || !this.gameObject.activeInHierarchy)
            return;

        GL.invertCulling = false;
    }

    public void UpdateCameraMatrix()
    {
        Camera.ResetWorldToCameraMatrix();
        Camera.ResetProjectionMatrix();
        Vector3 scale = new Vector3(
            flipHorizontal ? -1 : 1,
            flipVertical ? -1 : 1,
            1);
        Camera.projectionMatrix = Camera.projectionMatrix * Matrix4x4.Scale(scale);

    }

    void OnValidate()
    {
        if (Camera == null)
            return;

        UpdateCameraMatrix();

#if UNITY_EDITOR
        RenderPipelineManager.beginCameraRendering -= beginCameraRendering;
        RenderPipelineManager.endCameraRendering -= endCameraRendering;
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            RenderPipelineManager.beginCameraRendering += beginCameraRendering;
            RenderPipelineManager.endCameraRendering += endCameraRendering;
        }
#endif
    }
}