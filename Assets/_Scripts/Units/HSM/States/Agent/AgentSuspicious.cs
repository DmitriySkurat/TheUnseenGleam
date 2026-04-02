namespace HSM {
    public class AgentSuspicious : State
    {
        readonly AgentContext ctx;

        public AgentSuspicious(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            // Stop all movement immediately
            ctx.nav.Abort();

            // Determine what triggered suspicion and configure the window
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                ctx.suspicionSource           = SuspicionSource.Vision;
                ctx.suspicionTimer            = ctx.stats.SuspicionTimeOnSight;
                ctx.suspicionPosition         = ctx.vision.LastSeenPosition;
                ctx.suspicionNoiseLoudness    = 0f;
                ctx.noisesHeardDuringSuspicion = 0;
            }
            else if (ctx.pendingNoiseAlert)
            {
                ctx.suspicionSource            = SuspicionSource.Noise;
                ctx.suspicionTimer             = ctx.stats.SuspicionTimeOnNoise;
                ctx.suspicionPosition          = ctx.pendingNoisePosition;
                ctx.suspicionNoiseLoudness     = ctx.pendingNoiseLoudness;
                ctx.noisesHeardDuringSuspicion = 1;
            }

            // Consume the pending noise that triggered the transition
            ctx.pendingNoiseAlert = false;

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Absorb any new noise events that arrive during the suspicion window
            if (ctx.pendingNoiseAlert)
            {
                ctx.noisesHeardDuringSuspicion++;
                if (ctx.pendingNoiseLoudness > ctx.suspicionNoiseLoudness)
                    ctx.suspicionNoiseLoudness = ctx.pendingNoiseLoudness;
                ctx.pendingNoiseAlert = false;
            }

            // If triggered by noise but player becomes visible — upgrade to vision suspicion
            if (ctx.suspicionSource == SuspicionSource.Noise &&
                ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                ctx.suspicionSource   = SuspicionSource.Vision;
                ctx.suspicionTimer    = ctx.stats.SuspicionTimeOnSight;
                ctx.suspicionPosition = ctx.vision.LastSeenPosition;
            }

            if (ctx.suspicionTimer > 0f)
                ctx.suspicionTimer -= deltaTime;

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            // Still waiting out the suspicion window
            if (ctx.suspicionTimer > 0f) return null;

            switch (ctx.suspicionSource)
            {
                case SuspicionSource.Vision:
                    // Player still visible → pursue
                    if (ctx.vision != null && ctx.vision.CanSeePlayer)
                        return Machine != null ? Machine.GetState<AgentChase>() : null;
                    // Lost sight → return to patrol
                    return Machine != null ? Machine.GetState<AgentPatrol>() : null;

                case SuspicionSource.Noise:
                    bool isLoud     = ctx.suspicionNoiseLoudness >= ctx.stats.LoudNoiseThreshold;
                    bool isRepeated = ctx.noisesHeardDuringSuspicion > 1;
                    // Loud or repeated noise → investigate
                    if (isLoud || isRepeated)
                        return Machine != null ? Machine.GetState<AgentSearch>() : null;
                    // Quiet single noise → forget it
                    return Machine != null ? Machine.GetState<AgentPatrol>() : null;

                default:
                    return Machine != null ? Machine.GetState<AgentPatrol>() : null;
            }
        }
    }
}
