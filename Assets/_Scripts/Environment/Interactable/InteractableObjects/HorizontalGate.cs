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
    private PlayerContext _playerCtx;
    private bool _pendingGoSolid;

    public override void Initialize()
    {
        base.Initialize();
        _gateCollider = GetComponent<Collider2D>();
        _shadowCaster = GetComponent<ShadowCaster2D>();

        if (Services.IsRegistered<PlayerContext>())
        {
            _playerCtx = Services.Get<PlayerContext>();
            _playerCollider = _playerCtx.coll;
        }

        UpdateVisuals();
    }

    private void Update()
    {
        if (_pendingGoSolid && _gateCollider != null && _playerCollider != null)
        {
            if (_playerCtx != null) _playerCtx.gateBlocksStanding = true;

            if (!_gateCollider.bounds.Intersects(_playerCollider.bounds))
            {
                _gateCollider.isTrigger = false;
                _pendingGoSolid = false;
                if (_playerCtx != null) _playerCtx.gateBlocksStanding = false;
                if (platformCollider != null)
                {
                    if (_playerCollider != null)
                        platformCollider.excludeLayers = default;
                    else
                        platformCollider.enabled = true;
                }
            }
        }

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
        {
            if (isOpen)
            {
                _gateCollider.isTrigger = true;
                _pendingGoSolid = false;
                if (_playerCtx != null) _playerCtx.gateBlocksStanding = false;
            }
            else
            {
                bool playerOverlaps = _playerCollider != null && _gateCollider.bounds.Intersects(_playerCollider.bounds);
                if (playerOverlaps)
                {
                    // Keep gate as trigger until player crouches clear of it
                    _gateCollider.isTrigger = true;
                    _pendingGoSolid = true;
                    if (_playerCtx != null) _playerCtx.gateBlocksStanding = true;
                }
                else
                {
                    _gateCollider.isTrigger = false;
                    _pendingGoSolid = false;
                }
            }
        }

        if (platformCollider != null)
        {
            // Delay platform change while pending to avoid physics depenetration push during crouch.
            bool treatAsOpen = isOpen || _pendingGoSolid;
            if (_playerCollider != null)
                platformCollider.excludeLayers = treatAsOpen
                    ? (LayerMask)(1 << _playerCollider.gameObject.layer)
                    : default;
            else
                platformCollider.enabled = !treatAsOpen;
        }
    }
}
