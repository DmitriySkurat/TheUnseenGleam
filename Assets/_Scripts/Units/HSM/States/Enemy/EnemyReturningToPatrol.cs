namespace HSM
{
    public class EnemyReturningToPatrol : State
    {
        private readonly EnemyContext ctx;

        public EnemyReturningToPatrol(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.StopVisualChase();
            ctx.ResetWait();
            ctx.returnTarget = ctx.GetPatrolReturnPoint();
            ctx.ForceMoveTo(ctx.returnTarget, ctx.returnSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!ctx.TryMoveTo(ctx.returnTarget, ctx.returnSpeed))
            {
                EnterPatrol();
                return;
            }

            if (!ctx.HasCompletedManualMove())
                return;

            ctx.patrolDirection = 1;
            ctx.patrolIndex = ctx.patrolRoute != null && ctx.patrolRoute.Length > 1 ? 1 : 0;
            ctx.BeginWait(ctx.patrolWaitTime);
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyPatrol>());
        }

        private void EnterPatrol()
        {
            ctx.patrolDirection = 1;
            ctx.patrolIndex = ctx.GetInitialPatrolIndex();
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyPatrol>());
        }
    }
}
