using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace HSM
{
    [Serializable]
    public class PlayerContext {
        
        public FrameInput input;
        public ScriptableStats stats;
        
        
        public bool jumpToConsume;
        public float timeInteractWasPressed;
        
        
        
        public bool isCrouching;
        
        
        // for Complex interactables
        public bool isInteracting;
        
        
        
        public bool ceilingAbove;

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
        public float timeLastInteraction;
        public float time;
        public Animator anim;
        public Rigidbody2D rb;
        public Renderer renderer;
        public Collider2D coll;
        public AudioSource audio;
        
        

        public bool HasBufferedJump => bufferedJumpUsable && stats != null && time < timeJumpWasPressed + stats.JumpBuffer;
        public bool CanUseCoyote => coyoteUsable && !grounded && stats != null && time < frameLeftGrounded + stats.CoyoteTime;
        
        public bool CanInteract => time > timeLastInteraction + stats.InteractionCooldown;
    }
}
