namespace HSM {
    public class AgentChase : State
    {
        readonly AgentContext ctx;

        public AgentChase(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.chaseVisionLostTimer = 0f;

            // Set speed once; auto-repath in Tick will keep using this value
            ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
            // Hand the player transform to PlatNavHandler so it re-paths automatically
            ctx.nav.SetTarget(ctx.playerTransform);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Keep suspicionPosition up-to-date so AgentSearch knows where to go
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                ctx.suspicionPosition    = ctx.vision.LastSeenPosition;
                ctx.chaseVisionLostTimer = 0f;
            }
            else
            {
                ctx.chaseVisionLostTimer += deltaTime;
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
            if (ctx.chaseVisionLostTimer >= ctx.stats.ChaseVisionGraceTime)
                return Machine != null ? Machine.GetState<AgentSearch>() : null;

            return null;
        }
    }
}