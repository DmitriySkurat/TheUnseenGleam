using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(PathfinderHandler))]
public class EnemyMotor : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("References")]
    [SerializeField] private PathfinderHandler pathfinder;
    [SerializeField] private Transform agent;

    private Transform _moveTarget;

    public void Initialize()
    {
        if (pathfinder == null) pathfinder = GetComponent<PathfinderHandler>();
        if (agent == null) agent = transform;

        EnsureMoveTarget();
    }

    public void MoveTo(Vector2 targetPosition)
    {
        if (pathfinder == null) return;
        EnsureMoveTarget();

        Vector3 pos = new Vector3(targetPosition.x, targetPosition.y, _moveTarget.position.z);
        _moveTarget.position = pos;
        pathfinder.SetTarget(_moveTarget);
        pathfinder.tryFindPath = true;
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

    private void EnsureMoveTarget()
    {
        if (_moveTarget != null) return;
        var go = new GameObject("EnemyMoveTarget");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        _moveTarget = go.transform;
    }
}
