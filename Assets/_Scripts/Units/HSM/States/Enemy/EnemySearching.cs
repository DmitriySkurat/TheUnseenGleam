using UnityEngine;

namespace HSM
{
    public class EnemySearching : State
    {
        private readonly EnemyContext ctx;

        public EnemySearching(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.StopVisualChase();
            ctx.searchTimer = ctx.searchDuration;
            ctx.ResetWait();

            Vector2 offset = Vector2.right * ctx.searchHalfWidth;
            ctx.searchRoute[0] = ctx.AdjustPositionAwayFromLight(ctx.searchCenter - offset);
            ctx.searchRoute[1] = ctx.AdjustPositionAwayFromLight(ctx.searchCenter + offset);
            ctx.searchDirection = 1;
            ctx.searchIndex = EnemyContext.GetClosestIndex(ctx.searchRoute, ctx.selfTransform.position);

            ctx.ForceMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.searchTimer -= Time.deltaTime;
            if (ctx.searchTimer <= 0f)
            {
                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (ctx.isWaiting)
            {
                if (!ctx.UpdateWaitTimer())
                    return;

                ctx.TryMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed);
            }

            if (!ctx.TryMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed))
            {
                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (ctx.HasCompletedManualMove())
            {
                ctx.AdvanceSearchIndex();
                ctx.BeginWait(ctx.searchWaitTime);
            }
        }
    }
}
