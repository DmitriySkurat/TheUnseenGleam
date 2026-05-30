using PlatNav;
using UnityEngine;

/// <summary>
/// Cutscene-only agent: runs to a target (WanderingNPC or Player), grabs it, carries it to an exit point.
/// Uses PlatNav for pathfinding — supports jumping over obstacles.
/// Multiple NPC-targeting instances automatically avoid grabbing the same NPC.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PlatNavHandler))]
public class ScriptedAgent : MonoBehaviour, ISceneLifecycle
{
    public InitializationOrder Order => InitializationOrder.Enemy;

    [Header("Movement")]
    [SerializeField] private float _runSpeed = 5f;

    [Header("Grab")]
    [SerializeField] private float _grabRadius = 1.2f;
    [SerializeField] private float _grabOffset = 0.6f;

    [Header("Target")]
    [Tooltip("Если true — агент хватает игрока, иначе — ближайшего WanderingNPC")]
    [SerializeField] private bool _targetPlayer = false;

    [Header("Exit")]
    [SerializeField] private Transform _exitPoint;

    [Header("Visuals")]
    [SerializeField] private Transform _spriteTransform;

    private PlatNavHandler _nav;
    private Rigidbody2D _rb;
    private Animator _anim;

    // NPC target
    private WanderingNPC _target;

    // Player target
    private PlayerContext _playerCtx;
    private Rigidbody2D  _playerRb;

    private bool _wasTraversing;
    private PlatNavState _prevNavState;

    private enum Phase { FindTarget, Chase, Carry, Done }
    private Phase _phase;

    public void Initialize()
    {
        _nav  = GetComponent<PlatNavHandler>();
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        if (_spriteTransform == null)
            _spriteTransform = _anim != null ? _anim.transform : transform;

        if (_targetPlayer && Services.IsRegistered<PlayerContext>())
        {
            _playerCtx = Services.Get<PlayerContext>();
            _playerRb  = _playerCtx?.rb;
        }

        _phase = Phase.FindTarget;
        _anim?.Play(AgentAnimations.Walk, 0, 0f);
    }

    public void Dispose() { }

    void Flip(float dirX)
    {
        if (_spriteTransform == null || Mathf.Abs(dirX) < 0.01f) return;
        Vector3 s = _spriteTransform.localScale;
        s.x = dirX > 0f ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
        _spriteTransform.localScale = s;
    }

    void FixedUpdate()
    {
        UpdateTraversalAnimation();

        switch (_phase)
        {
            case Phase.FindTarget: TickFindTarget(); break;
            case Phase.Chase:      TickChase(Time.fixedDeltaTime); break;
            case Phase.Carry:      TickCarry(Time.fixedDeltaTime); break;
        }
    }

    void UpdateTraversalAnimation()
    {
        var navState = _nav.State;
        bool traversing = navState == PlatNavState.TraversingLink;

        if (traversing != _wasTraversing)
        {
            int anim = traversing
                ? (_nav.IsTraversingFall ? AgentAnimations.Dropdown : AgentAnimations.Jump)
                : AgentAnimations.Walk;
            _anim?.Play(anim, 0, 0f);
            _wasTraversing = traversing;
        }
        else if (!traversing && _prevNavState == PlatNavState.Idle
                 && navState == PlatNavState.WalkingSegment)
        {
            // Nav resumed walking after a brief Idle — restore movement animation
            _anim?.Play(AgentAnimations.Walk, 0, 0f);
        }

        _prevNavState = navState;
    }

    // ── FindTarget ──────────────────────────────────────────────────────────

    void TickFindTarget()
    {
        if (_targetPlayer)
        {
            if (_playerCtx == null || !_playerCtx.isAlive) return;
            // Ждём, пока игрок войдёт в Stumble (можно атаковать всегда — на усмотрение дизайнера)
            _phase = Phase.Chase;
            _anim?.Play(AgentAnimations.Walk, 0, 0f);
            _nav.SetSpeed(_runSpeed);
            _nav.SetTarget(_playerCtx.transform);
        }
        else
        {
            _target = FindNearestFreeNPC();
            if (_target == null) return;

            _phase = Phase.Chase;
            _anim?.Play(AgentAnimations.Walk, 0, 0f);
            _nav.SetSpeed(_runSpeed);
            _nav.SetTarget(_target.transform);
        }
    }

    // ── Chase ────────────────────────────────────────────────────────────────

    void TickChase(float deltaTime)
    {
        if (_targetPlayer)
            TickChasePlayer(deltaTime);
        else
            TickChaseNPC(deltaTime);
    }

