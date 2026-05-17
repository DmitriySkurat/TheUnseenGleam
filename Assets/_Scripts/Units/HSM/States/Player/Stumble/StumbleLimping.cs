using UnityEngine;

namespace HSM {
    public class StumbleLimping : State
    {
        readonly PlayerContext ctx;
        float _timer;

        public StumbleLimping(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.LimpSpeedMultiplier;
            _timer = ctx.stats != null ? ctx.stats.LimpDuration : 3f;
            ctx.anim?.Play(PlayerAnimations.Limp, 0, 0f);
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer -= deltaTime;

            float targetX = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.LimpSpeedMultiplier;
            float accel   = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;
            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetX, accel * deltaTime);

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (_timer <= 0f) return Machine?.GetState<Grounded>();
            return null;
        }
    }
}
