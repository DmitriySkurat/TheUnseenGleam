namespace HSM {
    public class AgentRoot : State
    {
        public readonly AgentPatrol Patrol;
        public readonly AgentSuspicious Suspicious;
        public readonly AgentChase Chase;
        public readonly AgentPredictionChase PredictionChase;
        public readonly AgentSearch Search;
        public readonly AgentReturnToPatrol ReturnToPatrol;
        public readonly AgentAttack Attack;

        readonly AgentContext ctx;

        public AgentRoot(StateMachine m, AgentContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol          = new AgentPatrol(m, this, ctx);
            Suspicious      = new AgentSuspicious(m, this, ctx);
            Chase           = new AgentChase(m, this, ctx);
            PredictionChase = new AgentPredictionChase(m, this, ctx);
            Search          = new AgentSearch(m, this, ctx);
            ReturnToPatrol  = new AgentReturnToPatrol(m, this, ctx);
            Attack          = new AgentAttack(m, this, ctx);
        }

        // Debug mode: always start from direct chase.
        protected override State GetInitialState() => Patrol;

        protected override State GetTransition() => null;
    }
}
