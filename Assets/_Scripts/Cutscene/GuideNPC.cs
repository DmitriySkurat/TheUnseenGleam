using PlatNav;
using UnityEngine;

/// <summary>
/// NPC-проводник: бежит к игроку → ждёт диалога → идёт к точке назначения.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlatNavHandler))]
public class GuideNPC : Interactable
{
    [Header("Guide")]
    [SerializeField] private float _chaseSpeed = 5f;
    [SerializeField] private float _walkSpeed  = 3f;
    [Tooltip("Точка у подсолнухов, к которой NPC идёт после диалога")]
    [SerializeField] private Transform _destination;

    [Header("Dialog")]
    [SerializeField] private DialogData _dialogData;
    [SerializeField] private string _npcName = "Villager";

    private PlatNavHandler _nav;
    private Animator _anim;
    private PlayerContext _playerCtx;
    private DialogManager _dialogManager;
    private bool _wasTraversing;

    private enum Phase { Chase, InDialog, Walk, Done }
    private Phase _phase;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void Initialize()
    {
        base.Initialize();
        _nav  = GetComponent<PlatNavHandler>();
        _anim = GetComponentInChildren<Animator>();

        if (Services.IsRegistered<PlayerContext>())
        {
            _playerCtx = Services.Get<PlayerContext>();
            _nav.SetTarget(_playerCtx.transform);
            _nav.SetSpeed(_chaseSpeed);
        }

        _phase = Phase.Chase;
        _anim?.Play(AgentAnimations.Run, 0, 0f);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    void FixedUpdate()
    {
        HandleTraversalAnimation();

        switch (_phase)
        {
            case Phase.Chase:    TickChase(Time.fixedDeltaTime);    break;
            case Phase.InDialog: TickInDialog();                    break;
            case Phase.Walk:     TickWalk(Time.fixedDeltaTime);     break;
        }
    }

    void HandleTraversalAnimation()
    {
        bool traversing = _nav.State == PlatNavState.TraversingLink;
        if (traversing == _wasTraversing) return;

        int idleOrMove = _phase == Phase.Walk ? AgentAnimations.Walk : AgentAnimations.Run;
        int anim = traversing
            ? (_nav.IsTraversingFall ? AgentAnimations.Dropdown : AgentAnimations.Jump)
            : idleOrMove;
        _anim?.Play(anim, 0, 0f);
        _wasTraversing = traversing;
    }

    void TickChase(float deltaTime)
    {
        _nav.Tick(deltaTime);
    }

    void TickInDialog()
    {
        // Разворачиваемся к игроку пока идёт диалог
        if (_playerCtx?.transform != null)
        {
            float dir = _playerCtx.transform.position.x - transform.position.x;
            Vector3 s = transform.localScale;
            if (Mathf.Abs(dir) > 0.05f)
                s.x = dir > 0f ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (_playerCtx != null && !_playerCtx.isInDialog)
            BeginWalk();
    }

    void BeginWalk()
    {
        _phase = Phase.Walk;
        if (_destination != null)
            _nav.MoveTo(_destination.position, _walkSpeed);
        _anim?.Play(AgentAnimations.Walk, 0, 0f);
    }

    void TickWalk(float deltaTime)
    {
        if (_nav.State == PlatNavState.Idle)
        {
            if (_destination != null &&
                Vector2.Distance(transform.position, _destination.position) > 0.5f)
            {
                _nav.MoveTo(_destination.position, _walkSpeed); // пересчёт после прыжка
            }
            else
            {
                _phase = Phase.Done;
                _anim?.Play(AgentAnimations.Idle, 0, 0f);
                return;
            }
        }

        _nav.Tick(deltaTime);
    }

    // ── Interaction ──────────────────────────────────────────────────────────

    public override bool CanBeInteractedBy(Interactor interactor)
    {
        if (_phase != Phase.Chase) return false;
        return base.CanBeInteractedBy(interactor);
    }

    public override void OnInteract(Interactor interactor)
    {
        if (_phase != Phase.Chase) return;
        if (interactor is not PlayerInteractor) return;

        if (_dialogManager == null && Services.IsRegistered<DialogManager>())
            _dialogManager = Services.Get<DialogManager>();

        if (_dialogManager == null) return;

        _nav.SetTarget(null);
        _nav.Abort();
        _anim?.Play(AgentAnimations.Idle, 0, 0f);

        _dialogManager.StartDialog(_dialogData, _npcName);
        _phase = Phase.InDialog;
    }
}
