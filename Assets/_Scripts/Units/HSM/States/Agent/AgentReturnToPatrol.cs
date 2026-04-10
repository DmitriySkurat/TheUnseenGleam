using PlatNav;

namespace HSM {
    public class AgentReturnToPatrol : State
    {
        readonly AgentContext ctx;
        bool _navigating;

        public AgentReturnToPatrol(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();
            ctx.currentPatrolIndex = 0;
            _navigating = false;
            StartNavigating();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_navigating)
            {
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

            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;

            if (ctx.pendingNoiseAlert)
            {
                ctx.pendingNoiseAlert = false;
                if (ctx.pendingNoiseRadius >= ctx.stats.SearchNoiseRadius)
                {
                    ctx.suspicionSource   = SuspicionSource.Noise;
                    ctx.suspicionPosition = ctx.pendingNoisePosition;
                    return Machine != null ? Machine.GetState<AgentSearch>() : null;
                }
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;
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
