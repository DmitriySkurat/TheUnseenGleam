using UnityEngine;

namespace HSM {
    public class PlayerStumble : State
    {
        enum Phase { Falling, Limping }

        readonly PlayerContext ctx;
        Phase _phase;
        float _fallTimer;

        public PlayerStumble(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.stumblePending = false;
            _phase    = Phase.Falling;
            _fallTimer = ctx.stats != null ? ctx.stats.StumbleDuration : 1.2f;

            ctx.jumpToConsume = false;
            ctx.anim?.Play(PlayerAnimations.Stumble, 0, 0f);
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.jumpToConsume = false;

            switch (_phase)
            {
                case Phase.Falling:
                    TickFalling(deltaTime);
                    break;
                case Phase.Limping:
                    TickLimping(deltaTime);
                    break;
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.currentSpeedMultiplier = 1f;
            base.OnExit();
        }

        // Выход только через PlayerRoot: isGrabbed → PlayerGrabbed, !isAlive → Death
        protected override State GetTransition() => null;

        void TickFalling(float deltaTime)
        {
            _fallTimer -= deltaTime;

            // Тормозим до нуля — HandleHorizontal не вызывается вне Grounded
            ctx.velocity.x = Mathf.MoveTowards(
                ctx.velocity.x, 0f,
                ctx.stats.GroundDeceleration * deltaTime);

            if (_fallTimer <= 0f)
                EnterLimping();
        }

        void EnterLimping()
        {
            _phase = Phase.Limping;
            ctx.currentSpeedMultiplier = ctx.stats.LimpSpeedMultiplier;
            ctx.anim?.Play(PlayerAnimations.Limp, 0, 0f);
        }

        void TickLimping(float deltaTime)
        {
            // Движение с уменьшенной скоростью — дублируем логику Grounded.HandleHorizontal
            float targetX = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.LimpSpeedMultiplier;
            float accel   = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;
            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetX, accel * deltaTime);
        }
    }
}
