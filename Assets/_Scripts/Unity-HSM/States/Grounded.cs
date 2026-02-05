using UnityEngine;

namespace HSM {
    public class Grounded : State {
        readonly PlayerContext2D ctx;
        public readonly Idle Idle;
        public readonly Walk Walk;
        public readonly Run Run;
        public readonly Crouch Crouch;

        public Grounded(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent) {
            this.ctx = ctx;
            Idle = new Idle(m, this, ctx);
            Walk = new Walk(m, this, ctx);
            Run = new Run(m, this, ctx);
            Crouch = new Crouch(m, this, ctx);
            Add(new ColorPhaseActivity(null){
                enterColor = Color.yellow,  // runs while Grounded is activating
            });
        }
        
        protected override State GetInitialState() => Idle;

        protected override State GetTransition() {
            return ctx.IsGrounded ? null : ((PlayerRoot)Parent).Airborne;
        }
    }
}
