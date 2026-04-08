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
            // Debug behavior: always use exact player position (no LOS dependency).
            if (ctx.playerTransform != null)
            {
                ctx.suspicionPosition = ctx.playerTransform.position;
                ctx.chaseVisionLostTimer = 0f;
                ctx.nav.SetTarget(ctx.playerTransform);
                if (ctx.playerRb != null)
                    ctx.predictionPlayerVelocity = ctx.playerRb.linearVelocity;
            }
            // Fallback to old behavior if player transform is unavailable.
            else if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                ctx.suspicionPosition         = ctx.vision.LastSeenPosition;
                ctx.chaseVisionLostTimer      = 0f;
                if (ctx.playerRb != null)
                    ctx.predictionPlayerVelocity = ctx.playerRb.linearVelocity;
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
            // Debug mode: stay in AgentChase permanently.
            return null;
        }
    }
}
