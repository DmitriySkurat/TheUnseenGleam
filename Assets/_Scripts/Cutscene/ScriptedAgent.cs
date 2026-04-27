using PlatNav;
using UnityEngine;

/// <summary>
/// Cutscene-only agent: runs to the nearest free WanderingNPC, grabs it, then carries it to an exit point.
/// Uses PlatNav for pathfinding — supports jumping over obstacles.
/// Multiple instances automatically avoid grabbing the same NPC.
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

    [Header("Exit")]
    [SerializeField] private Transform _exitPoint;

    [Header("Visuals")]
    [SerializeField] private Transform _spriteTransform;

    private PlatNavHandler _nav;
    private Rigidbody2D _rb;
    private Animator _anim;

    private WanderingNPC _target;
    private bool _wasTraversing;

    private enum Phase { FindTarget, Chase, Carry, Done }
    private Phase _phase;

    public void Initialize()
    {
        _nav  = GetComponent<PlatNavHandler>();
        _rb   = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        if (_spriteTransform == null)
            _spriteTransform = _anim != null ? _anim.transform : transform;

        _phase = Phase.FindTarget;
    }

    public void Dispose() { }

    // Флипает только sprite child — root трогать не нужно (им управляет PlatNav).
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
        bool traversing = _nav.State == PlatNavState.TraversingLink;
        if (traversing == _wasTraversing) return;

        int anim = traversing
            ? (_nav.IsTraversingFall ? AgentAnimations.Dropdown : AgentAnimations.Jump)
            : (_phase == Phase.Carry ? AgentAnimations.GrabPlayer : AgentAnimations.Run);
        _anim?.Play(anim, 0, 0f);
        _wasTraversing = traversing;
    }

    void TickFindTarget()
    {
        _target = FindNearestFreeNPC();
        if (_target == null) return;

        _phase = Phase.Chase;
        _anim?.Play(AgentAnimations.Run, 0, 0f);
        _nav.SetSpeed(_runSpeed);
        _nav.SetTarget(_target.transform);
    }

    void TickChase(float deltaTime)
    {
        if (_target == null || _target.IsGrabbed)
        {
            _nav.SetTarget(null);
            _nav.Abort();
            _target = null;
            _phase = Phase.FindTarget;
            return;
        }

        _nav.Tick(deltaTime);

        float dist = Vector2.Distance(transform.position, _target.transform.position);
        if (dist <= _grabRadius && _nav.State != PlatNavState.TraversingLink)
            BeginGrab();
    }

    void TickCarry(float deltaTime)
    {
        if (_nav.State == PlatNavState.Idle)
        {
            // PlatNav путь может закончиться на точке приземления после прыжка,
            // не дойдя до exitPoint внутри целевого сегмента. Переиздаём MoveTo.
            if (_exitPoint == null ||
                Vector2.Distance(transform.position, _exitPoint.position) <= 0.5f)
            {
                _phase = Phase.Done;
                return;
            }
            _nav.MoveTo(_exitPoint.position, _runSpeed);
        }

        _nav.Tick(deltaTime);
        KeepNPCAttached();
    }

    void BeginGrab()
    {
        _nav.SetTarget(null);
        _nav.Abort();

        _target.Grab();
        _phase = Phase.Carry;
        _anim?.Play(AgentAnimations.GrabPlayer, 0, 0f);

        if (_exitPoint != null)
            _nav.MoveTo(_exitPoint.position, _runSpeed);
    }

    void KeepNPCAttached()
    {
        if (_target == null) return;

        // Facing direction читается из root — им управляет PlatNav.
        float facingDir = transform.localScale.x >= 0f ? 1f : -1f;

        Vector2 grabPos = (Vector2)transform.position + Vector2.right * (facingDir * _grabOffset);
        _target.GetComponent<Rigidbody2D>().position = grabPos;

        // NPC смотрит в сторону, противоположную агенту (его тащат).
        // WanderingNPC.Grab() уже сбросил root.x = 1, поэтому Flip работает чисто.
        _target.Flip(-facingDir);
    }

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
        Gizmos.color = Color.yellow;
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
