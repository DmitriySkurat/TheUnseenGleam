using PlatNav;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HSM
{
    public class EnemyPatrol : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        public EnemyPatrol(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.blue,
            });
        }

        protected override void OnEnter()
        {
            root.StopVisualChase();
            root.ResetWait();
            root.ResetManualCommand();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.patrolRoute == null || ctx.patrolRoute.Length == 0)
                return;

            if (TryHandlePatrolLightResponse())
                return;

            if (ctx.isWaiting)
            {
                if (!root.UpdateWaitTimer())
                    return;

                root.TryMoveTo(ctx.patrolRoute[ctx.patrolIndex], ctx.patrolSpeed);
            }

            if (!root.TryMoveTo(ctx.patrolRoute[ctx.patrolIndex], ctx.patrolSpeed))
            {
                AdvancePatrolIndex();
                root.BeginWait(ctx.patrolWaitTime);
                return;
            }

            if (root.HasCompletedManualMove())
            {
                AdvancePatrolIndex();
                root.BeginWait(ctx.patrolWaitTime);
            }
        }

        private bool TryHandlePatrolLightResponse()
        {
            if (ctx.lightSensor == null)
                return false;

            if (!ctx.lightSensor.IsBlinded(out Light2D strongestLight, out _))
                return false;

            bool isMirrorLight = strongestLight != null && strongestLight.GetComponent<MirrorLightSource>() != null;
            bool isStandingStill = ctx.isWaiting || (ctx.nav != null && ctx.nav.State == PlatNavState.Idle && !ctx.nav.HasPath);

            if (isMirrorLight && isStandingStill)
            {
                root.ResetManualCommand();
                return true;
            }

            if (isMirrorLight)
                return false;

            root.ResetWait();
            Vector2 escapeTarget = (Vector2)ctx.selfTransform.position + root.GetFacingDirection() * ctx.lightOverrunDistance;
            root.TryMoveTo(escapeTarget, ctx.lightEscapeSpeed);
            return true;
        }

        private void AdvancePatrolIndex()
        {
            if (ctx.patrolRoute == null || ctx.patrolRoute.Length <= 1)
            {
                ctx.patrolIndex = 0;
                return;
            }

            ctx.patrolIndex = EnemyRoot.GetNextBounceIndex(ctx.patrolIndex, ref ctx.patrolDirection, ctx.patrolRoute.Length);
        }
    }
}
