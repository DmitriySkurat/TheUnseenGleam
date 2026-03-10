using UnityEngine;

namespace HSM
{
    public class Climb : State
    {
        private PlayerContext ctx;

        public Climb(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.purple,
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Climb", true, false));
        }

        protected override void OnEnter()
        {
            ctx.isClimbing = true;
            ctx.velocity = Vector2.zero;
        }

        protected override void OnExit()
        {
            ctx.isClimbing = false;
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.velocity.y = ctx.input.Move.y * ctx.stats.MaxSpeed * ctx.stats.ClimbVerticalSpeedMultiplier;
            ctx.velocity.x = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.ClimbHorizontalSpeedMultiplier;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.grounded && ctx.input.Move.y <= 0)
                return Machine.GetState<Grounded>();

            if (!ctx.onLadder)
                return Machine.GetState<Airborne>();

            return null;
        }
    }
}