using UnityEngine;

namespace HSM
{
    public class EnemyPatrol : State
    {
        private readonly EnemyContext ctx;

        public EnemyPatrol(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.blue,  
            });
        }

        protected override void OnEnter()
        {
            ctx.StopVisualChase();
            ctx.ResetWait();
            ctx.ResetManualCommand();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.patrolRoute == null || ctx.patrolRoute.Length == 0)
                return;

            if (ctx.TryHandlePatrolLightResponse())
                return;

            if (ctx.isWaiting)
            {
                if (!ctx.UpdateWaitTimer())
                    return;

                ctx.TryMoveTo(ctx.patrolRoute[ctx.patrolIndex], ctx.patrolSpeed);
            }

            if (!ctx.TryMoveTo(ctx.patrolRoute[ctx.patrolIndex], ctx.patrolSpeed))
            {
                ctx.AdvancePatrolIndex();
                ctx.BeginWait(ctx.patrolWaitTime);
                return;
            }

            if (ctx.HasCompletedManualMove())
            {
                ctx.AdvancePatrolIndex();
                ctx.BeginWait(ctx.patrolWaitTime);
            }
        }
    }
}
