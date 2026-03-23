using HSM;
using UnityEngine;

public class CameraFollow : MonoBehaviour, IInitializable
{
    private const float MovementThreshold = 0.05f;

    public InitializationOrder Order => InitializationOrder.Camera;

    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Facing Offset")]
    [SerializeField] private float facingOffset = 1.5f;
    [SerializeField] private float facingOffsetSmooth = 5f;

    [Header("Movement Offset")]
    [SerializeField] private float moveOffset = 0.5f;
    [SerializeField] private float runMoveOffsetMultiplier = 1.5f;
    [SerializeField] private float moveOffsetSmooth = 8f;

    [Header("Look Around")]
    [SerializeField] private float lookRange = 3f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float lookSensivity = 0.01f;

    private Vector3 _currentOffset;
    private float _currentFacingOffset;
    private float _currentMoveOffset;
    private float _facingSign = 1f;
    private bool _isLookingAround;

    private PlayerContext _ctx;
    private Rigidbody2D _targetRb;

    public bool IsLookingAround() => _isLookingAround;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        target = _ctx.transform;
        _targetRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
    }

    private void Update()
    {
        if (target == null || _ctx == null)
            return;

        float actualMoveX = _targetRb != null ? _targetRb.linearVelocity.x : 0f;
        bool isActuallyMovingHorizontally = Mathf.Abs(actualMoveX) > MovementThreshold;
        float normalizedMoveX = 0f;

        if (isActuallyMovingHorizontally)
        {
            float maxSpeed = _ctx.stats != null ? Mathf.Max(_ctx.stats.MaxSpeed, MovementThreshold) : 1f;
            normalizedMoveX = Mathf.Clamp(actualMoveX / maxSpeed, -1f, 1f);
            _facingSign = Mathf.Sign(actualMoveX);
        }

        bool canLookAround = !isActuallyMovingHorizontally && _ctx.grounded;
        _isLookingAround = canLookAround && _ctx.input.LookAroundHeld;

        if (_isLookingAround)
        {
            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 mouse = _ctx.input.MousePosition;
                Vector2 viewport = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);
                Vector2 centered = (viewport - new Vector2(0.5f, 0.5f)) * 2f;
                Vector3 desiredOffset = new Vector3(centered.x, centered.y, 0f) * lookRange;
                float lookSpeed = lookSensivity < 1f ? lookSensivity * 100f : lookSensivity;
                _currentOffset = Vector3.Lerp(_currentOffset, desiredOffset, Time.deltaTime * lookSpeed);
            }
        }
        else
        {
            _currentOffset = Vector3.Lerp(_currentOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        }

        float targetFacingOffset = _facingSign * facingOffset;
        if (facingOffsetSmooth <= 0f)
            _currentFacingOffset = targetFacingOffset;
        else
            _currentFacingOffset = Mathf.Lerp(_currentFacingOffset, targetFacingOffset, Time.deltaTime * facingOffsetSmooth);

        float runMultiplier = (_ctx.stats != null && _ctx.input.RunHeld && _ctx.CanRun)
            ? runMoveOffsetMultiplier
            : 1f;
        float targetMoveOffset = normalizedMoveX * moveOffset * runMultiplier;

        if (moveOffsetSmooth <= 0f)
            _currentMoveOffset = targetMoveOffset;
        else
            _currentMoveOffset = Mathf.Lerp(_currentMoveOffset, targetMoveOffset, Time.deltaTime * moveOffsetSmooth);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset + new Vector3(_currentFacingOffset + _currentMoveOffset, 0f, 0f);
        Vector3 desiredPos = targetPos + _currentOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * chaseSpeed);
    }
}
