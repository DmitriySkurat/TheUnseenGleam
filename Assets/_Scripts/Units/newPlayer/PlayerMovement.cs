using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace newFSM 
{
    public class PlayerMovement : MonoBehaviour {

        public AirState airState;
        public GroundState groundState;
        public IdleState idleState;
        public RunState runState;
        public KneelState kneelState;
        
        State state;
        
        public Animator animator;

        //scene instanced objects
        public Rigidbody2D body;
        public BoxCollider2D groundCheck;
        public LayerMask groundMask;

        //movement properties
        public float acceleration;
        [Range(0f, 1f)]
        public float groundDecay;
        public float maxXSpeed;


        //variables
        public bool grounded { get; private set; }
        public float xInput { get; private set; }
        public float yInput { get; private set; }

        void Start() {
            idleState.Setup(body, animator, this);
            runState.Setup(body, animator, this);
            airState.Setup(body, animator, this);
            kneelState.Setup(body, animator, this);
        
            state = idleState;
        }

        // Update is called once per frame
        void Update() {
            CheckInput();
            HandleJumpInput();
            
            
            SelectState();
            
            state.Do();
        }

        void FixedUpdate() {
            CheckGround();
            HandleXMovement();
            ApplyFriction();
        }
        
        void SelectState()
        {
            State oldState = state;
            
            if (grounded)
            {
                if (yInput < 0 && Mathf.Abs(xInput) < 0.1f)
                {
                    state = kneelState;
                }
                else if (yInput == 0)
                {
                    state = idleState;
                }
                else
                {
                    state = runState;
                }
            }
            else
            {
                state = airState;
            }
            
            if (oldState != state || oldState.isComplete)
            {
                oldState.Exit();
                state.Initialize();
                state.Enter();
            }
        }

        void CheckInput() {
            xInput = Input.GetAxis("Horizontal");
            yInput = Input.GetAxis("Vertical");
        }

        void HandleXMovement() {
            if (Mathf.Abs(xInput) > 0) {

                //increment linearVelocity by our accelleration, then clamp within max
                float increment = xInput * acceleration;
                float newSpeed = Mathf.Clamp(body.linearVelocity.x + increment, -maxXSpeed, maxXSpeed);
                body.linearVelocity = new Vector2(newSpeed, body.linearVelocity.y);

                FaceInput();
            }
        }

        void FaceInput() {
            float direction = Mathf.Sign(xInput);
            transform.localScale = new Vector3(direction, 1, 1);
        }

        void HandleJumpInput() {
            if (Input.GetButtonDown("Jump") && grounded) {
                body.linearVelocity = new Vector2(body.linearVelocity.x, airState.jumpSpeed);
            }
        }

        void CheckGround() {
            grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
        }

        void ApplyFriction() {
            if (grounded && xInput == 0 && body.linearVelocity.y <= 0) {
                body.linearVelocity *= groundDecay;
            }
        }

    }
}

