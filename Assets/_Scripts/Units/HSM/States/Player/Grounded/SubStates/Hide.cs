using UnityEngine;

namespace HSM
{
    public class Hide : State
    {
        private PlayerContext ctx;

        public Hide(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.purple,
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Climb", true, false));
        }

        protected override void OnEnter()
        {
            //ctx.isHiding = true;
        }

        protected override void OnExit()
        {
            ctx.isHiding = false;
        }

        protected override void OnUpdate(float deltaTime)
        {
            
        }

        protected override State GetTransition()
        {
            return null;
        }
    }
}