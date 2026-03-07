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
            //ctx.velocity = Vector2.zero;
        }

        protected override void OnExit()
        {
            ctx.isClimbing = false;
        }

        protected override void OnUpdate(float deltaTime)
        {
            float climbSpeed = ctx.stats.MaxSpeed * 0.5f;

            ctx.velocity.y = ctx.input.Move.y * climbSpeed;
            ctx.velocity.x = 0;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!ctx.onLadder && !ctx.grounded) return Machine.GetState<Airborne>();
            if (!ctx.onLadder && ctx.grounded) return Machine.GetState<Grounded>();

            return null;
        }
    }
}