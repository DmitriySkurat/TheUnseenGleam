using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyInvestigate : State
    {
        private readonly EnemyContext ctx;

        public EnemyInvestigate(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            if (ctx?.pathfinder == null) return;

            if (ctx.noiseTarget != null) ctx.pathfinder.SetTarget(ctx.noiseTarget);
            ctx.pathfinder.tryFindPath = ctx.noiseTarget != null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx?.pathfinder != null && ctx.noiseTarget != null && ctx.hasNoiseTarget)
            {
                // Clear stale noise or stop when close enough.
                if (ctx.noiseMemoryDuration > 0f && Time.time - ctx.lastNoiseTime > ctx.noiseMemoryDuration)
                {
                    ClearNoiseTarget();
                }
                else if (ctx.self != null)
                {
                    float dist = Vector2.Distance((Vector2)ctx.self.position, (Vector2)ctx.noiseTarget.position);
                    if (dist <= ctx.investigateStopDistance)
                    {
                        ClearNoiseTarget();
                    }
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            if (ctx?.pathfinder != null) ctx.pathfinder.tryFindPath = false;
        }

        protected override State GetTransition()
        {
            if (CanChase()) return Machine != null ? Machine.GetState<EnemyChase>() : null;
            if (ctx == null || !ctx.hasNoiseTarget) return Machine != null ? Machine.GetState<EnemyIdle>() : null;
            return null;
        }

        private bool CanChase()
        {
            if (ctx == null || ctx.target == null || ctx.self == null) return false;
            float distance = Vector3.Distance(ctx.self.position, ctx.target.position);
            return distance <= ctx.detectRange;
        }

        private void ClearNoiseTarget()
        {
            ctx.hasNoiseTarget = false;
            if (ctx.pathfinder != null) ctx.pathfinder.tryFindPath = false;
        }
    }
}
