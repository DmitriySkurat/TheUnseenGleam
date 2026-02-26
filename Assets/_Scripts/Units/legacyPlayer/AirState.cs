using UnityEngine;

namespace newFSM 
{
    public class AirState : State
    {
        public float jumpSpeed;
        public AnimationClip anim;
        
        public override void Enter()
        {
            animator.Play(anim.name);
        }
        
        public override void Do()
        {
            float time = Helpers.Map(body.linearVelocity.y, jumpSpeed, -jumpSpeed, 0, 1, true);
            animator.Play("Jump", 0, time);
            animator.speed = 0;
            
            if (input.grounded)
            {
                isComplete = true;
            }
        }

        public override void FixedDo()
        {
            
        }

        public override void Exit()
        {
            
        }
    }
}