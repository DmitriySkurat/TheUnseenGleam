namespace HSM {
    public class PlayerRoot : State {
        public readonly Grounded Grounded;
        public readonly Airborne Airborne;
        readonly PlayerContext2D ctx;

        public PlayerRoot(StateMachine m, PlayerContext2D ctx) : base(m, null) {
            this.ctx = ctx;
            Grounded = new Grounded(m, this, ctx);
            Airborne = new Airborne(m, this, ctx);
        }
        
        protected override State GetInitialState() => Grounded;
        protected override State GetTransition() => null;
    }
}
