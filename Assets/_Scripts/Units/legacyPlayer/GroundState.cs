using UnityEngine;

namespace newFSM 
{
    public class GroundState : State
    {
        public AnimationClip anim;
        
        public override void Enter()
        {
            animator.Play(anim.name);
        }

        public override void Do()
        {
            
        }

        public override void FixedDo()
        {
            
        }

        public override void Exit()
        {
            
        }
    }
}