using PlatNav;
using UnityEngine;

namespace HSM {
    /// <summary>
    /// Entered from AgentChase when the agent loses sight of the player.
    /// Phase 1 (GoToLastSeen): Navigates to the last position the player was seen.
    /// Phase 2 (GoToPredicted): Uses the player's last known velocity to infer the most
    ///   likely escape route (straight > down > up) and navigates to that predicted position.
    /// If the predicted position is reached (or no path exists, or the timer expires) → AgentSearch.
    /// If the player is spotted again → AgentChase.
    /// </summary>
    public class AgentPredictionChase : State
    {
        readonly AgentContext ctx;

        enum Phase { GoToLastSeen, GoToPredicted }
        Phase _phase;
        bool  _navigating;

        public AgentPredictionChase(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.predictionTimer = ctx.stats.PredictionSearchTime;
            _phase              = Phase.GoToLastSeen;
            _navigating         = ctx.nav.MoveTo(ctx.suspicionPosition, ctx.stats.ChaseSpeed);

            // If we can't even reach lastSeen, skip straight to prediction
            if (!_navigating)
                ComputeAndNavigateToPredicted();

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.predictionTimer -= deltaTime;

            if (_phase == Phase.GoToLastSeen)
            {
                if (_navigating)
                {
                    ctx.nav.Tick(deltaTime);
                    if (ctx.nav.State == PlatNavState.Idle)
                        _navigating = false;
                }

                if (!_navigating)
                    ComputeAndNavigateToPredicted();
            }
            else if (_phase == Phase.GoToPredicted && _navigating)
            {
                ctx.nav.Tick(deltaTime);
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
                return Machine?.GetState<AgentChase>();

            if (ctx.predictionTimer <= 0f)
            {
                ctx.suspicionPosition = ctx.transform.position;
                return Machine?.GetState<AgentSearch>();
            }

            // GoToPredicted: path not found OR arrived at predicted position → Search
            if (_phase == Phase.GoToPredicted && (!_navigating || ctx.nav.State == PlatNavState.Idle))
                return Machine?.GetState<AgentSearch>();

            return null;
        }

        void ComputeAndNavigateToPredicted()
        {
            _phase = Phase.GoToPredicted;

            float dir = ctx.predictionPlayerVelocity.x;
            if (Mathf.Abs(dir) < 0.01f)
            {
                // No horizontal direction info — fall through to Search
                _navigating = false;
                return;
            }

            Vector2 predicted;
            if (ctx.nav.TryGetPredictedDestination(ctx.suspicionPosition, dir, out predicted))
            {
                // Update suspicionPosition so AgentSearch wanders around the predicted spot
                ctx.suspicionPosition = predicted;
                _navigating = ctx.nav.MoveTo(predicted, ctx.stats.ChaseSpeed);
            }
            else
            {
                // Wall — no walkable exit. AgentSearch will wander around the current suspicionPosition.
                _navigating = false;
            }
        }
    }
}
