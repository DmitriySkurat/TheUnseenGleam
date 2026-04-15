using UnityEngine;

namespace HSM
{
    public class Death : State
    {
        private PlayerContext ctx;

        public readonly DeathNormal Normal;
        public readonly DeathGrabbed Grabbed;

        public Death(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Normal  = new DeathNormal(m, this, ctx);
            Grabbed = new DeathGrabbed(m, this, ctx);

            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.black,
            });
        }

        protected override State GetInitialState() =>
            ctx.diedWhileGrabbed ? (State)Grabbed : Normal;

        protected override void OnEnter()
        {
            ctx.velocity = Vector2.zero;

            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            base.OnEnter();
        }
    }
}
