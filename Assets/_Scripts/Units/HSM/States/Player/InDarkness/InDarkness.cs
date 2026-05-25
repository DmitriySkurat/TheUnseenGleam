using UnityEngine;

namespace HSM
{
    public class InDarkness : State
    {
        private readonly PlayerContext ctx;
        private int _currentAnimHash;

        public InDarkness(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.currentSpeedMultiplier = ctx.stats.DarknessSpeedMultiplier;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            _currentAnimHash = 0;
        }

        protected override void OnExit()
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            ApplyDamage(deltaTime);
            HandleHorizontal(deltaTime);
            UpdateAnimation();

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!ctx.isAlive)  return Machine.GetState<Death>();
            if (ctx.isGrabbed) return Machine.GetState<PlayerGrabbed>();

            if (ctx.OnClimbable && Mathf.Abs(ctx.input.Move.y) > ctx.stats.VerticalDeadZoneThreshold)
                return Machine.GetState<Climb>();

            if (!ctx.grounded && Time.time >= ctx.frameLeftGrounded + Time.fixedDeltaTime)
                return Machine.GetState<Airborne>();

            if (!ctx.isInDarkness)
                return Machine.GetState<Grounded>();

            return null;
        }

        private void ApplyDamage(float deltaTime)
        {
            ctx.health?.TakeDamage(ctx.darknessDamagePerSecond * deltaTime);
        }

        private void HandleHorizontal(float deltaTime)
        {
            if (ctx.stats == null) return;

            float targetSpeed = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier;
            float accel = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;

            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetSpeed, accel * deltaTime);
        }

        private void UpdateAnimation()
        {
            if (ctx.anim == null) return;

            int targetHash = Mathf.Abs(ctx.velocity.x) > 0.5f
                ? PlayerAnimations.Walk
                : PlayerAnimations.Idle;

            if (targetHash == _currentAnimHash) return;
            _currentAnimHash = targetHash;
            ctx.anim.Play(targetHash);
        }
    }
}
