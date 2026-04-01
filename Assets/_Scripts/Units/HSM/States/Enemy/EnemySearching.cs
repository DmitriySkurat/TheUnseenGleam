using UnityEngine;

namespace HSM
{
    public class EnemySearching : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        public EnemySearching(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.yellow,
            });
        }

        protected override void OnEnter()
        {
            root.StopVisualChase();
            ctx.searchTimer = ctx.searchDuration;
            root.ResetWait();

            Vector2 offset = Vector2.right * ctx.searchHalfWidth;
            ctx.searchRoute[0] = AdjustPositionAwayFromLight(ctx.searchCenter - offset);
            ctx.searchRoute[1] = AdjustPositionAwayFromLight(ctx.searchCenter + offset);
            ctx.searchDirection = 1;
            ctx.searchIndex = EnemyRoot.GetClosestIndex(ctx.searchRoute, ctx.selfTransform.position);

            root.ForceMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed);
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
                if (!root.UpdateWaitTimer())
                    return;

                root.TryMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed);
            }

            if (!root.TryMoveTo(ctx.searchRoute[ctx.searchIndex], ctx.searchSpeed))
            {
                Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
                return;
            }

            if (root.HasCompletedManualMove())
            {
                AdvanceSearchIndex();
                root.BeginWait(ctx.searchWaitTime);
            }
        }

        private void AdvanceSearchIndex()
        {
            ctx.searchIndex = EnemyRoot.GetNextBounceIndex(ctx.searchIndex, ref ctx.searchDirection, ctx.searchRoute.Length);
        }

        private Vector2 AdjustPositionAwayFromLight(Vector2 position)
        {
            if (ctx.lightSensor == null || !ctx.lightSensor.IsBlindedAt(position, out _, out _))
                return position;

            Vector2 dir = (position - (Vector2)ctx.selfTransform.position).normalized;
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector2.right;

            Vector2 adjusted = position + dir * ctx.lightOverrunDistance;

            if (ctx.lightSensor.IsBlindedAt(adjusted, out _, out _))
            {
                Vector2 perp = new Vector2(-dir.y, dir.x);
                adjusted = position + perp * ctx.lightOverrunDistance;
            }

            return adjusted;
        }
    }
}
