using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;

        bool isJumping;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red,
            });
        }
        
        protected override State GetTransition()
        {
            if (ctx.OnClimbable && Mathf.Abs(ctx.input.Move.y) > 0.1f && !ctx.isClimbing) return Machine.GetState<Climb>();
            if (ctx.canGrabLedge) return Machine?.GetState<LedgeClimb>();
            if (ctx.grounded) return Machine != null ? Machine.GetState<Grounded>() : null;

            return null;
        }

        protected override void OnEnter()
        {
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            isJumping = ctx.velocity.y > 0f;
            ctx.anim.Play(isJumping ? PlayerAnimations.Jump : PlayerAnimations.Dropdown);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) HandleHorizontal(deltaTime);

            // coyote-прыжок: перешли из падения в прыжок
            if (!isJumping && ctx.velocity.y > 0f) {
                isJumping = true;
                ctx.anim.Play(PlayerAnimations.Jump);
            }

            // переключаем с Jump на Dropdown в вершине дуги
            if (isJumping && ctx.velocity.y < 0f) {
                isJumping = false;
                ctx.anim.Play(PlayerAnimations.Dropdown);
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            base.OnExit();
        }

        void HandleHorizontal(float deltaTime) {
            if (Mathf.Abs(ctx.input.Move.x) <= 0.01f) {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, ctx.stats.AirDeceleration * deltaTime);
            } else {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier * ctx.darknessSpeedMultiplier * ctx.spikesSpeedMultiplier, ctx.stats.Acceleration * deltaTime);
            }
        }
    }
}
