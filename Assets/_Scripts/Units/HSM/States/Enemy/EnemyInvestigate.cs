using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyInvestigateState : State
    {
        private readonly EnemyContext ctx;
        private bool _reached;

        public EnemyInvestigateState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _reached = false;
            if (ctx == null) return;

            UpdateLastKnownFromNoise();
            if (ctx.hasLastKnownPlayerPosition)
            {
                ctx.movement?.MoveTo(ctx.lastKnownPlayerPosition);
            }
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
        }

        protected override State GetTransition()
        {
            if (ctx == null) return null;
            if (ctx.vision != null && ctx.vision.CanSeePlayer) return Machine != null ? Machine.GetState<EnemyChaseState>() : null;
            if (!ctx.hasLastKnownPlayerPosition) return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
            if (_reached) return Machine != null ? Machine.GetState<EnemySearchState>() : null;
            return null;
        }

        private void UpdateLastKnownFromNoise()
        {
            if (ctx == null || !ctx.hasNoiseTarget) return;
            ctx.lastKnownPlayerPosition = ctx.lastNoisePosition;
            ctx.hasLastKnownPlayerPosition = true;
            ctx.lastKnownPlayerTime = Time.time;
            ctx.movement?.MoveTo(ctx.lastKnownPlayerPosition);
        }
    }
}
