using UnityEngine;

namespace HSM {
    public class StumbleLimping : State
    {
        readonly PlayerContext ctx;
        float _timer;
        int _currentAnimHash;

        public StumbleLimping(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.isStumbleFalling = false;
            ctx.currentSpeedMultiplier = ctx.stats.LimpSpeedMultiplier;
            _timer = ctx.stats != null ? ctx.stats.LimpDuration : 3f;
            _currentAnimHash = 0;
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

            UpdateAnimation();

            base.OnUpdate(deltaTime);
        }

        void UpdateAnimation()
        {
            if (ctx.anim == null) return;
            int target = Mathf.Abs(ctx.velocity.x) > 0.5f
                ? PlayerAnimations.WalkAfterStumble
                : PlayerAnimations.Idle;
            if (target == _currentAnimHash) return;
            _currentAnimHash = target;
            ctx.anim.Play(target, 0, 0f);
        }

        protected override State GetTransition()
        {
            if (_timer <= 0f)
            {
                ctx.isStumbling        = false;
                ctx.grabEscapeDisabled = false;
                return Machine?.GetState<Grounded>();
            }
            return null;
        }
    }
}
