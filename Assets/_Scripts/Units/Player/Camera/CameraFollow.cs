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

    [Header("Edge Peek")]
    [SerializeField] private float edgeCheckForwardDistance = 0.35f;
    [SerializeField] private float edgeCheckDepth = 4f;
    [SerializeField] private float edgePeekMinDrop = 0.75f;
    [SerializeField] private float edgePeekDownOffset = 1f;
    [SerializeField] private float edgePeekSmooth = 4f;
    [SerializeField] private float edgeCheckHeightPadding = 0.05f;

    [Header("Falling")]
    [SerializeField] private float minFallingSpeed = 5f;
    [SerializeField] private float fallingDownOffset = 0.5f;
    [SerializeField] private float fallingOffsetSmooth = 6f;

    [Header("Look Around")]
    [SerializeField] private float lookRange = 3f;
    [SerializeField] private float returnSpeed = 5f;
    [SerializeField] private float lookSensivity = 0.01f;

    private Vector3 _currentOffset;
    private float _currentFacingOffset;
    private float _currentMoveOffset;
    private float _currentEdgePeekOffsetY;
    private float _currentFallingOffsetY;
    private float _facingSign = 1f;
    private bool _isLookingAround;
    private bool _wasLookingAround;
    private Vector2 _lookInputOrigin;
    private Vector3 _lookStartTotalOffset;

    private PlayerContext _ctx;
    private Rigidbody2D _targetRb;
    private Collider2D _targetCollider;

    public bool IsLookingAround() => _isLookingAround;

    public void Initialize()
    {
        _ctx = Services.Get<PlayerContext>();

        target = _ctx.transform;
        _targetRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        _targetCollider = target != null ? target.GetComponent<Collider2D>() : null;
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

        float targetEdgePeekOffsetY = EvaluateEdgePeekOffsetY();
        if (edgePeekSmooth <= 0f)
            _currentEdgePeekOffsetY = targetEdgePeekOffsetY;
        else
            _currentEdgePeekOffsetY = Mathf.Lerp(_currentEdgePeekOffsetY, targetEdgePeekOffsetY, Time.deltaTime * edgePeekSmooth);

        float targetFallingOffsetY = EvaluateFallingOffsetY();
        if (fallingOffsetSmooth <= 0f)
            _currentFallingOffsetY = targetFallingOffsetY;
        else
            _currentFallingOffsetY = Mathf.Lerp(_currentFallingOffsetY, targetFallingOffsetY, Time.deltaTime * fallingOffsetSmooth);

        Vector3 followOffset = new Vector3(_currentFacingOffset + _currentMoveOffset, _currentEdgePeekOffsetY + _currentFallingOffsetY, 0f);

        if (_isLookingAround)
        {
            Vector2 centeredLookInput = GetCenteredLookInput();

            if (!_wasLookingAround)
            {
                _lookInputOrigin = centeredLookInput;
                _lookStartTotalOffset = transform.position - (target.position + offset);
                _lookStartTotalOffset.z = 0f;
            }

            Vector2 lookDelta = centeredLookInput - _lookInputOrigin;
            Vector3 desiredTotalOffset = _lookStartTotalOffset + new Vector3(lookDelta.x * lookRange, lookDelta.y * lookRange, 0f);
            desiredTotalOffset.x = Mathf.Clamp(desiredTotalOffset.x, -lookRange, lookRange);
            desiredTotalOffset.y = Mathf.Clamp(desiredTotalOffset.y, -lookRange, lookRange);

            Vector3 desiredOffset = desiredTotalOffset - followOffset;
            float lookSpeed = lookSensivity < 1f ? lookSensivity * 100f : lookSensivity;
            _currentOffset = Vector3.Lerp(_currentOffset, desiredOffset, Time.deltaTime * lookSpeed);
        }
        else
        {
            _currentOffset = Vector3.Lerp(_currentOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        }

        _wasLookingAround = _isLookingAround;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 followOffset = new Vector3(_currentFacingOffset + _currentMoveOffset, _currentEdgePeekOffsetY + _currentFallingOffsetY, 0f);
        Vector3 desiredPos = target.position + offset + followOffset + _currentOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * chaseSpeed);
    }

    private float EvaluateEdgePeekOffsetY()
    {
        if (_ctx == null || _ctx.stats == null || target == null || _targetCollider == null)
            return 0f;

        if (!_ctx.grounded || _ctx.isClimbing || edgeCheckDepth <= 0f || edgePeekDownOffset <= 0f)
            return 0f;

        Bounds bounds = _targetCollider.bounds;
        float facingDirection = Mathf.Sign(Mathf.Abs(_facingSign) > 0f ? _facingSign : 1f);
        Vector2 rayOrigin = new Vector2(
            bounds.center.x + facingDirection * (bounds.extents.x + edgeCheckForwardDistance),
            bounds.min.y + edgeCheckHeightPadding);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, edgeCheckDepth, _ctx.stats.GroundLayer);
        float dropDepth = hit.collider == null ? edgeCheckDepth : rayOrigin.y - hit.point.y;

        if (dropDepth <= edgePeekMinDrop)
            return 0f;

        float peekStrength = Mathf.InverseLerp(edgePeekMinDrop, edgeCheckDepth, dropDepth);
        return -edgePeekDownOffset * peekStrength;
    }

    private float EvaluateFallingOffsetY()
    {
        if (_ctx == null || target == null)
            return 0f;

        // Смещение камеры вниз только при значительном падении
        if (!_ctx.grounded && _targetRb != null)
        {
            float fallSpeed = Mathf.Abs(_targetRb.linearVelocity.y);
            if (fallSpeed >= minFallingSpeed)
            {
                return -fallingDownOffset;
            }
        }

        return 0f;
    }

    private Vector2 GetCenteredLookInput()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return Vector2.zero;

        Vector2 mouse = _ctx.input.MousePosition;
        Vector2 viewport = new Vector2(mouse.x / Screen.width, mouse.y / Screen.height);
        return (viewport - new Vector2(0.5f, 0.5f)) * 2f;
    }
}
