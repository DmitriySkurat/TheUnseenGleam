namespace HSM {
    public class CrouchIdle : State {
        readonly PlayerContext ctx;

        public CrouchIdle(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            Add(new AnimatorPlayActivity(ctx.anim, "CrouchIdle"));
        }

        protected override State GetTransition() {
            if (ctx.HasMovementIntent) return Machine?.GetState<CrouchWalk>();
            return null;
        }
    }
}
