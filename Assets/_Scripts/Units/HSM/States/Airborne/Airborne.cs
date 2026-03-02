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
        
        protected override State GetTransition() => ctx.grounded ? (Machine != null ? Machine.GetState<Grounded>() : null) : null;

        protected override void OnEnter() {
            // TODO: Update Animator through ctx.anim
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) HandleHorizontal(deltaTime);
            base.OnUpdate(deltaTime);
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
