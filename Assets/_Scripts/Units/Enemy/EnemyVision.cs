using System;
using UnityEngine;
using Entity.Enemy;
using HSM;

[RequireComponent(typeof(EnemyStateDriver))]
public class EnemyVision : MonoBehaviour, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Vision")]
    [SerializeField, Min(0f)] private float viewDistance = 6f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Transform eye;

    private EnemyStateDriver _driver;
    private EnemyContext _ctx;

    public bool CanSeePlayer { get; private set; }
    public Transform Player => _ctx != null ? _ctx.player : null;

    public event Action<Transform> PlayerDetected;
    public event Action PlayerLost;

    public void Initialize()
    {
        _driver = GetComponent<EnemyStateDriver>();
        BindContext(_driver != null ? _driver.Context : null);
    }

    public void BindContext(EnemyContext ctx)
    {
        _ctx = ctx;
        if (_ctx != null) _ctx.vision = this;
    }

    public void SetPlayer(Transform player)
    {
        if (_ctx != null) _ctx.player = player;
    }

    private void Update()
    {
        if (_ctx == null && _driver != null) BindContext(_driver.Context);
        EvaluateVision();
    }

    private void EvaluateVision()
    {
        if (_ctx == null || _ctx.player == null)
        {
            SetCanSee(false);
            return;
        }

        Vector2 origin = eye != null ? (Vector2)eye.position : (Vector2)transform.position;
        Vector2 toPlayer = (Vector2)_ctx.player.position - origin;
        float sqrDistance = toPlayer.sqrMagnitude;
        if (viewDistance > 0f && sqrDistance > viewDistance * viewDistance)
        {
            SetCanSee(false);
            return;
        }

        Vector2 forward = GetForward();
        float angle = Vector2.Angle(forward, toPlayer);
        if (viewAngle < 360f && angle > viewAngle * 0.5f)
        {
            SetCanSee(false);
            return;
        }

        int mask = obstacleMask | playerMask;
        float rayDistance = viewDistance > 0f ? viewDistance : toPlayer.magnitude;
        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, rayDistance, mask);
        bool visible = hit.collider != null &&
                       ((playerMask.value & (1 << hit.collider.gameObject.layer)) != 0);

        SetCanSee(visible);

        if (visible)
        {
            _ctx.lastKnownPlayerPosition = _ctx.player.position;
            _ctx.hasLastKnownPlayerPosition = true;
            _ctx.lastKnownPlayerTime = Time.time;
        }
    }

    private Vector2 GetForward()
    {
        float sign = transform.localScale.x < 0f ? -1f : 1f;
        return (Vector2)transform.right * sign;
    }

    private void SetCanSee(bool value)
    {
        if (CanSeePlayer == value) return;
        CanSeePlayer = value;
        if (value) PlayerDetected?.Invoke(_ctx != null ? _ctx.player : null);
        else PlayerLost?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Vector2 origin = eye != null ? (Vector2)eye.position : (Vector2)transform.position;
        Vector2 forward = GetForward();

        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.9f);
        if (viewDistance > 0f) Gizmos.DrawWireSphere(origin, viewDistance);

        if (viewAngle < 360f)
        {
            float half = viewAngle * 0.5f;
            Vector3 left = Quaternion.Euler(0f, 0f, half) * (Vector3)forward;
            Vector3 right = Quaternion.Euler(0f, 0f, -half) * (Vector3)forward;
            Gizmos.DrawLine(origin, origin + (Vector2)left * viewDistance);
            Gizmos.DrawLine(origin, origin + (Vector2)right * viewDistance);
        }
    }
}
