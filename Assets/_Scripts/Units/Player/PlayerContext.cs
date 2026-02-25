using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace HSM
{
    [Serializable]
    public class PlayerContext {
        
        // connected to buttons
        public bool jumpHeld;
        public bool jumpPressed;
        public bool jumpToConsume;
        public bool crouchHeld;
        public bool runHeld;
        
        // connected to states
        public bool isCrouching;
        public bool isInteracting;
        
        public bool ceilingAbove;

        public Vector2 move;
        public Vector2 velocity;
        public bool grounded;
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float jumpSpeed = 7f;
        public bool bufferedJumpUsable;
        public bool endedJumpEarly;
        public bool coyoteUsable;
        public float timeJumpWasPressed;
        public float frameLeftGrounded = float.MinValue;
        public float time;
        public Animator anim;
        public Rigidbody2D rb;
        public Renderer renderer;
        public Collider2D coll;
        public AudioSource audio;
        public ScriptableStats stats;
        
        

        public bool HasBufferedJump => bufferedJumpUsable && stats != null && time < timeJumpWasPressed + stats.JumpBuffer;
        public bool CanUseCoyote => coyoteUsable && !grounded && stats != null && time < frameLeftGrounded + stats.CoyoteTime;
    }
}
