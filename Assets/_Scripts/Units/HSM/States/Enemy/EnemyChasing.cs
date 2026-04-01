using UnityEngine;

namespace HSM
{
    public class EnemyChasing : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        public EnemyChasing(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.red,
            });
        }

        protected override void OnEnter()
        {
            root.ResetWait();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool canSeePlayer = ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null;

            if (canSeePlayer)
            {
                root.BeginVisualChase();
                return;
            }

            if (!ctx.hasKnownPlayerPosition)
            {
                BeginSearch(ctx.selfTransform.position);
                return;
            }

            if (!root.TryMoveTo(ctx.lastKnownPlayerPosition, ctx.chaseSpeed))
            {
                BeginSearch(ctx.lastKnownPlayerPosition);
                return;
            }

            if (root.HasCompletedManualMove())
                BeginSearch(ctx.lastKnownPlayerPosition);
        }

        private void BeginSearch(Vector2 center)
        {
            root.StopVisualChase();
            ctx.searchCenter = center;
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemySearching>());
        }
    }
}
