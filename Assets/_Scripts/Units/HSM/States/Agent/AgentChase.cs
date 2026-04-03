namespace HSM {
    public class AgentChase : State
    {
        readonly AgentContext ctx;

        bool _inMemoryPhase;

        public AgentChase(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _inMemoryPhase = false;
            ctx.chaseMemoryTimer = 0f;

            ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
            ctx.nav.SetTarget(ctx.playerTransform);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool canSee = ctx.vision != null && ctx.vision.CanSeePlayer;

            if (canSee)
            {
                ctx.suspicionPosition = ctx.vision.LastSeenPosition;

                if (ctx.playerRb != null)
                    ctx.chaseLastKnownVelocity = ctx.playerRb.linearVelocity;

                if (_inMemoryPhase)
                {
                    // Regained sight during memory phase — resume direct chase
                    _inMemoryPhase = false;
                    ctx.chaseMemoryTimer = 0f;
                    ctx.nav.Abort();
                    ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
                    ctx.nav.SetTarget(ctx.playerTransform);
                }
            }
            else if (!_inMemoryPhase)
            {
                // Just lost sight — enter memory phase and navigate to predicted position
                _inMemoryPhase = true;
                ctx.chaseMemoryTimer = ctx.stats.ChaseMemoryTime;

                ctx.chasePredictedPosition = ctx.suspicionPosition
                    + ctx.chaseLastKnownVelocity * ctx.stats.ChaseMemoryTime;
                ctx.suspicionPosition = ctx.chasePredictedPosition;

                ctx.nav.SetTarget(null);
                ctx.nav.MoveTo(ctx.chasePredictedPosition, ctx.stats.ChaseSpeed);
            }
            else
            {
                ctx.chaseMemoryTimer -= deltaTime;
            }

            ctx.nav.Tick(deltaTime);

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (_inMemoryPhase && ctx.chaseMemoryTimer <= 0f)
                return Machine != null ? Machine.GetState<AgentSearch>() : null;

            return null;
        }
    }
}
