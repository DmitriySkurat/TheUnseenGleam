using UnityEngine;

namespace HSM
{
    public class OnSpikes : State
    {
        private readonly PlayerContext ctx;
        private float _savedSpeedMultiplier;
        private float _damageTimer;
        private int _currentAnimHash;

        public OnSpikes(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _savedSpeedMultiplier = ctx.currentSpeedMultiplier;
            ctx.currentSpeedMultiplier = ctx.stats.SpikesSpeedMultiplier;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            _currentAnimHash = 0;
            _damageTimer = 0f;

            // Мгновенный урон при заходе (как в оригинале)
            ctx.health?.TakeDamage(ctx.spikesDamage);
        }

        protected override void OnExit()
        {
            ctx.currentSpeedMultiplier = _savedSpeedMultiplier;
            ctx.currentFootstepInterval = 0f;
            ctx.currentNoiseRadius = 0f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            HandlePeriodicDamage(deltaTime);
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

            if (!ctx.isOnSpikes)
                return Machine.GetState<Grounded>();

            return null;
        }

        // Урон раз в footstepInterval (как в оригинальном Spikes.Update)
        private void HandlePeriodicDamage(float deltaTime)
        {
            float interval = ctx.currentFootstepInterval;
            if (interval <= 0f) return;

            _damageTimer += deltaTime;
            if (_damageTimer < interval) return;

            _damageTimer = 0f;
            ctx.health?.TakeDamage(ctx.spikesDamage);
        }

        private void HandleHorizontal(float deltaTime)
        {
            if (ctx.stats == null) return;

            float targetSpeed = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier;
            float accel = Mathf.Abs(ctx.input.Move.x) > 0.01f
                ? ctx.stats.Acceleration
                : ctx.stats.GroundDeceleration;

            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, targetSpeed, accel * deltaTime);

            // Нужно для HandlePeriodicDamage: движение = интервал шага, стоим = нет периодического урона
            bool isMoving = Mathf.Abs(ctx.velocity.x) > 0.5f;
            ctx.currentFootstepInterval = isMoving ? ctx.noiseStats.WalkFootstepInterval : 0f;
            ctx.currentNoiseRadius      = isMoving ? ctx.noiseStats.WalkNoiseRadius      : 0f;
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
