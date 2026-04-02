using PlatNav;

namespace HSM {
    public class AgentSearch : State
    {
        readonly AgentContext ctx;
        bool _navigating;

        public AgentSearch(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.SetTarget(null);
            ctx.searchWaitTimer = 0f;
            _navigating = false;
            StartNavigatingToSearchPoint();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // New noise arrived — redirect to the updated position
            if (ctx.pendingNoiseAlert)
            {
                ctx.suspicionPosition = ctx.pendingNoisePosition;
                ctx.pendingNoiseAlert = false;
                ctx.searchWaitTimer   = 0f;
                StartNavigatingToSearchPoint();
            }

            if (_navigating)
            {
                ctx.nav.Tick(deltaTime);

                if (ctx.nav.State == PlatNavState.Idle)
                {
                    _navigating         = false;
                    ctx.searchWaitTimer = ctx.stats.SearchWaitTime;
                }
            }
            else if (ctx.searchWaitTimer > 0f)
            {
                ctx.searchWaitTimer -= deltaTime;
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
            // Spotted the player while searching — re-enter suspicion loop
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;

            // Finished navigating and waited long enough — back to patrol
            if (!_navigating && ctx.searchWaitTimer <= 0f)
                return Machine != null ? Machine.GetState<AgentPatrol>() : null;

            return null;
        }

        void StartNavigatingToSearchPoint()
        {
            _navigating = ctx.nav.MoveTo(ctx.suspicionPosition, ctx.stats.SearchSpeed);

            // If the point is unreachable, fall back to waiting then patrolling
            if (!_navigating)
                ctx.searchWaitTimer = ctx.stats.SearchWaitTime;
        }
    }
}
