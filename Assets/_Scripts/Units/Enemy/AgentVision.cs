using UnityEngine;

public class AgentVision : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Vision Settings")]
    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 90f;

    [SerializeField, Min(0f)] private float viewDistance = 5f;
    [Range(0f, 360f)]
    [SerializeField] private float maxViewAngle = 90f;
    [SerializeField, Min(0f)] private float maxViewDistance = 5f;
    [SerializeField, Min(0f)] private float visibilityMultiplier = 1f;
    [SerializeField] private LayerMask occlusionMask;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private Color maxGizmoColor = new Color(0.2f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color lastSeenColor = new Color(1f, 0.4f, 0.1f, 0.9f);
    [SerializeField, Min(0f)] private float lastSeenMarkerRadius = 0.2f;

    private PlayerContext _playerContext;
    private Transform _playerTransform;
    private Vector2? _lastSeenPosition;
    private bool _canSeePlayer;

    public bool CanSeePlayer => _canSeePlayer;
    public bool HasLastSeenPosition => _lastSeenPosition.HasValue;
    public Vector2 LastSeenPosition => _lastSeenPosition ?? Vector2.zero;
    public float ViewAngle => viewAngle;
    public float ViewDistance => viewDistance;
    public float VisibilityMultiplier
    {
        get => visibilityMultiplier;
        set => visibilityMultiplier = Mathf.Max(0f, value);
    }
    
    public void Initialize()
    {
        _playerContext = Services.Get<PlayerContext>();
        _playerTransform = _playerContext.transform;
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            if (_playerContext == null)
            {
                _canSeePlayer = false;
                return;
            }

            _playerTransform = _playerContext.transform;
            if (_playerTransform == null)
            {
                _canSeePlayer = false;
                return;
            }
        }

        _canSeePlayer = CheckPlayerVisibility(_playerTransform.position);
        if (_canSeePlayer)
            _lastSeenPosition = _playerTransform.position;
    }

    private bool CheckPlayerVisibility(Vector2 targetPosition)
    {
        float effectiveViewDistance = GetEffectiveViewDistance();
        float effectiveViewAngle = GetEffectiveViewAngle();
        if (effectiveViewDistance <= 0f || effectiveViewAngle <= 0f)
            return false;

        Vector2 origin = transform.position;
        Vector2 toTarget = targetPosition - origin;
        if (toTarget.sqrMagnitude > effectiveViewDistance * effectiveViewDistance)
            return false;

        Vector2 forward = transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, toTarget);
        if (angle > effectiveViewAngle * 0.5f)
            return false;

        if (occlusionMask.value != 0)
        {
            var hit = Physics2D.Linecast(origin, targetPosition, occlusionMask);
            if (hit.collider != null)
                return false;
        }

        return true;
    }

    private float GetEffectiveViewDistance()
    {
        float effective = viewDistance * Mathf.Max(0f, visibilityMultiplier);
        if (maxViewDistance > 0f)
            effective = Mathf.Min(effective, maxViewDistance);
        return effective;
    }

    private float GetEffectiveViewAngle()
    {
        float effective = viewAngle;
        if (maxViewAngle > 0f)
            effective = Mathf.Min(effective, maxViewAngle);
        return effective;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawVision();
    }

    void OnDrawGizmosSelected()
    {
        DrawVision();
    }

    void DrawVision()
    {
        if (!drawGizmos || !isActiveAndEnabled)
            return;

        Vector3 pos = transform.position;
        Vector3 forward = transform.localScale.x >= 0 ? Vector3.right : Vector3.left;

        DrawVisionCone(pos, forward, maxViewAngle, maxViewDistance, maxGizmoColor);

        float effectiveViewAngle = GetEffectiveViewAngle();
        float effectiveViewDistance = GetEffectiveViewDistance();
        DrawVisionCone(pos, forward, effectiveViewAngle, effectiveViewDistance, gizmoColor);

        if (_lastSeenPosition.HasValue)
        {
            Gizmos.color = lastSeenColor;
            Gizmos.DrawSphere(_lastSeenPosition.Value, lastSeenMarkerRadius);
        }
    }

    private void DrawVisionCone(Vector3 pos, Vector3 forward, float angle, float distance, Color color)
    {
        if (angle <= 0f || distance <= 0f)
            return;

        Gizmos.color = color;

        float halfAngle = angle * 0.5f;
        Vector3 leftDir = Quaternion.AngleAxis(-halfAngle, Vector3.forward) * forward;
        Vector3 rightDir = Quaternion.AngleAxis(halfAngle, Vector3.forward) * forward;

        Gizmos.DrawLine(pos, pos + leftDir * distance);
        Gizmos.DrawLine(pos, pos + rightDir * distance);

        int segments = 30;
        Vector3 prevPoint = pos + leftDir * distance;

        for (int i = 1; i <= segments; i++)
        {
            float segAngle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            Vector3 dir = Quaternion.AngleAxis(segAngle, Vector3.forward) * forward;
            Vector3 point = pos + dir * distance;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
#endif
}
