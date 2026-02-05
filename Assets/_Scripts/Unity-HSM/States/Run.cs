using UnityEngine;

namespace HSM {
    public class Run : GroundedStateBase {
        readonly PlayerContext2D ctx;

        public Run(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent, ctx) {
            this.ctx = ctx;
        }

        protected override State GetTransition() {
            if (ctx.frameInput.CrouchHeld) return ((Grounded)Parent).Crouch;

            if (Mathf.Abs(ctx.frameInput.Move.x) <= 0.01f) return ((Grounded)Parent).Idle;

            return ctx.frameInput.RunHeld ? null : ((Grounded)Parent).Walk;
        }

        protected override float GetMaxSpeed() => Stats.MaxSpeed * Stats.RunSpeedMultiplier;
    }
}
