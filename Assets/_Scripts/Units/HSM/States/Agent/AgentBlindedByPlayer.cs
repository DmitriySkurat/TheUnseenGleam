namespace HSM {
    /// <summary>
    /// Агент ослеплён светом игрока — стоит на месте.
    ///
    /// Два исхода:
    ///   - Игрок слепил >= BlindDurationToStun секунд → AgentStunned (оглушение).
    ///   - Игрок перестал слепить раньше порога (и прошло >= BlindedByPlayerReactionTime) → AgentChase.
    /// </summary>
    public class AgentBlindedByPlayer : State
    {
        readonly AgentContext ctx;

        // Сколько секунд агент непрерывно слеплён светом игрока
        float _blindedTimer;
        // Минимальное время стояния перед реакцией
        float _reactionTimer;

        public AgentBlindedByPlayer(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();
            _blindedTimer  = 0f;
            _reactionTimer = ctx.stats.BlindedByPlayerReactionTime;
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.isBlindedByPlayer)
            {
                // Пока слепит — отслеживаем позицию источника (= позиция игрока)
                ctx.suspicionPosition = ctx.blindingSourcePosition;
                _blindedTimer += deltaTime;
            }

            if (_reactionTimer > 0f)
                _reactionTimer -= deltaTime;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Слепит достаточно долго — оглушение
            if (_blindedTimer >= ctx.stats.BlindDurationToStun)
                return Machine?.GetState<AgentStunned>();

            // Ослепление прекратилось и минимальная пауза прошла — бежим на игрока
            if (!ctx.isBlindedByPlayer && _reactionTimer <= 0f)
                return Machine?.GetState<AgentChase>();

            return null;
        }
    }
}
