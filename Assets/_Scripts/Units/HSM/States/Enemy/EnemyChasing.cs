using UnityEngine;

namespace HSM
{
    public class EnemyChasing : State
    {
        private readonly EnemyContext ctx;

        public EnemyChasing(StateMachine m, State parent, EnemyContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red, 
            });
        }

        protected override void OnEnter()
        {
            ctx.ResetWait();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool canSeePlayer = ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null;

            if (canSeePlayer)
            {
                ctx.BeginVisualChase();
                return;
            }

            if (!ctx.hasKnownPlayerPosition)
            {
                BeginSearch(ctx.selfTransform.position);
                return;
            }

            if (!ctx.TryMoveTo(ctx.lastKnownPlayerPosition, ctx.chaseSpeed))
            {
                BeginSearch(ctx.lastKnownPlayerPosition);
                return;
            }

            if (ctx.HasCompletedManualMove())
                BeginSearch(ctx.lastKnownPlayerPosition);
        }

        private void BeginSearch(UnityEngine.Vector2 center)
        {
            ctx.StopVisualChase();
            ctx.searchCenter = center;
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemySearching>());
        }
    }
}
