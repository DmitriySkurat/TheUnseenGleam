using UnityEngine;

namespace HSM {
    public class AirborneFall : AirborneStateBase {
        readonly PlayerContext2D ctx;

        public AirborneFall(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (ctx.rb == null) return null;
            return ctx.rb.linearVelocity.y >= 0f ? ((Airborne)Parent).Jump : null;
        }
    }
}
