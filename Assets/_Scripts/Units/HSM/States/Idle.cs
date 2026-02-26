using UnityEngine;

namespace HSM {
    public class Idle : State {
        readonly PlayerContext ctx;

        public Idle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorBoolActivity(ctx.anim, "Idle", true, false));
        }

        protected override State GetTransition() {
            if (ctx.input.CrouchHeld) return Machine != null ? Machine.GetState<Crouch>() : null;
            return Mathf.Abs(ctx.input.Move.x) > 0.01f ? (Machine != null ? Machine.GetState<Move>() : null) : null;
        }

        protected override void OnUpdate(float deltaTime) {
            if (ctx.stats != null) {
                var deceleration = ctx.grounded ? ctx.stats.GroundDeceleration : ctx.stats.AirDeceleration;
                ctx.velocity.x = Mathf.MoveTowards(ctx.velocity.x, 0, deceleration * deltaTime);
            }
            base.OnUpdate(deltaTime);
        }
    }
}
