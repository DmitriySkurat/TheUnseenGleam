namespace HSM {
    /// <summary>
    /// Агент останавливается, сигнализирует союзникам и ждёт <see cref="AgentScriptableStats.AlertDuration"/> секунд.
    /// Все агенты в радиусе <see cref="AgentScriptableStats.AlertRadius"/> получат alertPending = true
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
        bool _turned;

        public AgentAlert(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            if (ctx.anim != null)
                Add(new AnimatorPlayActivity(ctx.anim, AgentAnimations.Alert));
        }

        protected override void OnEnter()
        {
            _timer  = ctx.stats.AlertDuration;
            _turned = false;

            ctx.nav.SetTarget(null);
            ctx.nav.Abort();

            // Не реагируем на собственный же broadcast
            ctx.alertPending = false;

            ctx.alertSystem?.BroadcastAlert(ctx.transform.position);

            ctx.SayRandom(ctx.stats.AlertLines);
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _timer -= deltaTime;

            if (!_turned && _timer <= ctx.stats.AlertTurnTime)
            {
                FlipDirection();
                _turned = true;
            }

            base.OnUpdate(deltaTime);
        }

        void FlipDirection()
        {
            var scale = ctx.transform.localScale;
            scale.x = -scale.x;
            ctx.transform.localScale = scale;
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine?.GetState<AgentChase>();

            if (_timer <= 0f)
                return Machine?.GetState<AgentPredictionChase>();

            return null;
        }
    }
}
