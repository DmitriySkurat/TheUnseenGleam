using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space progress bar that appears above the player while they are grabbed.
/// This MonoBehaviour must stay active at all times so Update() keeps running.
/// Assign barRoot to the child GameObject that holds the Canvas/visuals —
/// that child is what gets shown/hidden.
/// </summary>
public class GrabProgressBar : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.UI;

    [Tooltip("Child GameObject that holds the Canvas and visuals. Gets shown/hidden.")]
    public GameObject barRoot;

    [Tooltip("Fill image of the progress bar")]
    public Image fillImage;

    private PlayerContext _ctx;
    private bool _wasGrabbed;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();
        SetVisible(false);
    }

    public void Dispose() { }

    private void Update()
    {
        if (_ctx == null) return;

        bool grabbed = _ctx.isGrabbed;

        if (grabbed != _wasGrabbed)
        {
            SetVisible(grabbed);
            _wasGrabbed = grabbed;
        }

        if (grabbed && fillImage != null)
            fillImage.fillAmount = _ctx.grabProgress;
    }

    private void SetVisible(bool visible)
    {
        if (barRoot != null)
            barRoot.SetActive(visible);
    }
}
