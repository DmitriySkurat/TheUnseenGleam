using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext ctx;

        public Airborne(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
        }
        
        //protected override State GetTransition() => ctx.grounded ? ((PlayerRoot)Parent).Grounded : null;
        protected override State GetTransition() => null;

        protected override void OnEnter() {
            // TODO: Update Animator through ctx.anim
        }
    }
}