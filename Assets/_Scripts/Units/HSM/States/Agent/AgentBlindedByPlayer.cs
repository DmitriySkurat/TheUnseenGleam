namespace HSM {
    /// <summary>
    /// Агент ослеплён светом игрока.
    /// Стоит на месте; после BlindedByPlayerReactionTime переходит в Chase к игроку.
    /// Если ослепление прекратилось раньше — переходит в Suspicious.
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
            // Обновляем позицию игрока пока он ещё слепит — свет = позиция игрока
            if (ctx.isBlindedByPlayer)
                ctx.suspicionPosition = ctx.blindingSourcePosition;

            _reactionTimer -= deltaTime;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Реакция истекла — бежим к игроку
            if (_reactionTimer <= 0f)
                return Machine?.GetState<AgentChase>();

            // Игрок перестал слепить раньше времени — переходим в Suspicious
            if (!ctx.isBlindedByPlayer)
            {
                ctx.suspicionSource = SuspicionSource.Vision;
                ctx.suspicionTimer  = ctx.stats.SuspicionTimeOnSight;
                return Machine?.GetState<AgentSuspicious>();
            }

            return null;
        }
    }
}
