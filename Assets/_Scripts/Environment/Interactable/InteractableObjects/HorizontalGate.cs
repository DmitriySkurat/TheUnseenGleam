using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HorizontalGate : Interactable
{
    [Header("Gate Settings")]
    [SerializeField] private bool isOpen = false;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private bool disableColliderWhenOpen = true;

    [Header("Platform")]
    [Tooltip("Коллайдер на слое Ground (дочерний объект). Игрок стоит на нём. Только игрок исключается через IgnoreCollision когда люк открыт.")]
    [SerializeField] private Collider2D platformCollider;

    public bool IsOpen => isOpen;

    private Collider2D _gateCollider;
    private ShadowCaster2D _shadowCaster;
    private PlayerInteractor _playerWatcher;
    private Collider2D _playerCollider;

    public override void Initialize()
    {
        base.Initialize();
        _gateCollider = GetComponent<Collider2D>();
        _shadowCaster = GetComponent<ShadowCaster2D>();

        if (Services.IsRegistered<PlayerContext>())
            _playerCollider = Services.Get<PlayerContext>().coll;

        UpdateVisuals();
    }

    private void Update()
    {
        if (!isOpen || _playerWatcher == null) return;

        Vector2 origin = _playerWatcher.interactOrigin != null
            ? _playerWatcher.interactOrigin.position
            : _playerWatcher.transform.position;

        // Mirror OverlapCircleAll: use closest point on collider, not object center
        Vector2 closest = _gateCollider != null
            ? (Vector2)_gateCollider.bounds.ClosestPoint(origin)
            : (Vector2)transform.position;

        if (Vector2.Distance(closest, origin) > _playerWatcher.interactRadius)
            SetOpen(false);
    }

    public override void OnInteract(Interactor interactor)
    {
        bool newState = !isOpen;
        SetOpen(newState);
        _playerWatcher = newState && interactor is PlayerInteractor pi ? pi : null;
    }

    public void SetOpen(bool state)
    {
        if (isOpen == state) return;
        isOpen = state;
        if (!isOpen) _playerWatcher = null;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (!isOpen && _shadowCaster != null)
            _shadowCaster.enabled = true;

        if (_sr != null)
        {
            if (isOpen && openSprite != null) _sr.sprite = openSprite;
            else if (!isOpen && closedSprite != null) _sr.sprite = closedSprite;
        }

        if (isOpen && _shadowCaster != null)
            _shadowCaster.enabled = false;

        if (_gateCollider != null && disableColliderWhenOpen)
            _gateCollider.isTrigger = isOpen;

        if (platformCollider != null)
        {
            if (_playerCollider != null)
                Physics2D.IgnoreCollision(platformCollider, _playerCollider, isOpen);
            else
                platformCollider.enabled = !isOpen;
        }
    }
}
