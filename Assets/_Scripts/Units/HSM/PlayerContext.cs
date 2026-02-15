using UnityEngine;
using System;

namespace HSM 
{
    [Serializable]
    public class PlayerContext {
        public Animator anim;
        public Rigidbody2D rb;
        public Renderer renderer;
        
        public bool isGrounded;
    }
}