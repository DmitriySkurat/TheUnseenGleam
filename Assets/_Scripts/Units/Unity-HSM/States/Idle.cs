using UnityEngine;

namespace HSM {
    public class Idle : State {
        readonly PlayerContext ctx;

        public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Idle", true, false));
        }

        protected override State GetTransition() {
            return Mathf.Abs(ctx.move.x) > 0.01f ? ((Grounded)Parent).Move : null;
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats == null) return;
            var deceleration = ctx.grounded ? ctx.stats.GroundDeceleration : ctx.stats.AirDeceleration;
            ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, deceleration * deltaTime);
        }
    }
}
