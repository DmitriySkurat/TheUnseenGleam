using UnityEngine;

namespace HSM
{
    public class DeathGrabbed : State
    {
        private PlayerContext ctx;

        public DeathGrabbed(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "DeathGrabbed", true, false));
            
            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.Death));
        }

        protected override void OnEnter()
        {
            Debug.Log("Player has died while grabbed");
            
            base.OnEnter();
        }

        protected override State GetTransition()
        {
            if (ctx.isAlive) return Machine?.GetState<PlayerRoot>();
            
            return null;
        }
    }
}
