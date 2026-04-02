namespace HSM {
    public class AgentRoot : State
    {
        public readonly AgentPatrol Patrol;

        readonly AgentContext ctx;

        public AgentRoot(StateMachine m, AgentContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol = new AgentPatrol(m, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override State GetTransition() => null;
    }
}
