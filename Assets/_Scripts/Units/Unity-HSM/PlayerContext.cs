using UnityEngine;
using System;

namespace HSM
{
    [Serializable]
    public class PlayerContext {
        public Vector2 move;
        public Vector2 velocity;
        public bool grounded;
        public float moveSpeed = 6f;
        public float accel = 40f;
        public float jumpSpeed = 7f;
        public bool jumpPressed;
        public Animator anim;
        public Rigidbody2D rb;
        public Renderer renderer;
        public Collider2D coll;
        
    }
}