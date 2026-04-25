using PlatNav;

namespace HSM {
    public class AgentReturnToPatrol : State
    {
        readonly AgentContext ctx;
        bool _navigating;
        bool _wasTraversing;

        public AgentReturnToPatrol(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _wasTraversing = false;
            ctx.anim?.Play(AgentAnimations.Walk, 0, 0f);

            ctx.nav.SetTarget(null);
            ctx.nav.Abort();
            ctx.currentPatrolIndex = 0;
            _navigating = false;
            StartNavigating();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool traversing = ctx.IsTraversingLink;
            if (traversing != _wasTraversing)
            {
                var traversalAnim = ctx.nav.IsTraversingFall ? AgentAnimations.Dropdown : AgentAnimations.Jump;
                ctx.anim?.Play(traversing ? traversalAnim : AgentAnimations.Walk, 0, 0f);
                _wasTraversing = traversing;
            }

            if (_navigating)
            {
                ctx.nav.SetSpeed(ctx.isBlindedByEnvironment
                    ? ctx.stats.BlindedByEnvironmentSpeed
                    : ctx.stats.PatrolSpeed);
                ctx.nav.Tick(deltaTime);

                if (ctx.nav.State == PlatNavState.Idle)
                    _navigating = false;
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.nav.Abort();
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Игрок слепит агента — переходим в BlindedByPlayer
            if (ctx.isBlindedByPlayer)
                return Machine != null ? Machine.GetState<AgentBlindedByPlayer>() : null;

            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;

            if (ctx.pendingNoiseAlert)
            {
                ctx.pendingNoiseAlert = false;
                // Ослеплены окружением — поглощаем шум без смены маршрута
                if (!ctx.isBlindedByEnvironment)
                {
                    if (ctx.pendingNoiseRadius >= ctx.stats.SearchNoiseRadius)
                    {
                        ctx.suspicionSource   = SuspicionSource.Noise;
                        ctx.suspicionPosition = ctx.pendingNoisePosition;
                        return Machine != null ? Machine.GetState<AgentSearch>() : null;
                    }
                    return Machine != null ? Machine.GetState<AgentSuspicious>() : null;
                }
            }

            if (ctx.alertPending && !ctx.isBlindedByEnvironment)
            {
                ctx.alertPending      = false;
                ctx.suspicionPosition = ctx.alertPosition;
                return Machine != null ? Machine.GetState<AgentSearch>() : null;
            }

            // Arrived at patrol start (or path failed) — hand off to AgentPatrol
            if (!_navigating)
                return Machine != null ? Machine.GetState<AgentPatrol>() : null;

            return null;
        }

        void StartNavigating()
        {
            if (ctx.PatrolCount == 0) return;
            _navigating = ctx.nav.MoveTo(ctx.CurrentPatrolPosition, ctx.stats.PatrolSpeed);
        }
    }
}
