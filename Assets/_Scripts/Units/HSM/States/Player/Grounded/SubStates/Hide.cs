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
                enterColor = Color.cyan,
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Hide", true, false));
        }

        protected override void OnEnter()
        {
            ctx.isHiding = true;
            ctx.currentSpeedMultiplier = ctx.stats.HideSpeedMultiplier;
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.velocity = Vector2.zero;
        }

        protected override void OnExit()
        {
            ctx.isHiding = false;
            ctx.velocity.y = 0f;
            // ctx.currentNoiseRadius = 0f;
            // ctx.currentFootstepInterval = 0f;
            // if (ctx.activeHideSpot != null)
            //     ctx.activeHideSpot = null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.velocity = Vector2.zero;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!ctx.isHiding)
            {
                if (ctx.WantsCrouch) return Machine != null ? Machine.GetState<Crouch>() : null;
                if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;
                return Machine != null ? Machine.GetState<Idle>() : null;
            }

            return null;
        }
    }
}
