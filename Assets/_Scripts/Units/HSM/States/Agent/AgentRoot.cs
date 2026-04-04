namespace HSM {
    public class AgentRoot : State
    {
        public readonly AgentPatrol Patrol;
        public readonly AgentSuspicious Suspicious;
        public readonly AgentChase Chase;
        public readonly AgentPredictionChase PredictionChase;
        public readonly AgentSearch Search;

        readonly AgentContext ctx;

        public AgentRoot(StateMachine m, AgentContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol          = new AgentPatrol(m, this, ctx);
            Suspicious      = new AgentSuspicious(m, this, ctx);
            Chase           = new AgentChase(m, this, ctx);
            PredictionChase = new AgentPredictionChase(m, this, ctx);
            Search          = new AgentSearch(m, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override State GetTransition() => null;
    }
}
