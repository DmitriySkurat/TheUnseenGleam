using UnityEngine;

namespace HSM {
    public class StumbleFalling : State
    {
        readonly PlayerContext ctx;
        float _timer;

        public StumbleFalling(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _timer = ctx.stats != null ? ctx.stats.StumbleDuration : 1.2f;
            ctx.isStumbleFalling = true;
            float facing = ctx.velocity.x != 0f ? Mathf.Sign(ctx.velocity.x) : 1f;
            ctx.velocity.x = facing * (ctx.stats != null ? ctx.stats.StumbleImpulse : 3f);
            ctx.anim?.Play(PlayerAnimations.Stumble, 0, 0f);
            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.isStumbleFalling = false;
            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer -= deltaTime;

            // HandleHorizontal не вызывается вне Grounded — тормозим напрямую
            ctx.velocity.x = Mathf.MoveTowards(
                ctx.velocity.x, 0f,
                ctx.stats.GroundDeceleration * deltaTime);

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (_timer <= 0f) return Machine?.GetState<StumbleLimping>();
            return null;
        }
    }
}
