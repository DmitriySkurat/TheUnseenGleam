using UnityEngine;

namespace HSM {
    public class Walk : GroundedStateBase {
        readonly PlayerContext2D ctx;

        public Walk(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (ctx.frameInput.CrouchHeld) return ((Grounded)Parent).Crouch;

            if (Mathf.Abs(ctx.frameInput.Move.x) <= 0.01f) return ((Grounded)Parent).Idle;

            return ctx.frameInput.RunHeld ? ((Grounded)Parent).Run : null;
        }

        protected override float GetMaxSpeed() => Stats.MaxSpeed * Stats.WalkSpeedMultiplier;
    }
}
