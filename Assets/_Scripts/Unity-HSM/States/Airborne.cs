using UnityEngine;

namespace HSM {
    public class Airborne : State {
        readonly PlayerContext2D ctx;
        public readonly AirborneJump Jump;
        public readonly AirborneFall Fall;

        public Airborne(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent) {
            this.ctx = ctx;
            Jump = new AirborneJump(m, this, ctx);
            Fall = new AirborneFall(m, this, ctx);
            Add(new ColorPhaseActivity(null){
                enterColor = Color.red, // runs while Airborne is activating
            });
        }
        
        protected override State GetTransition() => ctx.IsGrounded ? ((PlayerRoot)Parent).Grounded : null;

        protected override State GetInitialState() {
            return ctx.rb != null && ctx.rb.linearVelocity.y >= 0f ? Jump : Fall;
        }

        protected override void OnEnter() {
            // TODO: Update Animator through ctx.anim
        }
    }
}
