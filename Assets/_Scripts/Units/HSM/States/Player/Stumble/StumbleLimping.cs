using UnityEngine;

namespace HSM {
    public class StumbleLimping : State
    {
        readonly PlayerContext ctx;

        public StumbleLimping(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.LimpSpeedMultiplier;
            ctx.anim?.Play(PlayerAnimations.Limp, 0, 0f);
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            float targetX = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.LimpSpeedMultiplier;
            float accel   = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;
            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetX, accel * deltaTime);

            base.OnUpdate(deltaTime);
        }

        // Выход только через родителя Stumble → PlayerRoot (grab / death)
        protected override State GetTransition() => null;
    }
}
