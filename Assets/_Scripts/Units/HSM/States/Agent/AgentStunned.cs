namespace HSM {
    /// <summary>
    /// Агент оглушён после длительного ослепления светом игрока.
    /// Стоит неподвижно StunDuration секунд, затем переходит в Suspicious
    /// на позицию последнего замеченного источника света.
    /// </summary>
    public class AgentStunned : State
    {
        readonly AgentContext ctx;
        float _stunTimer;

        public AgentStunned(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();
            _stunTimer = ctx.stats.StunDuration;
            // Сброс таймера: чтобы Root не сразу запросил повторный стан
            ctx.blindedByPlayerTimer = 0f;
            base.OnEnter();
        }

        protected override void OnExit()
        {
            // Сброс и при выходе: игроку нужно слепить заново BlindDurationToStun секунд
            ctx.blindedByPlayerTimer = 0f;
            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _stunTimer -= deltaTime;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            if (_stunTimer <= 0f)
            {
                // Агент приходит в себя — подозревает место, где был источник света
                ctx.suspicionSource = SuspicionSource.Vision;
                ctx.suspicionTimer  = ctx.stats.SuspicionTimeOnSight;
                return Machine?.GetState<AgentSuspicious>();
            }

            return null;
        }
    }
}