    void TickChaseNPC(float deltaTime)
    {
        if (_target == null || _target.IsGrabbed)
        {
            _nav.SetTarget(null);
            _nav.Abort();
            _target = null;
            _phase  = Phase.FindTarget;
            return;
        }

        _nav.Tick(deltaTime);

        float dist = Vector2.Distance(transform.position, _target.transform.position);
        if (dist <= _grabRadius && _nav.State != PlatNavState.TraversingLink)
            BeginGrabNPC();
    }

    void TickChasePlayer(float deltaTime)
    {
        if (_playerCtx == null || !_playerCtx.isAlive || _playerCtx.isGrabbed)
        {
            _nav.SetTarget(null);
            _nav.Abort();
            _phase = Phase.Done;
            return;
        }

        _nav.Tick(deltaTime);

        float dist = Vector2.Distance(transform.position, _playerCtx.transform.position);
        if (dist <= _grabRadius && _nav.State != PlatNavState.TraversingLink)
            BeginGrabPlayer();
    }

    // ── Carry ────────────────────────────────────────────────────────────────

    void TickCarry(float deltaTime)
    {
        if (_nav.State == PlatNavState.Idle)
        {
            if (_exitPoint == null ||
                Vector2.Distance(transform.position, _exitPoint.position) <= 0.5f)
            {
                _phase = Phase.Done;
                return;
            }
            if (_nav.MoveTo(_exitPoint.position, _runSpeed))
                _anim?.Play(AgentAnimations.Walk, 0, 0f);
        }

        _nav.Tick(deltaTime);

        if (_targetPlayer)
            KeepPlayerAttached();
        else
            KeepNPCAttached();
    }

    // ── Grab ─────────────────────────────────────────────────────────────────

    void BeginGrabNPC()
    {
        _nav.SetTarget(null);
        _nav.Abort();

        _target.Grab();
        _phase = Phase.Carry;

        bool willMove = _exitPoint != null && _nav.MoveTo(_exitPoint.position, _runSpeed);
        _anim?.Play(willMove ? AgentAnimations.Walk : AgentAnimations.GrabPlayer, 0, 0f);
    }

    void BeginGrabPlayer()
    {
        _nav.SetTarget(null);
        _nav.Abort();

        _playerCtx.velocity            = Vector2.zero;
        _playerRb.linearVelocity       = Vector2.zero;
        _playerCtx.isGrabbed           = true;
        if (Services.IsRegistered<DialogManager>())
            Services.Get<DialogManager>().Cancel();
        _playerCtx.grabEscapeCount     = 10; // значение не важно — grabEscapeDisabled блокирует побег

        _phase = Phase.Carry;

        bool willMove = _exitPoint != null && _nav.MoveTo(_exitPoint.position, _runSpeed);
        _anim?.Play(willMove ? AgentAnimations.Walk : AgentAnimations.GrabPlayer, 0, 0f);
    }

    // ── Attach ───────────────────────────────────────────────────────────────

    void KeepNPCAttached()
    {
        if (_target == null) return;

        float facingDir = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 grabPos = (Vector2)transform.position + Vector2.right * (facingDir * _grabOffset);
        _target.GetComponent<Rigidbody2D>().position = grabPos;
        _target.Flip(-facingDir);
    }

    void KeepPlayerAttached()
    {
        if (_playerCtx == null || _playerRb == null) return;

        float facingDir = transform.localScale.x >= 0f ? 1f : -1f;
        Vector2 grabPos = (Vector2)transform.position + Vector2.right * (facingDir * _grabOffset);
        _playerRb.position     = grabPos;
        _playerCtx.velocity    = Vector2.zero;
    }

    // ── NPC search ───────────────────────────────────────────────────────────

    WanderingNPC FindNearestFreeNPC()
    {
        var all = Object.FindObjectsByType<WanderingNPC>(FindObjectsSortMode.None);
        WanderingNPC best = null;
        float bestDist = float.MaxValue;

        foreach (var npc in all)
        {
            if (npc.IsGrabbed) continue;
            if (IsAlreadyTargeted(npc)) continue;
            float d = Vector2.Distance(transform.position, npc.transform.position);
            if (d < bestDist) { bestDist = d; best = npc; }
        }
        return best;
    }

    bool IsAlreadyTargeted(WanderingNPC npc)
    {
        var agents = Object.FindObjectsByType<ScriptedAgent>(FindObjectsSortMode.None);
        foreach (var a in agents)
        {
            if (a == this) continue;
            if (a._target == npc && a._phase == Phase.Chase)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = _targetPlayer ? Color.cyan : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _grabRadius);
        if (_exitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _exitPoint.position);
            Gizmos.DrawSphere(_exitPoint.position, 0.2f);
        }
    }
#endif
}
