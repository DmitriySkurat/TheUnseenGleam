using UnityEngine;

public class PlayerAimAndThrow : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Player + 1;
    
    [SerializeField] private GameObject hand;
    public GameObject pebble;
    [SerializeField] private Transform bulletSpawnPoint;

    [Header("Throw Power")]
    [SerializeField] private float minThrowRadius = 0.5f;
    [SerializeField] private float maxThrowRadius = 4f;
    [SerializeField] private float maxSpeedMultiplier = 2f;
    [SerializeField] private float maxDownDistance = 2f;
    
    private GameObject bulletInst;
    
    private Vector2 worldPosition;
    private Vector2 direction;

    private float _baseSpeed;
    private float _baseGravity;
    private float _currentAimDistance;
    private float _currentSpeed;
    private bool _wasAttackHeld;
    
    private PlayerContext _ctx;

    public float CurrentProjectileSpeed => _currentSpeed;
    public float CurrentAimDistance => _currentAimDistance;
    public float MinThrowRadius => minThrowRadius;
    
    public void Initialize()
    {
        if (!Services.IsRegistered<PlayerContext>())
            return;
        _ctx = Services.Get<PlayerContext>();

        var pebbleBehavior = pebble != null ? pebble.GetComponent<PebbleBehavior>() : null;
        if (pebbleBehavior != null)
        {
            _baseSpeed = pebbleBehavior.pebbleSpeed;
            _baseGravity = pebbleBehavior.pebbleGravity;
            _currentSpeed = _baseSpeed;
        }
    }
    
    public void Dispose()
    {
        
    }
    
    
    private void LateUpdate()
    {
        if (!CanUseSelectedPebble())
        {
            ResetAimState();
            _wasAttackHeld = false;
            return;
        }

        var attackHeld = _ctx.input.AttackHeld;

        if (attackHeld)
        {
            HandleHandRotation();
        }

        HandleThrowing(attackHeld);

        if (!attackHeld)
        {
            ResetAimState();
        }
        _wasAttackHeld = attackHeld;
    }
    
    private void HandleHandRotation()
    {
        if (_ctx == null || hand == null) return;
        
        if (!_ctx.input.AttackHeld) return;

        var camera = Camera.main;
        if (camera == null) return;
        
        var screenPos = _ctx.input.MousePosition;

        var camZ = -camera.transform.position.z;
        worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZ));
        var handPos = (Vector2)hand.transform.position;
        direction = (worldPosition - handPos).normalized;

        if (direction.sqrMagnitude > 0.0001f)
            hand.transform.up = direction;

        var rawDistance = Vector2.Distance(worldPosition, handPos);
        var downDistance = Mathf.Max(0f, handPos.y - worldPosition.y);
        var downPenalty = maxDownDistance > 0f ? 1f - Mathf.Clamp01(downDistance / maxDownDistance) : 1f;
        var adjustedDistance = rawDistance * downPenalty;
        _currentAimDistance = maxThrowRadius > 0f ? Mathf.Min(adjustedDistance, maxThrowRadius) : adjustedDistance;

        var t = Mathf.InverseLerp(minThrowRadius, maxThrowRadius, _currentAimDistance);
        _currentSpeed = _baseSpeed * Mathf.Lerp(1f, maxSpeedMultiplier, t);
    }
    
    private void HandleThrowing(bool attackHeld)
    {
        if (_ctx == null) return;
        
        if (_wasAttackHeld && !attackHeld)
        {
            if (_currentAimDistance < minThrowRadius) return;
            if (!_ctx.inventory.TryUse(_ctx.hotbar?.selectedHotbarEntry, _ctx)) return;

            bulletInst = Instantiate(pebble, bulletSpawnPoint.position, hand.transform.rotation);

            var pebbleBehavior = bulletInst.GetComponent<PebbleBehavior>();
            if (pebbleBehavior != null)
            {
                pebbleBehavior.SetThrowStats(_currentSpeed, _baseGravity);
            }
        }
    }

    private bool CanUseSelectedPebble()
    {
        return _ctx != null
            && _ctx.SelectedHotbarItem != null
            && _ctx.SelectedHotbarItem.itemName == ItemName.Pebble
            && _ctx.inventory != null
            && _ctx.inventory.Has(_ctx.SelectedHotbarItem);
    }

    private void ResetAimState()
    {
        _currentAimDistance = 0f;
        _currentSpeed = _baseSpeed;
    }
}
