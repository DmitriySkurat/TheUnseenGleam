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
            if (!ctx.grounded) return ((PlayerRoot)Parent).Airborne;
            
            return Mathf.Abs(ctx.move.x) <= 0.01f ? ((Grounded)Parent).Idle : null;
        }
        
        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats == null) return;
            if (Mathf.Abs(ctx.move.x) <= 0.01f) {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, ctx.stats.GroundDeceleration * deltaTime);
            } else {
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, ctx.move.x * ctx.stats.MaxSpeed, ctx.stats.Acceleration * deltaTime);
            }
        }
    }
}
