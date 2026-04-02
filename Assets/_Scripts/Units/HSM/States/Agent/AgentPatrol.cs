using PlatNav;
using UnityEngine;

namespace HSM {
    public class AgentPatrol : State
    {
        readonly AgentContext ctx;
        bool _navigating;

        public AgentPatrol(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            // Clear any tracked target so MoveTo uses the patrol position, not a Transform
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();

            EnsurePatrolPoints();
            ctx.patrolWaitTimer = 0f;
            _navigating = false;
            StartNavigatingToCurrentPoint();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.IsWaitingAtPoint)
            {
                ctx.patrolWaitTimer -= deltaTime;
                if (!ctx.IsWaitingAtPoint)
                    AdvanceToNextPoint();
            }
            else if (_navigating)
            {
                ctx.nav.Tick(deltaTime);
                if (ctx.nav.State == PlatNavState.Idle)
                {
                    _navigating = false;
                    ctx.patrolWaitTimer = ctx.stats.PatrolWaitTime;
                }
            }
            else
            {
                // Path failed on previous attempt — retry
                StartNavigatingToCurrentPoint();
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;

            if (ctx.pendingNoiseAlert)
                return Machine != null ? Machine.GetState<AgentSuspicious>() : null;

            return null;
        }

        void EnsurePatrolPoints()
        {
            if (ctx.PatrolCount > 0) return;

            float dist = ctx.stats != null ? ctx.stats.DefaultPatrolDistance : 4f;
            ctx.patrolPositions = new Vector2[]
            {
                ctx.spawnPosition + Vector2.left  * dist,
                ctx.spawnPosition + Vector2.right * dist,
            };
        }

        void StartNavigatingToCurrentPoint()
        {
            if (ctx.PatrolCount == 0) return;

            _navigating = ctx.nav.MoveTo(ctx.CurrentPatrolPosition, ctx.stats.PatrolSpeed);
        }

        void AdvanceToNextPoint()
        {
            ctx.currentPatrolIndex = (ctx.currentPatrolIndex + 1) % ctx.PatrolCount;
            StartNavigatingToCurrentPoint();
        }
    }
}
