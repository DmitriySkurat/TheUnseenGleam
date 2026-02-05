using UnityEngine;

namespace HSM {
    public class Idle : GroundedStateBase {
        readonly PlayerContext2D ctx;

        public Idle(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (ctx.frameInput.CrouchHeld) return ((Grounded)Parent).Crouch;

            if (Mathf.Abs(ctx.frameInput.Move.x) > 0.01f) {
                return ctx.frameInput.RunHeld ? ((Grounded)Parent).Run : ((Grounded)Parent).Walk;
            }

            return null;
        }
    }
}
