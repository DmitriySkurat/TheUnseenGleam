namespace HSM {
    /// <summary>
    /// Агент ослеплён светом игрока — стоит на месте.
    ///
    /// Исход:
    ///   - Свет погас и прошло >= BlindedByPlayerReactionTime → AgentChase (бежит на игрока).
    ///   - Агент слеплён достаточно долго (>= BlindDurationToStun) → AgentRoot переводит в AgentStunned.
    /// </summary>
    public class AgentBlindedByPlayer : State
    {
        readonly AgentContext ctx;
        float _reactionTimer;

        public AgentBlindedByPlayer(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();
            _reactionTimer = ctx.stats.BlindedByPlayerReactionTime;
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Обновляем позицию источника пока он активен (свет = позиция игрока)
            if (ctx.isBlindedByPlayer)
                ctx.suspicionPosition = ctx.blindingSourcePosition;

            if (_reactionTimer > 0f)
                _reactionTimer -= deltaTime;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Свет погас и минимальная пауза прошла — бежим на игрока
            if (!ctx.isBlindedByPlayer && _reactionTimer <= 0f)
                return Machine?.GetState<AgentChase>();

            // Стан (длительное ослепление) обрабатывается глобально в AgentRoot

            return null;
        }
    }
}
