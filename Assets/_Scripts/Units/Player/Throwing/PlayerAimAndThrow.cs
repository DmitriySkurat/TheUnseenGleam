using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimAndThrow : MonoBehaviour, IInitializable
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
        _ctx = Services.Get<PlayerContext>();

        var pebbleBehavior = pebble != null ? pebble.GetComponent<PebbleBehavior>() : null;
        if (pebbleBehavior != null)
        {
            _baseSpeed = pebbleBehavior.pebbleSpeed;
            _baseGravity = pebbleBehavior.pebbleGravity;
            _currentSpeed = _baseSpeed;
        }
    }
    
    
    private void LateUpdate()
    {
        var attackHeld = _ctx != null && _ctx.input.AttackHeld;

        if (attackHeld)
        {
            HandleHandRotation();
        }

        HandleGunShooting(attackHeld);

        if (!attackHeld)
        {
            _currentAimDistance = 0f;
            _currentSpeed = _baseSpeed;
        }
        _wasAttackHeld = attackHeld;
    }
    
    // private void HandleGunRotation()
    // {
    //     worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    //     direction = (worldPosition - (Vector2)gun.transform.position).normalized;
    //     gun.transform.right = direction;
    // }
    
    private void HandleHandRotation()
    {
        if (_ctx == null || hand == null) return;
        
        if (!_ctx.input.AttackHeld) return;

        var camera = Camera.main;
        if (camera == null) return;

        // var screenPos = Mouse.current != null
        //     ? Mouse.current.position.ReadValue()
        //     : _ctx.input.MousePosition;
        
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
    
    private void HandleGunShooting(bool attackHeld)
    {
        if (_ctx == null) return;
        
        if (_wasAttackHeld && !attackHeld)
        {
            if (_currentAimDistance < minThrowRadius) return;

            bulletInst = Instantiate(pebble, bulletSpawnPoint.position, hand.transform.rotation);

            var pebbleBehavior = bulletInst.GetComponent<PebbleBehavior>();
            if (pebbleBehavior != null)
            {
                pebbleBehavior.SetThrowStats(_currentSpeed, _baseGravity);
            }
        }
    }
}
