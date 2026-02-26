using UnityEngine;

namespace HSM {
    public class Move : State {
        readonly PlayerContext ctx;

        public Move(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Moving", true, false));
            Add(new AudioLoopActivity(ctx.audio));
        }

        protected override State GetTransition() {
            if (!ctx.grounded) return Machine != null ? Machine.GetState<Airborne>() : null;
            if (ctx.input.CrouchHeld) return Machine != null ? Machine.GetState<Crouch>() : null;
            
            return Mathf.Abs(ctx.input.Move.x) <= 0.01f ? (Machine != null ? Machine.GetState<Idle>() : null) : null;
        }
        
        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) {
                if (Mathf.Abs(ctx.input.Move.x) <= 0.01f) {
                    ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, ctx.stats.GroundDeceleration * deltaTime);
                } else {
                    ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, ctx.input.Move.x * ctx.stats.MaxSpeed, ctx.stats.Acceleration * deltaTime);
                }
            }
            base.OnUpdate(deltaTime);
        }
    }
}
