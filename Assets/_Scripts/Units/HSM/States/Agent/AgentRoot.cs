namespace HSM {
    public class AgentRoot : State
    {
        public readonly AgentPatrol Patrol;
        public readonly AgentSuspicious Suspicious;
        public readonly AgentChase Chase;
        public readonly AgentAlert Alert;
        public readonly AgentPredictionChase PredictionChase;
        public readonly AgentSearch Search;
        public readonly AgentReturnToPatrol ReturnToPatrol;
        public readonly AgentGrabPlayer GrabPlayer;
        public readonly AgentBlindedByPlayer BlindedByPlayer;
        public readonly AgentStunned Stunned;

        readonly AgentContext ctx;

        public AgentRoot(StateMachine m, AgentContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol          = new AgentPatrol(m, this, ctx);
            Suspicious      = new AgentSuspicious(m, this, ctx);
            Chase           = new AgentChase(m, this, ctx);
            Alert           = new AgentAlert(m, this, ctx);
            PredictionChase = new AgentPredictionChase(m, this, ctx);
            Search          = new AgentSearch(m, this, ctx);
            ReturnToPatrol  = new AgentReturnToPatrol(m, this, ctx);
            GrabPlayer      = new AgentGrabPlayer(m, this, ctx);
            BlindedByPlayer = new AgentBlindedByPlayer(m, this, ctx);
            Stunned         = new AgentStunned(m, this, ctx);
        }

        protected override State GetInitialState() => Patrol;

        protected override State GetTransition()
        {
            // Глобальный стан: срабатывает из любого состояния
            if (!ctx.IsTraversingLink &&
                ctx.blindedByPlayerTimer >= ctx.stats.BlindDurationToStun)
                return Machine?.GetState<AgentStunned>();

            return null;
        }
    }
}
