using System.Collections.Generic;
using PlatNav;
using UnityEngine;

[RequireComponent(typeof(PlatNavHandler))]
[RequireComponent(typeof(AgentVision))]
[RequireComponent(typeof(AgentHearing))]
public class EnemyStateDriver : MonoBehaviour, ISceneLifecycle
{
    public enum EnemyState
    {
        Patrol,
        Chasing,
        Investigating,
        Searching,
        ReturningToPatrol,
    }

    public InitializationOrder Order => InitializationOrder.Enemy + 10;

    [Header("References")]
    [SerializeField] private PlatNavHandler enemy;
    [SerializeField] private AgentVision vision;
    [SerializeField] private AgentHearing hearing;

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

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color patrolColor = new Color(0.3f, 0.9f, 0.4f, 0.9f);
    [SerializeField] private Color searchColor = new Color(1f, 0.65f, 0.2f, 0.9f);
    [SerializeField] private Color targetColor = new Color(1f, 0.2f, 0.2f, 0.9f);

    public EnemyState CurrentState => _state;

    private PlayerContext _playerContext;
    private Transform _playerTransform;
    private EnemyState _state;
    private Vector2 _patrolOrigin;
    private Vector2[] _patrolRoute;
    private int _patrolIndex;
    private int _patrolDirection = 1;
    private Vector2 _investigationTarget;
    private Vector2 _returnTarget;
    private Vector2 _lastKnownPlayerPosition;
    private bool _hasKnownPlayerPosition;
    private bool _hasDetectedPlayer;
    private bool _usingVisualChase;
    private bool _wasSeeingPlayerLastFrame;
    private bool _isWaiting;
    private float _waitTimer;
    private Vector2 _manualDestination;
    private bool _manualCommandActive;
    private bool _manualCommandFailed;
    private readonly Vector2[] _searchRoute = new Vector2[2];
    private int _searchIndex;
    private int _searchDirection = 1;
    private float _searchTimer;

    public void Initialize()
    {
        if (enemy == null)
            enemy = GetComponent<PlatNavHandler>();
        if (vision == null)
            vision = GetComponent<AgentVision>();
        if (hearing == null)
            hearing = GetComponent<AgentHearing>();

        _playerContext = Services.Get<PlayerContext>();
        _playerTransform = _playerContext != null ? _playerContext.transform : null;

        BuildPatrolRoute();

        if (hearing != null)
            hearing.OnHeard += HandleHeard;

        EnterPatrol(true);
    }

    public void Dispose()
    {
        if (hearing != null)
            hearing.OnHeard -= HandleHeard;
    }

