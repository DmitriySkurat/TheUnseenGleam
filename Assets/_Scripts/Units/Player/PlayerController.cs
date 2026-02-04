using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[RequireComponent(typeof(PlayerInputReader), typeof(PlayerCollisionProbe))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [SerializeField] private ScriptableStats _stats;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private PlayerInputReader _inputReader;
    private PlayerCollisionProbe _collisionProbe;
    private Vector2 _colliderSize;
    private Vector2 _colliderOffset;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private Hsm _stateMachine;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public bool IsGrounded => _collisionProbe.IsGrounded;
    public FrameInput CurrentFrameInput => _frameInput;
    public ScriptableStats Stats => _stats;
    public Rigidbody2D Rigidbody => _rb;
    public CapsuleCollider2D Collider => _col;
    internal Vector2 ColliderSize => _colliderSize;
    internal Vector2 ColliderOffset => _colliderOffset;
    internal Vector2 FrameVelocity
    {
        get => _frameVelocity;
        set => _frameVelocity = value;
    }
    internal bool JumpToConsume
    {
        get => _jumpToConsume;
        set => _jumpToConsume = value;
    }
    internal bool BufferedJumpUsable
    {
        get => _bufferedJumpUsable;
        set => _bufferedJumpUsable = value;
    }
    internal bool EndedJumpEarly
    {
        get => _endedJumpEarly;
        set => _endedJumpEarly = value;
    }
    internal bool CoyoteUsable
    {
        get => _coyoteUsable;
        set => _coyoteUsable = value;
    }
    internal float TimeJumpWasPressed
    {
        get => _timeJumpWasPressed;
        set => _timeJumpWasPressed = value;
    }
    internal float TimeSinceStart => Time.fixedTime;
    internal bool HasBufferedJump => _bufferedJumpUsable && Time.fixedTime < _timeJumpWasPressed + _stats.JumpBuffer;
    internal bool CanUseCoyote => _coyoteUsable && !IsGrounded && Time.fixedTime < _collisionProbe.FrameLeftGrounded + _stats.CoyoteTime;
    internal void NotifyJumped() => Jumped?.Invoke();

    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _inputReader = GetComponent<PlayerInputReader>();
        _collisionProbe = GetComponent<PlayerCollisionProbe>();
        _colliderSize = _col.size;
        _colliderOffset = _col.offset;

        InitializeStateMachine();
        _collisionProbe.GroundedChanged += OnGroundedChangedInternal;
    }

    private void OnDestroy()
    {
        if (_collisionProbe == null) return;
        _collisionProbe.GroundedChanged -= OnGroundedChangedInternal;
    }

    private void Update()
    {
        _frameInput = _inputReader.ReadFrameInput(_stats);

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = Time.fixedTime;
        }

        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        var probe = _collisionProbe.Probe(_stats, _col, _frameVelocity.y);
        if (probe.CeilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        _stateMachine.FixedUpdate();
    }

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    #endregion

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif

    private void InitializeStateMachine()
    {
        _stateMachine = new Hsm();

        var grounded = new PlayerGroundedState(this, _stateMachine);
        var airborne = new PlayerAirborneState(this, _stateMachine);

        grounded.SetAirborneState(airborne);
        airborne.SetGroundedState(grounded);

        _stateMachine.Initialize(grounded);
    }

    internal void SetCrouchCollider(bool isCrouching)
    {
        if (!isCrouching)
        {
            _col.size = _colliderSize;
            _col.offset = _colliderOffset;
            return;
        }

        float heightPercent = Mathf.Clamp(_stats.CrouchHeightPercent, 0.1f, 1f);
        var newSize = new Vector2(_colliderSize.x, _colliderSize.y * heightPercent);
        var heightDelta = newSize.y - _colliderSize.y;
        var newOffset = new Vector2(_colliderOffset.x, _colliderOffset.y + heightDelta * 0.5f);

        _col.size = newSize;
        _col.offset = newOffset;
    }

    private void OnGroundedChangedInternal(bool grounded, float impact)
    {
        if (grounded)
        {
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
        }

        GroundedChanged?.Invoke(grounded, impact);
    }
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public bool RunHeld;
    public bool CrouchHeld;
    public Vector2 Move;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public Vector2 FrameInput { get; }
}
