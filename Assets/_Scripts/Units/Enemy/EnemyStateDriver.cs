using System.Collections.Generic;
using PlatNav;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using HSM;

[RequireComponent(typeof(PlatNavHandler))]
[RequireComponent(typeof(AgentVision))]
[RequireComponent(typeof(AgentHearing))]
public class EnemyStateDriver : MonoBehaviour, ISceneLifecycle, IInitializable
{
    public InitializationOrder Order => InitializationOrder.Enemy + 10;

    [Header("References")]
    [SerializeField] private PlatNavHandler enemy;
    [SerializeField] private AgentVision vision;
    [SerializeField] private AgentHearing hearing;
    [SerializeField] private AgentLightSensor lightSensor;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField, Min(0f)] private float patrolHalfWidth = 2f;
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float patrolWaitTime = 1.25f;

    [Header("Chase")]
    [SerializeField, Min(0f)] private float chaseSpeed = 4.5f;
    [SerializeField, Min(0f)] private float retargetDistance = 0.35f;

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

    private PlayerContext _playerContext;
    private EnemyContext _ctx;
    private StateMachine _machine;
    private EnemyRoot _root;

    public void Initialize()
    {
        // Create context
        _ctx = new EnemyContext
        {
            navHandler = GetComponent<PlatNavHandler>(),
            vision = GetComponent<AgentVision>(),
            hearing = GetComponent<AgentHearing>(),
            lightSensor = GetComponent<AgentLightSensor>(),
            transform = transform,
            animator = GetComponentInChildren<Animator>(),
            rb = GetComponentInChildren<Rigidbody2D>(),
            
            // Initialize patrol settings
            patrolSpeed = patrolSpeed,
            patrolHalfWidth = patrolHalfWidth,
            patrolWaitTime = patrolWaitTime,
            
            // Initialize chase settings
            chaseSpeed = chaseSpeed,
            retargetDistance = retargetDistance,
            
            // Initialize investigate settings
            investigateSpeed = investigateSpeed,
            investigateWaitTime = investigateWaitTime,
            
            // Initialize search settings
            searchDuration = searchDuration,
            searchSpeed = searchSpeed,
            searchHalfWidth = searchHalfWidth,
            searchWaitTime = searchWaitTime,
            
            // Initialize return settings
            returnSpeed = returnSpeed,
            
            // Initialize light response settings
            lightEscapeDistance = lightEscapeDistance,
            lightEscapeSpeed = lightEscapeSpeed,
        };

        // Get player context
        _playerContext = Services.Get<PlayerContext>();
        _ctx.playerContext = _playerContext;

        // Build patrol route
        BuildPatrolRoute();

        // Initialize HSM
        _root = new EnemyRoot(null, _ctx);
        var builder = new StateMachineBuilder(_root);
        _machine = builder.Build();

        // Setup hearing listener
        if (_ctx.hearing != null)
        {
            _ctx.hearing.OnHeard += HandleHeard;
        }
    }

    public void Dispose()
    {
        if (_ctx.hearing != null)
        {
            _ctx.hearing.OnHeard -= HandleHeard;
        }
    }

    private void BuildPatrolRoute()
    {
        _ctx.patrolOrigin = transform.position;

        var points = new List<Vector2>();
        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                    points.Add(patrolPoints[i].position);
            }
        }

        if (points.Count >= 2)
        {
            _ctx.patrolRoute = points.ToArray();
            return;
        }

        Vector2 offset = Vector2.right * patrolHalfWidth;
        _ctx.patrolRoute = new[]
        {
            _ctx.patrolOrigin - offset,
            _ctx.patrolOrigin + offset,
        };
    }

    private void HandleHeard(NoiseEvent noiseEvent, float loudness)
    {
        if (!isActiveAndEnabled)
            return;

        bool isPlayerNoise = IsPlayerNoise(noiseEvent.Source);

        if (isPlayerNoise && _ctx.hasDetectedPlayer)
        {
            UpdateKnownPlayerPosition(noiseEvent.Position);
            if (IsTraversingLink())
            {
                _machine.Sequencer.RequestTransition(_machine.Root.Leaf(), _root.Chasing);
                return;
            }

            EnterChasing(useVisualContact: false);
            return;
        }

        if (_machine.Root.Leaf() is EnemyChasing && !isPlayerNoise)
            return;

        if (IsTraversingLink())
            return;

        EnterInvestigating(noiseEvent.Position);
    }

    private void EnterChasing(bool useVisualContact)
    {
        _machine.Sequencer.RequestTransition(_machine.Root.Leaf(), _root.Chasing);
        
        if (!useVisualContact && _ctx.lastKnownPlayerPosition != Vector2.zero)
        {
            _ctx.usingVisualChase = false;
        }
    }

    private void EnterInvestigating(Vector2 targetPosition)
    {
        _ctx.investigationTarget = targetPosition;
        _machine.Sequencer.RequestTransition(_machine.Root.Leaf(), _root.Investigating);
    }

    private void UpdateKnownPlayerPosition(Vector2 position)
    {
        _ctx.lastKnownPlayerPosition = position;
        _ctx.hasKnownPlayerPosition = true;
        _ctx.hasDetectedPlayer = true;
    }

    private bool IsTraversingLink()
    {
        return _ctx.navHandler != null && _ctx.navHandler.State == PlatNavState.TraversingLink;
    }

    private bool IsPlayerNoise(GameObject source)
    {
        if (_ctx.playerTransform == null || source == null)
            return false;

        return source == _ctx.playerTransform.gameObject;
    }

    private void FixedUpdate()
    {
        if (_machine == null)
            return;

        _machine.Tick(Time.fixedDeltaTime);

        #if UNITY_EDITOR
        PrintStatePath();
        #endif
    }

    #region Debugging

    private string _lastPath;

    void PrintStatePath()
    {
        if (_machine == null || _machine.Root == null)
            return;

        var path = StatePath(_machine.Root.Leaf());
        if (path != _lastPath)
        {
            Debug.Log("Enemy State: " + path, gameObject);
            _lastPath = path;
        }
    }

    static string StatePath(State s)
    {
        if (s == null)
            return "None";

        var names = new List<string>();
        State current = s;
        while (current != null)
        {
            names.Add(current.GetType().Name);
            var parentField = current.GetType().BaseType?.GetField("Parent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (parentField != null)
                current = parentField.GetValue(current) as State;
            else
                break;
        }

        names.Reverse();
        return string.Join(" > ", names);
    }

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

    private void DrawRoute(Vector2[] route, Color color)
    {
        if (route == null || route.Length < 2)
            return;

        Gizmos.color = color;
        for (int i = 0; i < route.Length - 1; i++)
        {
            Gizmos.DrawLine(route[i], route[i + 1]);
        }

        foreach (var point in route)
        {
            Gizmos.DrawSphere((Vector3)point, 0.15f);
        }
    }

    #endregion
}