    private void LateUpdate()
    {
        ResolvePlayerTransform();

        bool canSeePlayer = vision != null && vision.CanSeePlayer && _playerTransform != null;
        if (canSeePlayer)
        {
            UpdateKnownPlayerPosition(_playerTransform.position);
            if (IsTraversingLink())
            {
                _state = EnemyState.Chasing;
                ResetWait();
            }
            else if (_state != EnemyState.Chasing || !_usingVisualChase)
                EnterChasing(useVisualContact: true);
        }
        else if (_state == EnemyState.Chasing && _hasDetectedPlayer && _playerContext != null && _playerContext.isHiding && _playerTransform != null)
        {
            UpdateKnownPlayerPosition(_playerTransform.position);
        }

        _wasSeeingPlayerLastFrame = canSeePlayer;

        if (IsTraversingLink())
            return;

        switch (_state)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chasing:
                UpdateChasing(canSeePlayer);
                break;
            case EnemyState.Investigating:
                UpdateInvestigating();
                break;
            case EnemyState.Searching:
                UpdateSearching();
                break;
            case EnemyState.ReturningToPatrol:
                UpdateReturnToPatrol();
                break;
        }
    }

    private void HandleHeard(NoiseEvent noiseEvent, float loudness)
    {
        if (!isActiveAndEnabled)
            return;

        if (noiseEvent.Source == null || noiseEvent.Source == gameObject)
            return;

        bool isPlayerNoise = IsPlayerNoise(noiseEvent.Source);

        if (isPlayerNoise && _hasDetectedPlayer)
        {
            UpdateKnownPlayerPosition(noiseEvent.Position);
            if (IsTraversingLink())
            {
                _state = EnemyState.Chasing;
                ResetWait();
                return;
            }

            EnterChasing(useVisualContact: false);
            return;
        }

        if (_state == EnemyState.Chasing && !isPlayerNoise)
            return;

        if (IsTraversingLink())
            return;

        EnterInvestigating(noiseEvent.Position);
    }

    private void UpdatePatrol()
    {
        if (_patrolRoute == null || _patrolRoute.Length == 0)
            return;

        if (_isWaiting)
        {
            if (!UpdateWaitTimer())
                return;

            TryMoveTo(_patrolRoute[_patrolIndex], patrolSpeed);
        }

        if (!TryMoveTo(_patrolRoute[_patrolIndex], patrolSpeed))
        {
            AdvancePatrolIndex();
            BeginWait(patrolWaitTime);
            return;
        }

        if (HasCompletedManualMove())
        {
            AdvancePatrolIndex();
            BeginWait(patrolWaitTime);
        }
    }

    private void UpdateChasing(bool canSeePlayer)
    {
        if (canSeePlayer && _playerTransform != null)
        {
            BeginVisualChase();
            return;
        }

        if (!_hasKnownPlayerPosition)
        {
            StartSearchFrom(transform.position);
            return;
        }

        if (!TryMoveTo(_lastKnownPlayerPosition, chaseSpeed))
        {
            StartSearchFrom(_lastKnownPlayerPosition);
            return;
        }

        if (HasCompletedManualMove())
            StartSearchFrom(_lastKnownPlayerPosition);
    }

    private void UpdateInvestigating()
    {
        if (_isWaiting)
        {
            if (!UpdateWaitTimer())
                return;

            EnterReturningToPatrol();
            return;
        }

        if (!TryMoveTo(_investigationTarget, investigateSpeed))
        {
            EnterReturningToPatrol();
            return;
        }

        if (HasCompletedManualMove())
            BeginWait(investigateWaitTime);
    }

    private void UpdateSearching()
    {
        _searchTimer -= Time.deltaTime;
        if (_searchTimer <= 0f)
        {
            EnterReturningToPatrol();
            return;
        }

        if (_isWaiting)
        {
            if (!UpdateWaitTimer())
                return;

            TryMoveTo(_searchRoute[_searchIndex], searchSpeed);
        }

        if (!TryMoveTo(_searchRoute[_searchIndex], searchSpeed))
        {
            EnterReturningToPatrol();
            return;
        }

        if (HasCompletedManualMove())
        {
            AdvanceSearchIndex();
            BeginWait(searchWaitTime);
        }
    }

    private void UpdateReturnToPatrol()
    {
        if (!TryMoveTo(_returnTarget, returnSpeed))
        {
            EnterPatrol(true);
            return;
        }

        if (!HasCompletedManualMove())
            return;

        _patrolDirection = 1;
        _patrolIndex = _patrolRoute != null && _patrolRoute.Length > 1 ? 1 : 0;
        BeginWait(patrolWaitTime);
        _state = EnemyState.Patrol;
    }

    private void EnterPatrol(bool resetRoute)
    {
        StopVisualChase();
        if (resetRoute)
        {
            _patrolDirection = 1;
            _patrolIndex = GetInitialPatrolIndex();
        }

        _state = EnemyState.Patrol;
        ResetWait();
        ResetManualCommand();
    }

    private void EnterInvestigating(Vector2 targetPosition)
    {
        StopVisualChase();
        _state = EnemyState.Investigating;
        _investigationTarget = targetPosition;
        ResetWait();
        ForceMoveTo(_investigationTarget, investigateSpeed);
    }

    private void EnterChasing(bool useVisualContact)
    {
        _state = EnemyState.Chasing;
        ResetWait();

        if (useVisualContact)
        {
            BeginVisualChase();
            return;
        }

        StopVisualChase();
        ForceMoveTo(_lastKnownPlayerPosition, chaseSpeed);
    }

    private void StartSearchFrom(Vector2 center)
    {
        StopVisualChase();
        _state = EnemyState.Searching;
        _searchTimer = searchDuration;
        ResetWait();

        Vector2 offset = Vector2.right * searchHalfWidth;
        _searchRoute[0] = center - offset;
        _searchRoute[1] = center + offset;
        _searchDirection = 1;
        _searchIndex = GetClosestIndex(_searchRoute, transform.position);

        ForceMoveTo(_searchRoute[_searchIndex], searchSpeed);
    }

    private void EnterReturningToPatrol()
    {
        StopVisualChase();
        _state = EnemyState.ReturningToPatrol;
        ResetWait();
        _returnTarget = GetPatrolReturnPoint();
        ForceMoveTo(_returnTarget, returnSpeed);
    }

    private void BeginVisualChase()
    {
        if (enemy == null || _playerTransform == null)
            return;

        if (_usingVisualChase)
            return;

        enemy.Abort();
        enemy.SetBehaviour(PlatNavBehaviour.FollowTarget);
        enemy.SetTarget(_playerTransform);
        enemy.MoveTo(_playerTransform.position, chaseSpeed);
        _usingVisualChase = true;
        ResetManualCommand();
    }

    private void StopVisualChase()
    {
        if (enemy == null)
            return;

        enemy.SetTarget(null);
        if (_usingVisualChase)
            enemy.Abort();

        _usingVisualChase = false;
    }

    private bool TryMoveTo(Vector2 targetPosition, float speed)
    {
        if (enemy == null)
            return false;

        float minRetargetDistance = Mathf.Max(0.01f, retargetDistance);
        bool needsNewCommand = !_manualCommandActive
            || _manualCommandFailed
            || (_manualDestination - targetPosition).sqrMagnitude > minRetargetDistance * minRetargetDistance;

        if (!needsNewCommand)
            return !_manualCommandFailed;

        ForceMoveTo(targetPosition, speed);
        return !_manualCommandFailed;
    }

    private void ForceMoveTo(Vector2 targetPosition, float speed)
    {
        if (enemy == null)
            return;

        enemy.SetBehaviour(PlatNavBehaviour.FollowTarget);
        enemy.SetTarget(null);
        enemy.Abort();
        _usingVisualChase = false;

        _manualDestination = targetPosition;
        _manualCommandFailed = !enemy.MoveTo(targetPosition, speed);
        _manualCommandActive = !_manualCommandFailed;
    }

    private bool HasCompletedManualMove()
    {
        if (!_manualCommandActive || enemy == null)
            return false;

        if (enemy.HasPath || enemy.State != PlatNavState.Idle)
            return false;

        ResetManualCommand();
        return true;
    }

    private bool UpdateWaitTimer()
    {
        if (!_isWaiting)
            return false;

        _waitTimer -= Time.deltaTime;
        if (_waitTimer > 0f)
            return false;

        _isWaiting = false;
        _waitTimer = 0f;
        return true;
    }

    private void BeginWait(float duration)
    {
        _isWaiting = duration > 0f;
        _waitTimer = duration;
        ResetManualCommand();
    }

    private void ResetWait()
    {
        _isWaiting = false;
        _waitTimer = 0f;
    }

    private void ResetManualCommand()
    {
        _manualCommandActive = false;
        _manualCommandFailed = false;
    }

    private void AdvancePatrolIndex()
    {
        if (_patrolRoute == null || _patrolRoute.Length <= 1)
        {
            _patrolIndex = 0;
            return;
        }

        _patrolIndex = GetNextBounceIndex(_patrolIndex, ref _patrolDirection, _patrolRoute.Length);
    }

    private void AdvanceSearchIndex()
    {
        _searchIndex = GetNextBounceIndex(_searchIndex, ref _searchDirection, _searchRoute.Length);
    }

    private void BuildPatrolRoute()
    {
        _patrolOrigin = transform.position;

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
            _patrolRoute = points.ToArray();
            return;
        }

        Vector2 offset = Vector2.right * patrolHalfWidth;
        _patrolRoute = new[]
        {
            _patrolOrigin - offset,
            _patrolOrigin + offset,
        };
    }

    private int GetInitialPatrolIndex()
    {
        if (_patrolRoute == null || _patrolRoute.Length == 0)
            return 0;

        return GetClosestIndex(_patrolRoute, transform.position);
    }

    private Vector2 GetPatrolReturnPoint()
    {
        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            return patrolPoints[0].position;

        return _patrolOrigin;
    }

    private void ResolvePlayerTransform()
    {
        if (_playerTransform != null)
            return;

        if (_playerContext == null)
            _playerContext = Services.Get<PlayerContext>();

        if (_playerContext != null)
            _playerTransform = _playerContext.transform;
    }

    private bool IsTraversingLink()
    {
        return enemy != null && enemy.State == PlatNavState.TraversingLink;
    }

    private bool IsPlayerNoise(GameObject source)
    {
        if (_playerTransform == null || source == null)
            return false;

        return source == _playerTransform.gameObject;
    }

    private void UpdateKnownPlayerPosition(Vector2 position)
    {
        _lastKnownPlayerPosition = position;
        _hasKnownPlayerPosition = true;
        _hasDetectedPlayer = true;
    }

    private static int GetClosestIndex(IReadOnlyList<Vector2> points, Vector2 worldPosition)
    {
        int bestIndex = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            float sqrDistance = (points[i] - worldPosition).sqrMagnitude;
            if (sqrDistance >= bestDistance)
                continue;

            bestDistance = sqrDistance;
            bestIndex = i;
        }

        return bestIndex;
    }

    private static int GetNextBounceIndex(int currentIndex, ref int direction, int length)
    {
        if (length <= 1)
            return 0;

        int nextIndex = currentIndex + direction;
        if (nextIndex >= length || nextIndex < 0)
        {
            direction *= -1;
            nextIndex = currentIndex + direction;
        }

        return Mathf.Clamp(nextIndex, 0, length - 1);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        DrawRoute(_patrolRoute, patrolColor);
        DrawRoute(_searchRoute, searchColor);

        if (_manualCommandActive)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawSphere(_manualDestination, 0.2f);
        }
    }

    private static void DrawRoute(IReadOnlyList<Vector2> route, Color color)
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
