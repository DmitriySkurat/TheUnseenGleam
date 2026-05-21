using PlatNav;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlatNavHandler))]
public class WanderingNPC : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Wander")]
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _waitTime = 1.5f;
    [SerializeField] private Transform _spriteTransform;

    [Header("Flee")]
    [SerializeField] private float _fleeSpeed = 5f;
    [Tooltip("Точка, к которой NPC убегает при панике. Если не задана — бежит к дальнейшей точке блуждания.")]
    [SerializeField] private Transform _fleeTarget;

    private PlatNavHandler _nav;
    private Rigidbody2D _rb;
    private Animator _anim;

    private int _waypointIndex;
    private float _waitTimer;
    private bool _navigating;
    private bool _fleeing;
    private Vector2 _fleeFrom;
    private Vector2 _fleeDestination;

    public bool IsGrabbed { get; private set; }

    public void Initialize()
    {
        _nav  = GetComponent<PlatNavHandler>();
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        if (_spriteTransform == null)
            _spriteTransform = _anim != null ? _anim.transform : transform;

        _waypointIndex = 0;
        NavigateToCurrentWaypoint();
    }

    // Флипает спрайт-чайлд. Вызывать только когда root не управляется PlatNav
    // (т.е. после Grab(), который сбрасывает root.x = 1).
    public void Flip(float dirX)
    {
        if (_spriteTransform == null || Mathf.Abs(dirX) < 0.01f) return;
        Vector3 s = _spriteTransform.localScale;
        s.x = dirX > 0f ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        _spriteTransform.localScale = s;
    }

    public void Dispose() { }

    public void StartFleeing(Vector2 threatPos)
    {
        if (IsGrabbed || _fleeing) return;
        _fleeing  = true;
        _fleeFrom = threatPos;
        _waitTimer = 0f;
        _nav.Abort();
        NavigateToFleeTarget();
    }

    void FixedUpdate()
    {
        if (IsGrabbed) return;
        if (_waypoints == null || _waypoints.Length == 0) return;

        if (_fleeing)
            TickFlee(Time.fixedDeltaTime);
        else
            Tick(Time.fixedDeltaTime);
    }

    void Tick(float deltaTime)
    {
        if (_waitTimer > 0f)
        {
            _waitTimer -= deltaTime;
            if (_waitTimer <= 0f)
                AdvanceWaypoint();
            return;
        }

        if (_navigating)
        {
            _nav.Tick(deltaTime);
            if (_nav.State == PlatNavState.Idle)
            {
                _navigating = false;
                _waitTimer = _waitTime;
                _anim?.Play(AgentAnimations.Idle, 0, 0f);
            }
        }
        else
        {
            NavigateToCurrentWaypoint();
        }
    }

    void TickFlee(float deltaTime)
    {
        if (_nav.State == PlatNavState.Idle)
        {
            if (Vector2.Distance(transform.position, _fleeDestination) > 0.5f)
                _nav.MoveTo(_fleeDestination, _fleeSpeed);
            else
                _anim?.Play(AgentAnimations.Idle, 0, 0f);
        }

        _nav.Tick(deltaTime);
    }

    void NavigateToFleeTarget()
    {
        if (_fleeTarget != null)
        {
            _fleeDestination = _fleeTarget.position;
        }
        else
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            float maxDist = -1f;
            int best = _waypointIndex;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null) continue;
                float d = Vector2.Distance(_fleeFrom, _waypoints[i].position);
                if (d > maxDist) { maxDist = d; best = i; }
            }
            _waypointIndex = best;
            _fleeDestination = _waypoints[_waypointIndex].position;
        }

        bool started = _nav.MoveTo(_fleeDestination, _fleeSpeed);
        _anim?.Play(started ? AgentAnimations.Walk : AgentAnimations.Idle, 0, 0f);
    }

    void NavigateToCurrentWaypoint()
    {
        if (_waypoints == null || _waypoints.Length == 0) return;
        _navigating = _nav.MoveTo(_waypoints[_waypointIndex].position, _walkSpeed);
        if (_navigating)
            _anim?.Play(AgentAnimations.Walk, 0, 0f);
    }

    void AdvanceWaypoint()
    {
        _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
        NavigateToCurrentWaypoint();
    }

    public void Grab()
    {
        IsGrabbed = true;
        _nav.Abort();
        _navigating = false;
        _waitTimer = 0f;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _anim?.Play(AgentAnimations.Idle, 0, 0f);

        // Сбрасываем root.x в 1, чтобы Flip() на sprite child стал единственным
        // источником поворота (PlatNav больше не управляет этим объектом).
        Vector3 rs = transform.localScale;
        rs.x = Mathf.Abs(rs.x);
        transform.localScale = rs;
    }

    public void Release()
    {
        IsGrabbed = false;
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_waypoints != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.6f);
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null) continue;
                Gizmos.DrawSphere(_waypoints[i].position, 0.15f);
                int next = (i + 1) % _waypoints.Length;
                if (_waypoints[next] != null)
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[next].position);
            }
        }

        if (_fleeTarget != null)
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
            Gizmos.DrawSphere(_fleeTarget.position, 0.25f);
            Gizmos.DrawLine(transform.position, _fleeTarget.position);
        }
    }
#endif
}
