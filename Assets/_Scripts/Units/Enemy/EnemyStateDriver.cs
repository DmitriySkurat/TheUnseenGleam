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
    [SerializeField] private Renderer enemyRenderer;

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
    [SerializeField, Min(0f)] private float lightEscapeDistance = 4f;
    [SerializeField, Min(0f)] private float lightEscapeSpeed = 4.5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color patrolColor = new Color(0.3f, 0.9f, 0.4f, 0.9f);
    [SerializeField] private Color searchColor = new Color(1f, 0.65f, 0.2f, 0.9f);
    [SerializeField] private Color targetColor = new Color(1f, 0.2f, 0.2f, 0.9f);

    private EnemyContext _ctx;
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
            lightEscapeDistance = lightEscapeDistance,
            lightEscapeSpeed = lightEscapeSpeed,
            contactDamage = contactDamage,
            contactDamageCooldown = contactDamageCooldown,
            contactDamageRangeFallback = contactDamageRangeFallback,
            contactAttackWindup = contactAttackWindup,
        };

        if (_ctx.playerContext != null)
            _ctx.playerTransform = _ctx.playerContext.transform;

        _ctx.BuildPatrolRoute();
        _ctx.patrolIndex = _ctx.GetInitialPatrolIndex();

        var root = new EnemyRoot(null, _ctx);
        _machine = new StateMachineBuilder(root).Build();
        _machine.Start();
    }

    public void Dispose() { }

    private void LateUpdate()
    {
        _ctx.ResolvePlayerTransform();
        _ctx.TryApplyContactDamage();

        if (_ctx.TryHandleBlindRun())
            return;

        _machine.Tick(Time.deltaTime);
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
