using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyInvestigateState : State
    {
        private readonly EnemyContext ctx;
        private bool _reached;
        private float _startTime;

        public EnemyInvestigateState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _reached = false;
            _startTime = Time.time;
            if (ctx == null) return;

            UpdateLastKnownFromNoise();
            if (ctx.hasLastKnownPlayerPosition)
            {
                ctx.movement?.MoveTo(ctx.lastKnownPlayerPosition);
            }
            if (ctx.debugStateLifecycle) ctx.Log("Investigate: Enter");
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (ctx.hasNoiseTarget)
            {
                if (ctx.noiseMemoryDuration > 0f && Time.time - ctx.lastNoiseTime > ctx.noiseMemoryDuration)
                {
                    ctx.hasNoiseTarget = false;
                }
                else
                {
                    UpdateLastKnownFromNoise();
                }
            }

            if (ctx.hasLastKnownPlayerPosition && ctx.self != null)
            {
                float dist = Vector2.Distance((Vector2)ctx.self.position, ctx.lastKnownPlayerPosition);
                float threshold = ctx.investigateStopDistance > 0f ? ctx.investigateStopDistance : 0.25f;
                if (dist <= threshold)
                {
                    _reached = true;
                    if (ctx.hasNoiseTarget) ctx.hasNoiseTarget = false;
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx?.movement?.Stop();
            if (ctx != null && ctx.debugStateLifecycle) ctx.Log("Investigate: Exit");
        }

        protected override State GetTransition()
        {
            if (ctx == null) return null;
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                if (ctx.debugTransitions) ctx.Log("Investigate -> Chase (vision)");
                return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            }
            if (!ctx.hasLastKnownPlayerPosition)
            {
                if (ctx.debugTransitions) ctx.Log("Investigate -> Patrol (no last known)");
                return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            }
            if (_reached)
            {
                if (ctx.debugTransitions) ctx.Log("Investigate -> Search (reached)");
                return Machine != null ? Machine.GetState<EnemySearchState>() : null;
            }
            if (ctx.investigateDuration > 0f && Time.time - _startTime >= ctx.investigateDuration)
            {
                if (ctx.debugTransitions) ctx.Log("Investigate -> Search (timeout)");
                return Machine != null ? Machine.GetState<EnemySearchState>() : null;
            }
            return null;
        }

        private void UpdateLastKnownFromNoise()
        {
            if (ctx == null || !ctx.hasNoiseTarget) return;
            ctx.lastKnownPlayerPosition = ctx.lastNoisePosition;
            ctx.hasLastKnownPlayerPosition = true;
            ctx.lastKnownPlayerTime = Time.time;
            ctx.movement?.FaceTowards(ctx.lastKnownPlayerPosition);
            ctx.movement?.MoveTo(ctx.lastKnownPlayerPosition);
        }
    }
}
