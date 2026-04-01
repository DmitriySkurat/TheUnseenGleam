using UnityEngine;

namespace HSM
{
    public class EnemyReturningToPatrol : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        public EnemyReturningToPatrol(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.cyan,
            });
        }

        protected override void OnEnter()
        {
            root.StopVisualChase();
            root.ResetWait();
            ctx.returnTarget = GetPatrolReturnPoint();
            root.ForceMoveTo(ctx.returnTarget, ctx.returnSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!root.TryMoveTo(ctx.returnTarget, ctx.returnSpeed))
            {
                EnterPatrol();
                return;
            }

            if (!root.HasCompletedManualMove())
                return;

            ctx.patrolDirection = 1;
            ctx.patrolIndex = ctx.patrolRoute != null && ctx.patrolRoute.Length > 1 ? 1 : 0;
            root.BeginWait(ctx.patrolWaitTime);
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyPatrol>());
        }

        private void EnterPatrol()
        {
            ctx.patrolDirection = 1;
            ctx.patrolIndex = root.GetInitialPatrolIndex();
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyPatrol>());
        }

        private Vector2 GetPatrolReturnPoint()
        {
            if (ctx.patrolPoints != null && ctx.patrolPoints.Length > 0 && ctx.patrolPoints[0] != null)
                return ctx.patrolPoints[0].position;

            return ctx.patrolOrigin;
        }
    }
}
