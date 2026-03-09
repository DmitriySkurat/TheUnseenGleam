using UnityEngine;
using Entity.Enemy;

namespace HSM
{
    public class EnemyChaseState : State
    {
        private readonly EnemyContext ctx;

        public EnemyChaseState(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            if (ctx == null) return;
            if (ctx.player != null)
            {
                ctx.movement?.MoveTo(ctx.player);
            }
            else
            {
                ctx.movement?.Stop();
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx != null)
            {
                if (ctx.vision != null && ctx.vision.CanSeePlayer && ctx.player != null)
                {
                    ctx.lastKnownPlayerPosition = ctx.player.position;
                    ctx.hasLastKnownPlayerPosition = true;
                    ctx.lastKnownPlayerTime = Time.time;
                }
                if (ctx.player != null) ctx.movement?.MoveTo(ctx.player);
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
            if (ctx.vision != null && ctx.vision.CanSeePlayer) return null;

            if (ctx.hasLastKnownPlayerPosition || ctx.hasNoiseTarget)
                return Machine != null ? Machine.GetState<EnemyInvestigateState>() : null;

            return Machine != null ? Machine.GetState<EnemyPatrolState>() : null;
        }
    }
}
