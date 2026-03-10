using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(PathfinderHandler))]
public class EnemyMotor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("References")]
    [SerializeField] private PathfinderHandler pathfinder;
    [SerializeField] private Transform agent;
    [SerializeField] private Rigidbody2D rb;

    [Header("Target Snapping")]
    [SerializeField] private bool snapToWalkableTargets = true;
    [SerializeField, Min(0)] private int snapRadius = 3;
    [SerializeField, Min(0f)] private float snapVerticalWeight = 2.5f;

    [Header("Facing")]
    [SerializeField, Min(0f)] private float facingDeadzone = 0.05f;

    private Transform _moveTarget;
    private static Transform _targetsRoot;

    public void Initialize()
    {
        if (pathfinder == null) pathfinder = GetComponent<PathfinderHandler>();
        if (agent == null) agent = transform;
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = GetComponentInChildren<Rigidbody2D>();
        }

        EnsureMoveTarget();
    }

    private void Update()
    {
        UpdateFacing();
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (pathfinder == null) return;
        EnsureMoveTarget();

        Vector2 resolved = targetPosition;
        TryResolveTarget(targetPosition, out resolved);

        Vector3 pos = new Vector3(resolved.x, resolved.y, _moveTarget.position.z);
        _moveTarget.position = pos;
        pathfinder.SetTarget(_moveTarget);
        pathfinder.tryFindPath = true;
    }

    public bool TryResolveTarget(Vector2 targetPosition, out Vector2 resolved)
    {
        resolved = targetPosition;
        if (pathfinder == null) return false;
        if (!snapToWalkableTargets) return true;
        return pathfinder.TryGetNearestWalkablePosition(targetPosition, snapRadius, out resolved, snapVerticalWeight);
    }

    public void MoveTo(Transform target)
    {
        if (pathfinder == null) return;
        if (target == null)
        {
            Stop();
            return;
        }

        pathfinder.SetTarget(target);
        pathfinder.tryFindPath = true;
    }

    public void Stop()
    {
        if (pathfinder == null) return;
        pathfinder.tryFindPath = false;
        pathfinder.SetVelocity(Vector2.zero);
    }

    public void FaceTowards(Vector2 targetPosition)
    {
        float dx = targetPosition.x - transform.position.x;
        if (Mathf.Abs(dx) <= facingDeadzone) return;

        Vector3 scale = transform.localScale;
        float sign = Mathf.Sign(dx);
        float absX = Mathf.Abs(scale.x);
        if (absX < 0.001f) absX = 1f;
        if (Mathf.Sign(scale.x) != sign)
        {
            scale.x = absX * sign;
            transform.localScale = scale;
        }
    }

    private void EnsureMoveTarget()
    {
        if (_moveTarget != null) return;
        Transform root = GetTargetsRoot();
        var go = new GameObject("EnemyMoveTarget");
        go.transform.SetParent(root, true);
        go.transform.localPosition = Vector3.zero;
        _moveTarget = go.transform;
    }

    private static Transform GetTargetsRoot()
    {
        if (_targetsRoot != null) return _targetsRoot;
        var existing = GameObject.Find("EnemyTargetsRoot");
        if (existing != null)
        {
            _targetsRoot = existing.transform;
            return _targetsRoot;
        }
        var root = new GameObject("EnemyTargetsRoot");
        _targetsRoot = root.transform;
        return _targetsRoot;
    }

    private void UpdateFacing()
    {
        if (rb == null) return;
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) <= facingDeadzone) return;

        Vector3 scale = transform.localScale;
        float sign = Mathf.Sign(vx);
        float absX = Mathf.Abs(scale.x);
        if (absX < 0.001f) absX = 1f;
        if (Mathf.Sign(scale.x) != sign)
        {
            scale.x = absX * sign;
            transform.localScale = scale;
        }
    }
}
