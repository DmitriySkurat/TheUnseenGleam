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
            //if (Mathf.Abs(ctx.input.Move.x) > 0.01f) return Machine != null ? Machine.GetState<Move>() : null;
            if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;
            
            return null;
        }

    }
}
