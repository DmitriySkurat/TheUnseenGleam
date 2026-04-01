using System.Collections.Generic;
using PlatNav;
using UnityEngine;
using HSM;

[RequireComponent(typeof(PlatNavHandler))]
[RequireComponent(typeof(AgentVision))]
[RequireComponent(typeof(AgentHearing))]
public class EnemyStateDriver : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy + 10;

    [Header("References")]
    [SerializeField] private PlatNavHandler enemy;
    [SerializeField] private AgentVision vision;
    [SerializeField] private AgentHearing hearing;
    [SerializeField] private AgentLightSensor lightSensor;
    private Renderer enemyRenderer;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float patrolHalfWidth = 2f;
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float patrolWaitTime = 1.25f;

    [Header("Chase")]
    [SerializeField, Min(0f)] private float chaseSpeed = 4.5f;
    [SerializeField, Min(0f)] private float retargetDistance = 0.35f;

    [Header("Contact Attack")]
    [SerializeField, Min(0f)] private float contactDamage = 40f;
    [SerializeField, Min(0f)] private float contactDamageCooldown = 2f;
    [SerializeField, Min(0f)] private float contactDamageRangeFallback = 0.75f;
    [SerializeField, Min(0f)] private float contactAttackWindup = 0.35f;

    [Header("Investigate")]
    [SerializeField, Min(0f)] private float investigateSpeed = 3f;
    [SerializeField, Min(0f)] private float investigateWaitTime = 0.75f;

    [Header("Search")]
    [SerializeField, Min(0f)] private float searchSpeed = 2.5f;
    [SerializeField, Min(0f)] private float searchHalfWidth = 1.75f;
    [SerializeField, Min(0f)] private float searchDuration = 4f;
    [SerializeField, Min(0f)] private float searchWaitTime = 0.6f;

    [Header("Return")]
    [SerializeField, Min(0f)] private float returnSpeed = 3f;

    [Header("Patrol Under Light")]
    [SerializeField, Min(0f)] private float lightEscapeSpeed = 4.5f;
    [SerializeField, Min(0f)] private float lightOverrunDistance = 2f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color patrolColor = new Color(0.3f, 0.9f, 0.4f, 0.9f);
    [SerializeField] private Color searchColor = new Color(1f, 0.65f, 0.2f, 0.9f);
    [SerializeField] private Color targetColor = new Color(1f, 0.2f, 0.2f, 0.9f);

    private EnemyContext _ctx;
    private EnemyRoot _root;
    private StateMachine _machine;

    public void Initialize()
    {
        if (enemy == null) enemy = GetComponent<PlatNavHandler>();
        if (vision == null) vision = GetComponent<AgentVision>();
        if (hearing == null) hearing = GetComponent<AgentHearing>();
        if (lightSensor == null) lightSensor = GetComponent<AgentLightSensor>();
        if (enemyRenderer == null) enemyRenderer = GetComponentInChildren<Renderer>();

        _ctx = new EnemyContext
        {
            nav = enemy,
            vision = vision,
            hearing = hearing,
            lightSensor = lightSensor,
            renderer = enemyRenderer,
            selfTransform = transform,
            selfCollider = GetComponent<Collider2D>(),
            rb = GetComponent<Rigidbody2D>(),
            playerContext = Services.Get<PlayerContext>(),

            patrolPoints = patrolPoints,
            patrolHalfWidth = patrolHalfWidth,
            patrolSpeed = patrolSpeed,
            patrolWaitTime = patrolWaitTime,
            chaseSpeed = chaseSpeed,
            retargetDistance = retargetDistance,
            investigateSpeed = investigateSpeed,
            investigateWaitTime = investigateWaitTime,
            searchSpeed = searchSpeed,
            searchHalfWidth = searchHalfWidth,
            searchDuration = searchDuration,
            searchWaitTime = searchWaitTime,
            returnSpeed = returnSpeed,
            lightEscapeSpeed = lightEscapeSpeed,
            lightOverrunDistance = lightOverrunDistance,
            contactDamage = contactDamage,
            contactDamageCooldown = contactDamageCooldown,
            contactDamageRangeFallback = contactDamageRangeFallback,
            contactAttackWindup = contactAttackWindup,
        };

        if (_ctx.playerContext != null)
            _ctx.playerTransform = _ctx.playerContext.transform;

        BuildPatrolRoute();

        _root = new EnemyRoot(null, _ctx);
        _ctx.patrolIndex = _root.GetInitialPatrolIndex();

        _machine = new StateMachineBuilder(_root).Build();
        _machine.Start();
    }

    public void Dispose() { }

    private void LateUpdate()
    {
        ResolvePlayerTransform();
        TryApplyContactDamage();
        _machine.Tick(Time.deltaTime);
    }

    private void BuildPatrolRoute()
    {
        _ctx.patrolOrigin = transform.position;

        var points = new List<Vector2>();
        if (_ctx.patrolPoints != null)
        {
            for (int i = 0; i < _ctx.patrolPoints.Length; i++)
            {
                if (_ctx.patrolPoints[i] != null)
                    points.Add(_ctx.patrolPoints[i].position);
            }
        }

        if (points.Count >= 2)
        {
            _ctx.patrolRoute = points.ToArray();
            return;
        }

        Vector2 offset = Vector2.right * _ctx.patrolHalfWidth;
        _ctx.patrolRoute = new[] { _ctx.patrolOrigin - offset, _ctx.patrolOrigin + offset };
    }

    private void ResolvePlayerTransform()
    {
        if (_ctx.playerTransform != null)
            return;

        if (_ctx.playerContext == null)
            _ctx.playerContext = Services.Get<PlayerContext>();

        if (_ctx.playerContext != null)
            _ctx.playerTransform = _ctx.playerContext.transform;
    }

    private void TryApplyContactDamage()
    {
        if (_ctx.contactDamage <= 0f)
            return;

        if (_ctx.lightSensor != null && _ctx.lightSensor.IsBlinded(out _, out _))
        {
            _ctx.contactAttackTimer = 0f;
            return;
        }

        if (Time.time < _ctx.nextContactDamageTime)
        {
            _ctx.contactAttackTimer = 0f;
            return;
        }

        if (_ctx.playerContext == null || _ctx.playerContext.health == null || !_ctx.playerContext.isAlive || _ctx.playerTransform == null)
        {
            _ctx.contactAttackTimer = 0f;
            return;
        }

        if (!IsPlayerInContactRange())
        {
            _ctx.contactAttackTimer = 0f;
            return;
        }

        if (_ctx.contactAttackWindup > 0f)
        {
            _ctx.contactAttackTimer += Time.deltaTime;
            if (_ctx.contactAttackTimer < _ctx.contactAttackWindup)
                return;
        }

        _ctx.playerContext.health.TakeDamage(_ctx.contactDamage);
        _ctx.nextContactDamageTime = Time.time + _ctx.contactDamageCooldown;
        _ctx.contactAttackTimer = 0f;
    }

    private bool IsPlayerInContactRange()
    {
        if (_ctx.selfCollider != null && _ctx.playerContext != null && _ctx.playerContext.coll != null)
            return _ctx.selfCollider.Distance(_ctx.playerContext.coll).isOverlapped;

        float sqrDist = ((Vector2)_ctx.selfTransform.position - (Vector2)_ctx.playerTransform.position).sqrMagnitude;
        return sqrDist <= _ctx.contactDamageRangeFallback * _ctx.contactDamageRangeFallback;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || _ctx == null)
            return;

        DrawRoute(_ctx.patrolRoute, patrolColor);
        DrawRoute(_ctx.searchRoute, searchColor);

        if (_ctx.manualCommandActive)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawSphere(_ctx.manualDestination, 0.2f);
        }
    }

    private static void DrawRoute(System.Collections.Generic.IReadOnlyList<UnityEngine.Vector2> route, Color color)
    {
        if (route == null || route.Count == 0)
            return;

        Gizmos.color = color;
        for (int i = 0; i < route.Count; i++)
        {
            Gizmos.DrawSphere(route[i], 0.12f);
            if (i + 1 < route.Count)
                Gizmos.DrawLine(route[i], route[i + 1]);
        }
    }
#endif
}
