namespace HSM {
    public class CrouchWalk : State {
        readonly PlayerContext ctx;

        public CrouchWalk(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorPlayActivity(ctx.anim, "CrouchWalk"));
        }

        protected override State GetTransition() {
            if (!ctx.HasMovementIntent) return Machine?.GetState<CrouchIdle>();
            return null;
        }
    }
}
