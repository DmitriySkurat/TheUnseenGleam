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
            _timer = ctx.limpTimeRemaining > 0f
                ? ctx.limpTimeRemaining
                : (ctx.stats != null ? ctx.stats.LimpDuration : 3f);
            ctx.limpTimeRemaining = 0f;
            _currentAnimHash = 0;
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer -= deltaTime;
            ctx.limpTimeRemaining = _timer;

            float t = Mathf.Clamp01(1f - (_timer / 0.5f));
            ctx.currentSpeedMultiplier = Mathf.Lerp(ctx.stats.LimpSpeedMultiplier, ctx.stats.WalkSpeedMultiplier, t);

            float targetX = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier;
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
            if (ctx.OnClimbable && Mathf.Abs(ctx.input.Move.y) > ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Climb>();

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
