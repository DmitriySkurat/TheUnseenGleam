using UnityEngine;

namespace HSM
{
    public class EnemyInvestigating : State
    {
        private readonly EnemyContext ctx;

        public EnemyInvestigating(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.orange, 
            });
        }

        protected override void OnEnter()
        {
            ctx.StopVisualChase();
            ctx.ResetWait();
            ctx.ForceMoveTo(ctx.investigationTarget, ctx.investigateSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.isWaiting)
            {
                if (!ctx.UpdateWaitTimer())
                    return;

                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (!ctx.TryMoveTo(ctx.investigationTarget, ctx.investigateSpeed))
            {
                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (ctx.HasCompletedManualMove())
                ctx.BeginWait(ctx.investigateWaitTime);
        }
    }
}
