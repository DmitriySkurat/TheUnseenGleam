using System.Linq;
using UnityEngine;

namespace HSM {
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PlayerCollisionProbe2D))]
    public class PlayerStateDriver : MonoBehaviour {
        [SerializeField] PlayerContext2D ctx = new PlayerContext2D();
        string lastPath;

        Rigidbody2D rb;
        CapsuleCollider2D col;
        PlayerCollisionProbe2D collisionProbe;
        StateMachine machine;
        State root;

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CapsuleCollider2D>();
            collisionProbe = GetComponent<PlayerCollisionProbe2D>();
            if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            ctx.rb = rb;
            ctx.col = col;
            ctx.collisionProbe = collisionProbe;
            if (col != null) {
                ctx.colliderSize = col.size;
                ctx.colliderOffset = col.offset;
            }

            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();

            if (collisionProbe != null) collisionProbe.GroundedChanged += OnGroundedChangedInternal;
        }

        void Update() {
            if (ctx.stats == null) return;

            var frameInput = new PlayerFrameInput {
                JumpDown = InputManager.JumpWasPressed,
                JumpHeld = InputManager.JumpIsHeld,
                RunHeld = InputManager.RunIsHeld,
                CrouchHeld = InputManager.CrouchIsHeld,
                Move = InputManager.Movement
            };

            if (ctx.stats.SnapInput) {
                frameInput.Move.x =
                    Mathf.Abs(frameInput.Move.x) < ctx.stats.HorizontalDeadZoneThreshold
                        ? 0
                        : Mathf.Sign(frameInput.Move.x);

                frameInput.Move.y =
                    Mathf.Abs(frameInput.Move.y) < ctx.stats.VerticalDeadZoneThreshold
                        ? 0
                        : Mathf.Sign(frameInput.Move.y);
            }

            ctx.frameInput = frameInput;

            if (frameInput.JumpDown) {
                ctx.jumpToConsume = true;
                ctx.timeJumpWasPressed = Time.fixedTime;
            }
        }

        void FixedUpdate() {
            if (ctx.stats == null || collisionProbe == null || col == null) return;

            var rbVelocity = rb.linearVelocity;
            var probe = collisionProbe.Probe(ctx.stats, col, rbVelocity.y);
            if (probe.CeilingHit) {
                rbVelocity = new Vector2(rbVelocity.x, Mathf.Min(0, rbVelocity.y));
                rb.linearVelocity = rbVelocity;
            }

            machine.Tick(Time.fixedDeltaTime);

#if UNITY_EDITOR
            var path = StatePath(machine.Root.Leaf());
            if (path != lastPath) {
                Debug.Log($"State: {path}");
                lastPath = path;
            }
#endif
        }

        void OnDestroy() {
            if (collisionProbe != null) collisionProbe.GroundedChanged -= OnGroundedChangedInternal;
        }

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }

        void OnGroundedChangedInternal(bool grounded, float impact) {
            if (grounded) {
                ctx.coyoteUsable = true;
                ctx.bufferedJumpUsable = true;
                ctx.endedJumpEarly = false;
            }
        }
    }
}
