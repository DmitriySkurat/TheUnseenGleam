namespace HSM {
    /// <summary>
    /// Агент останавливается, сигнализирует союзникам и ждёт <see cref="AgentScriptableStats.AlertDuration"/> секунд.
    /// Все агенты в радиусе <see cref="AgentScriptableStats.AlertHearRadius"/> получат alertPending = true
    /// и перейдут в Search.
    ///
    /// Переходы:
    ///   AgentChase → AgentAlert  (потеряли игрока из виду)
    ///   AgentAlert  → AgentChase (игрок снова виден)
    ///   AgentAlert  → AgentSearch (таймер истёк)
    /// </summary>
    public class AgentAlert : State
    {
        readonly AgentContext ctx;
        float _timer;

        public AgentAlert(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _timer = ctx.stats.AlertDuration;

            ctx.nav.SetTarget(null);
            ctx.nav.Abort();

            // Не реагируем на собственный же broadcast
            ctx.alertPending = false;

            ctx.alertSystem?.BroadcastAlert(ctx.transform.position);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer -= deltaTime;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine?.GetState<AgentChase>();

            if (_timer <= 0f)
            {
                ctx.suspicionPosition = ctx.transform.position;
                return Machine?.GetState<AgentSearch>();
            }

            return null;
        }
    }
}
