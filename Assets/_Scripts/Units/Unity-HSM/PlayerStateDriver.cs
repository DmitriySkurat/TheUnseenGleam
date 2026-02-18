using System.Linq;
using UnityEngine;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerStateDriver : MonoBehaviour {
        public PlayerContext ctx = new PlayerContext();

        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private InputManager _inputManager;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private bool _cachedQueryStartInColliders;

        private StateMachine _machine;
        private PlayerRoot _root;

        private float _time;

        private bool _grounded;
        private float _frameLeftGrounded = float.MinValue;

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private string _lastPath;

        private bool HasBufferedJump => _bufferedJumpUsable && _stats != null && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _stats != null && _time < _frameLeftGrounded + _stats.CoyoteTime;

        void Awake() {
            _rb = gameObject.GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (_inputManager == null) _inputManager = GetComponent<InputManager>();

            ctx.rb = _rb;
            ctx.anim = GetComponentInChildren<Animator>();
            ctx.renderer = GetComponent<Renderer>();

            if (_stats != null) {
                ctx.moveSpeed = _stats.MaxSpeed;
                ctx.accel = _stats.Acceleration;
                ctx.jumpSpeed = _stats.JumpPower;
            }

            ctx.velocity = _rb.linearVelocity;

            _root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(_root);
            _machine = builder.Build();
        }

        void OnEnable() {
            if (_inputManager == null) return;

            _inputManager.OnMove += HandleMoveInput;
            _inputManager.OnJumpStarted += HandleJumpStarted;
            _inputManager.OnJumpCanceled += HandleJumpCanceled;
        }

        void OnDisable() {
            if (_inputManager == null) return;

            _inputManager.OnMove -= HandleMoveInput;
            _inputManager.OnJumpStarted -= HandleJumpStarted;
            _inputManager.OnJumpCanceled -= HandleJumpCanceled;
        }

        void Update() {
            _time += Time.deltaTime;

            ctx.move = _frameInput.Move;
            ctx.jumpPressed = _frameInput.JumpDown;
            ctx.grounded = _grounded;

            _machine.Tick(Time.deltaTime);

            var path = StatePath(_machine.Root.Leaf());
            if (path != _lastPath) {
                Debug.Log("State: " + path);
                _lastPath = path;
            }
        }

        void FixedUpdate() {
            if (_stats == null || _col == null) return;

            CheckCollisions();
            HandleJump();
            HandleDirection();
            HandleGravity();
            ApplyMovement();

            _frameInput.JumpDown = false;
        }

        void CheckCollisions() {
            Physics2D.queriesStartInColliders = false;

            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            if (ceilingHit) ctx.velocity.y = Mathf.Min(0, ctx.velocity.y);

            if (!_grounded && groundHit) {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
            } else if (_grounded && !groundHit) {
                _grounded = false;
                _frameLeftGrounded = _time;
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;

            ctx.grounded = _grounded;
        }

        void HandleJump() {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.linearVelocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        void ExecuteJump() {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            ctx.velocity.y = _stats.JumpPower;
        }

        void HandleDirection() {
            if (_frameInput.Move.x == 0) {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, deceleration * Time.fixedDeltaTime);
            } else {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        void HandleGravity() {
            if (_grounded && ctx.velocity.y <= 0f) {
                ctx.velocity.y = _stats.GroundingForce;
            } else {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && ctx.velocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                ctx.velocity.y = Mathf.MoveTowards(ctx.velocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        void ApplyMovement() => _rb.linearVelocity = ctx.velocity;

        void HandleMoveInput(Vector2 direction) {
            _frameInput.Move = direction;

            if (_stats == null || !_stats.SnapInput) return;

            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
        }

        void HandleJumpStarted() {
            _frameInput.JumpDown = true;
            _frameInput.JumpHeld = true;

            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        void HandleJumpCanceled() {
            _frameInput.JumpDown = false;
            _frameInput.JumpHeld = false;
        }

#if UNITY_EDITOR
        void OnValidate() {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player State Driver's Stats slot", this);
        }
#endif

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
    }
}
