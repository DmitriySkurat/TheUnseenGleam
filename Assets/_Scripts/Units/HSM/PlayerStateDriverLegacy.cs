using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HSM {
    public class PlayerStateDriver : MonoBehaviour {
        public PlayerContext ctx = new PlayerContext();
        public Transform groundCheck;
        public float groundRadius = 0.2f;
        public LayerMask groundMask;
        public bool drawGizmos = true;
        string lastPath;

        Rigidbody2D rb;
        StateMachine machine;
        State root;

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
            // rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            ctx.rb = rb;
            ctx.anim = GetComponentInChildren<Animator>();
            ctx.renderer = GetComponent<Renderer>();

            root = new PlayerRoot(null, ctx);
            var builder = new StateMachineBuilder(root);
            machine = builder.Build();

            // fallback: create a groundCheck just below the collider's bounds
            if (groundCheck == null) {
                var col = GetComponent<Collider2D>();
                var t = new GameObject("groundCheck").transform;
                t.SetParent(transform, false);
                var y = col ? (-col.bounds.extents.y + 0.01f) : -0.5f;
                t.localPosition = new Vector3(0, y, 0);
                groundCheck = t;
            }
        }

        void Update() {
            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            ctx.jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            ctx.move.x = Mathf.Clamp(x, -1f, 1f);

            ctx.grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);

            machine.Tick(Time.deltaTime);

            var path = StatePath(machine.Root.Leaf());

            if (path != lastPath) {
                lastPath = path;
            }
        }

        void FixedUpdate() {
            var v = rb.linearVelocity;
            v.x = ctx.velocity.x;
            rb.linearVelocity = v;

            ctx.velocity.x = rb.linearVelocity.x;
        }

        void OnDrawGizmosSelected() {
            if (!drawGizmos || groundCheck == null) return;

            Gizmos.color = ctx.grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        static string StatePath(State s) {
            return string.Join(" > ", s.PathToRoot().Reverse().Select(n => n.GetType().Name));
        }
    }

    [Serializable]
    public class PlayerContext {
        public Vector3 move;
        public Vector3 velocity;
        public bool grounded;
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float jumpSpeed = 7f;
        public bool jumpPressed;
        public Animator anim;
        public Rigidbody2D rb;
        public Renderer renderer;
    }
}