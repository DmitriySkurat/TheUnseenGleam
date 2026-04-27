using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI progress bar that is projected above the player while they are grabbed.
/// This MonoBehaviour must stay active at all times so Update() keeps running.
/// Assign barRoot to the child GameObject that holds the Canvas/visuals;
/// that child is what gets shown/hidden.
/// </summary>
public class GrabProgressBar : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Tooltip("Child GameObject that holds the Canvas and visuals. Gets shown/hidden.")]
    public GameObject barRoot;

    [Tooltip("Fill image of the progress bar")]
    public Image fillImage;

    [Tooltip("Extra world-space padding above the player's top bound.")]
    public float verticalPadding = 0.25f;

    [Tooltip("Fallback height used when the player's bounds are unavailable.")]
    public float fallbackHeight = 1.5f;

    private PlayerContext _ctx;
    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;
    private Canvas _canvas;
    private Camera _worldCamera;
    private bool _wasGrabbed;

    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;

        _ctx = Services.Get<PlayerContext>();
        _rectTransform = transform as RectTransform;
        _parentRectTransform = _rectTransform != null ? _rectTransform.parent as RectTransform : null;
        _canvas = GetComponentInParent<Canvas>();
        ResolveWorldCamera();

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        SetVisible(false);
    }

    public void Dispose() { }

    private void Update()
    {
        if (_ctx == null)
            return;

        bool grabbed = _ctx.isGrabbed && _ctx.isAlive;

        if (grabbed != _wasGrabbed)
        {
            SetVisible(grabbed);
            _wasGrabbed = grabbed;
        }

        if (grabbed && fillImage != null)
            fillImage.fillAmount = _ctx.grabProgress;
    }

    private void LateUpdate()
    {
        if (_ctx == null || !_ctx.isGrabbed)
            return;

        UpdateScreenPosition();
    }

    private void SetVisible(bool visible)
    {
        if (barRoot != null)
            barRoot.SetActive(visible);
    }

    // Position in LateUpdate so the UI uses the camera's final position for this frame.
    private void UpdateScreenPosition()
    {
        if (_ctx.transform == null || _rectTransform == null || _parentRectTransform == null)
            return;

        Camera worldCamera = ResolveWorldCamera();
        if (worldCamera == null)
            return;

        Vector3 screenPoint = worldCamera.WorldToScreenPoint(GetAnchorWorldPosition());
        if (screenPoint.z < 0f)
            return;

        Camera uiCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? _canvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRectTransform,
                screenPoint,
                uiCamera,
                out Vector2 localPoint))
        {
            _rectTransform.anchoredPosition = localPoint;
        }
    }

    private Vector3 GetAnchorWorldPosition()
    {
        if (_ctx.coll != null)
        {
            Bounds bounds = _ctx.coll.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + verticalPadding, _ctx.transform.position.z);
        }

        if (_ctx.renderer != null)
        {
            Bounds bounds = _ctx.renderer.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + verticalPadding, _ctx.transform.position.z);
        }

        return _ctx.transform.position + Vector3.up * fallbackHeight;
    }

    private Camera ResolveWorldCamera()
    {
        if (_worldCamera != null)
            return _worldCamera;

        if (Services.IsRegistered<PlayerContext>())
        {
            var cameraFollow = Services.Get<PlayerContext>().cameraFollow;
            if (cameraFollow != null)
                _worldCamera = cameraFollow.GetComponent<Camera>();
        }

        if (_worldCamera == null && _canvas != null && _canvas.worldCamera != null)
            _worldCamera = _canvas.worldCamera;

        if (_worldCamera == null)
            _worldCamera = Camera.main;

        return _worldCamera;
    }
}
