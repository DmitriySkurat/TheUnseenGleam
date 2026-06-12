using UnityEngine;

namespace HSM
{
    public class Climb : State
    {
        private PlayerContext ctx;
        private NoiseSystem _noiseSystem;

        public Climb(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            _noiseSystem = Services.Get<NoiseSystem>();
            // Add(new ColorPhaseActivity(ctx.renderer){
            //     enterColor = Color.purple,
            // });
        }

        protected override void OnEnter()
        {
            ctx.isClimbing = true;
            ctx.velocity = Vector2.zero;
            ctx.anim.Play(PlayerAnimations.ClimbIdle, 0, 0f);

            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.isClimbing = false;
            
            // Если сошли с лестницы/лианы в воздухе —
            // начинаем отсчет падения заново
            if (!ctx.grounded)
            {
                ctx.airborneStartY = ctx.transform.position.y;
            }
            
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.velocity.y = ctx.input.Move.y * ctx.stats.MaxSpeed * ctx.stats.ClimbVerticalSpeedMultiplier;
            ctx.velocity.x = ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.stats.ClimbHorizontalSpeedMultiplier;

            if (Mathf.Abs(ctx.velocity.y) > 0f)
            {
                ctx.anim.Play(PlayerAnimations.Climb);
            }
            else
            {
                ctx.anim.Play(PlayerAnimations.ClimbIdle);
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.jumpToConsume && ctx.CanJump)
            {
                ctx.jumpToConsume = false;
                ctx.endedJumpEarly = false;
                ctx.timeJumpWasPressed = 0;
                ctx.bufferedJumpUsable = false;
                ctx.coyoteUsable = false;
                ctx.velocity.y = ctx.stats.JumpPower;
                ctx.stamina = Mathf.Max(0f, ctx.stamina - ctx.stats.JumpStaminaCost);
                if (ctx.transform != null)
                    _noiseSystem.EmitNoise(ctx.transform.position, ctx.noiseStats.JumpStartNoiseRadius, ctx.transform.gameObject, NoiseType.Jump, ctx.noiseStats.RadiusVariance);
                return Machine.GetState<Airborne>();
            }

            if (ctx.grounded && ctx.input.Move.y <= 0)
                return Machine.GetState<Grounded>();

            if (!ctx.OnClimbable)
                return ctx.grounded
                    ? Machine.GetState<Grounded>()
                    : Machine.GetState<Airborne>();

            return null;
        }
    }
}
