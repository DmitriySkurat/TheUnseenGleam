using UnityEngine;

namespace HSM
{
    public class EnemyInvestigating : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        public EnemyInvestigating(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.orange,
            });
        }

        protected override void OnEnter()
        {
            root.StopVisualChase();
            root.ResetWait();
            root.ForceMoveTo(ctx.investigationTarget, ctx.investigateSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.isWaiting)
            {
                if (!root.UpdateWaitTimer())
                    return;

                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (!root.TryMoveTo(ctx.investigationTarget, ctx.investigateSpeed))
            {
                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (root.HasCompletedManualMove())
                root.BeginWait(ctx.investigateWaitTime);
        }
    }
}
