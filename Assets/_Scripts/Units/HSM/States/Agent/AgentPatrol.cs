namespace HSM {
    /// <summary>
    /// Составное состояние патруля.
    /// Дочерние состояния:
    ///   AgentPatrolWalk  — навигация по точкам маршрута (начальное)
    ///   AgentLookAround  — периодический случайный осмотр
    ///
    /// AgentPatrol обрабатывает все внешние переходы (игрок, шум, тревога).
    /// Дочерние состояния переключаются между собой самостоятельно.
    /// </summary>
    public class AgentPatrol : State
    {
        public readonly AgentPatrolWalk Walk;
        public readonly AgentLookAround LookAround;

        readonly AgentContext ctx;

        public AgentPatrol(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Walk       = new AgentPatrolWalk(m, this, ctx);
            LookAround = new AgentLookAround(m, this, ctx);
        }

        protected override State GetInitialState() => Walk;

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // if (ctx.isBlindedByPlayer)
            //     return Machine?.GetState<AgentBlindedByPlayer>();

            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine?.GetState<AgentSuspicious>();

            if (ctx.pendingNoiseAlert)
            {
                if (ctx.pendingNoiseRadius >= ctx.stats.SearchNoiseRadius)
                {
                    ctx.suspicionSource   = SuspicionSource.Noise;
                    ctx.suspicionPosition = ctx.pendingNoisePosition;
                    return Machine?.GetState<AgentSearch>();
                }
                return Machine?.GetState<AgentSuspicious>();
            }

            if (ctx.alertPending && !ctx.isBlindedByEnvironment)
            {
                ctx.alertPending      = false;
                ctx.suspicionPosition = ctx.alertPosition;
                return Machine?.GetState<AgentSearch>();
            }

            return null;
        }
    }
}
