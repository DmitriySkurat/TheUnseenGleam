using UnityEngine;

namespace HSM
{
    public class Death : State
    {
        private PlayerContext ctx;

        public readonly DeathNormal DeathNormal;
        public readonly DeathGrabbed DeathGrabbed;

        public Death(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            DeathNormal  = new DeathNormal(m, this, ctx);
            DeathGrabbed = new DeathGrabbed(m, this, ctx);

            // Add(new ColorPhaseActivity(ctx.renderer){
            //     enterColor = Color.black,
            // });
        }

        protected override State GetInitialState() =>
            ctx.diedWhileGrabbed ? (State)DeathGrabbed : DeathNormal;

        protected override void OnEnter()
        {
            ctx.renderer.enabled = false;
            
            ctx.velocity = Vector2.zero;

            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            base.OnEnter();
        }
        

        protected override State GetTransition()
        {
            if (ctx.isAlive)
            {
                if (ctx.isGrabbed)
                    return Machine?.GetState<DeathGrabbed>();
                else
                    return Machine?.GetState<PlayerRoot>();
            }
            
            return null;
        }
    }
}
