using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red, // runs while Airborne is activating
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Airborne", true, false));
        }
        
        protected override State GetTransition()
        {
            if (ctx.OnClimbable && Mathf.Abs(ctx.input.Move.y) > 0.1f && !ctx.isClimbing) return Machine.GetState<Climb>();
            if (ctx.grounded) return Machine != null ? Machine.GetState<Grounded>() : null;
        
            return null;
        } 

        protected override void OnEnter()
        {
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
            
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) HandleHorizontal(deltaTime);
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
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, ctx.input.Move.x * ctx.stats.MaxSpeed * ctx.currentSpeedMultiplier, ctx.stats.Acceleration * deltaTime);
            }
        }
    }
}
