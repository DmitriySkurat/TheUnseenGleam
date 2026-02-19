using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext ctx;
        public readonly Idle Idle;
        public readonly Move Move;

        public Grounded(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Move = new Move(m, this, ctx);
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Grounded", true, false));
        }
        
        protected override State GetInitialState() => Idle;

        protected override State GetTransition() {
            return ctx.grounded ? null : (Machine != null ? Machine.GetState<Airborne>() : null);
        }
    }
}
