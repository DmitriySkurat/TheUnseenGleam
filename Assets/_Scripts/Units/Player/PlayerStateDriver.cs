using System.Linq;
using UnityEngine;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerStateDriver : MonoBehaviour {
        public PlayerContext ctx = new PlayerContext();

        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private InputManager _inputManager;
        
        private PlayerInteractor _interactor; 
        

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private bool _cachedQueryStartInColliders;

        private StateMachine _machine;
        private PlayerRoot _root;

        private string _lastPath;

        void Awake() {
            _rb = gameObject.GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (_inputManager == null) _inputManager = GetComponent<InputManager>();
            
            _interactor = new PlayerInteractor(ctx);

            ctx.rb = _rb;
            ctx.anim = GetComponentInChildren<Animator>();
            ctx.renderer = GetComponentInChildren<Renderer>();
            ctx.coll = _col;
            ctx.audio = GetComponent<AudioSource>();
            ctx.stats = _stats;
            

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
            _inputManager.OnCrouchToggled += HandleCrouchInput;
            _inputManager.OnRunToggled += HandleRunInput;
            _inputManager.OnInteractToggled += HandleInteractInput;
        }

        void OnDisable() {
            if (_inputManager == null) return;

            _inputManager.OnMove -= HandleMoveInput;
            _inputManager.OnJumpStarted -= HandleJumpStarted;
            _inputManager.OnJumpCanceled -= HandleJumpCanceled;
            _inputManager.OnCrouchToggled -= HandleCrouchInput;
            _inputManager.OnRunToggled -= HandleRunInput;
            _inputManager.OnInteractToggled -= HandleInteractInput;
        }

        void FixedUpdate() {
            if (_stats == null || _col == null) return;

            ctx.time = Time.time;
            ctx.move = _frameInput.Move;
            ctx.jumpPressed = _frameInput.JumpDown;
            ctx.jumpHeld = _frameInput.JumpHeld;
            ctx.crouchHeld = _frameInput.CrouchHeld;
            ctx.runHeld = _frameInput.RunHeld;
            ctx.isInteracting = _frameInput.InteractDown;

            CheckCollisions();
            _machine.Tick(Time.fixedDeltaTime);
            ApplyMovement();

            _frameInput.JumpDown = false;


            // for debbing, print current state path when it changes
            var path = StatePath(_machine.Root.Leaf());
            if (path != _lastPath) {
                Debug.Log("State: " + path);
                _lastPath = path;
            }
        }

        void CheckCollisions() {
            Physics2D.queriesStartInColliders = false;

            bool wasGrounded = ctx.grounded;
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            if (ceilingHit) ctx.velocity.y = Mathf.Min(0, ctx.velocity.y);

            if (!wasGrounded && groundHit) {
                ctx.grounded = true;
                ctx.coyoteUsable = true;
                ctx.bufferedJumpUsable = true;
                ctx.endedJumpEarly = false;
            } else if (wasGrounded && !groundHit) {
                ctx.grounded = false;
                ctx.frameLeftGrounded = ctx.time;
            } else {
                ctx.grounded = groundHit;
            }
            
            if (ctx.isCrouching)
            {
                ctx.ceilingAbove = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0f, Vector2.up, _stats.CeilingCheckDistance, ~_stats.PlayerLayer);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
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

            ctx.jumpToConsume = true;
            ctx.timeJumpWasPressed = Time.time;
        }

        void HandleJumpCanceled() {
            _frameInput.JumpDown = false;
            _frameInput.JumpHeld = false;
        }
        
        void HandleCrouchInput(bool isCrouching)
        {
            _frameInput.CrouchHeld = isCrouching;
        }
        
        void HandleRunInput(bool isRunning)
        {
            _frameInput.RunHeld = isRunning;
        }
        
        void HandleInteractInput(bool isInteracting)
        {
            _frameInput.InteractDown = isInteracting;
        }

#if UNITY_EDITOR
        void OnValidate() {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player State Driver's Stats slot", this);
        }
#endif

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
        
        
        void OnDrawGizmos()
        {
            var col = GetComponent<CapsuleCollider2D>();
            if (col == null || _stats == null) return;

            // Коллайдер
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);

            Gizmos.matrix = Matrix4x4.identity;
            
            // CeilingAbove
            Gizmos.color = ctx.ceilingAbove ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _stats.CeilingCheckDistance);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * _stats.CeilingCheckDistance, 0.05f);

            // Проверка вниз (groundHit)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.down * _stats.GrounderDistance);
            Gizmos.DrawWireSphere(col.bounds.center + Vector3.down * _stats.GrounderDistance, 0.05f);

            // Проверка вверх (ceilingHit)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + Vector3.up * _stats.GrounderDistance);
            Gizmos.DrawWireSphere(col.bounds.center + Vector3.up * _stats.GrounderDistance, 0.05f);
        }
        
    }   
}
