using UnityEngine;
using PlatNav;

namespace HSM
{
    public class EnemyInvestigating : State
    {
        private readonly EnemyContext _ctx;

        public EnemyInvestigating(StateMachine machine, State parent, EnemyContext ctx) : base(machine, parent)
        {
            _ctx = ctx;
        }

        protected override State GetInitialState() => null;

        protected override State GetTransition()
        {
            // If player is visible, transition to chasing
            if (_ctx.vision != null && _ctx.vision.CanSeePlayer && _ctx.playerTransform != null)
            {
                return Machine.GetState<EnemyChasing>();
            }

            // If investigation is complete, return to patrol
            if (!_ctx.isWaiting && HasArrivedAtTarget())
            {
                return Machine.GetState<EnemyPatrol>();
            }

            return null;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _ctx.isWaiting = false;
            _ctx.waitTimer = 0f;
            ForceMoveTo(_ctx.investigationTarget, _ctx.investigateSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_ctx.navHandler == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (_ctx.isWaiting)
            {
                if (!UpdateWaitTimer())
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                base.OnUpdate(deltaTime);
                return;
            }

            if (!TryMoveTo(_ctx.investigationTarget, _ctx.investigateSpeed))
            {
                BeginWait(_ctx.investigateWaitTime);
            }
            else if (HasCompletedManualMove())
            {
                BeginWait(_ctx.investigateWaitTime);
            }

            base.OnUpdate(deltaTime);
        }

        private bool HasArrivedAtTarget()
        {
            if (_ctx.navHandler == null)
                return false;

            return _ctx.navHandler.State == PlatNavState.Idle && !_ctx.navHandler.HasPath && !_ctx.manualCommandActive;
        }

        private void BeginWait(float duration)
        {
            _ctx.isWaiting = duration > 0f;
            _ctx.waitTimer = duration;
            ResetManualCommand();
        }

        private bool UpdateWaitTimer()
        {
            if (!_ctx.isWaiting)
                return false;

            _ctx.waitTimer -= Time.deltaTime;
            if (_ctx.waitTimer > 0f)
                return false;

            _ctx.isWaiting = false;
            _ctx.waitTimer = 0f;
            return true;
        }

        private bool TryMoveTo(Vector2 targetPosition, float speed)
        {
            if (_ctx.navHandler == null)
                return false;

            float minRetargetDistance = Mathf.Max(0.01f, _ctx.retargetDistance);
            bool needsNewCommand = !_ctx.manualCommandActive
                || _ctx.manualCommandFailed
                || (_ctx.manualDestination - targetPosition).sqrMagnitude > minRetargetDistance * minRetargetDistance;

            if (!needsNewCommand)
                return !_ctx.manualCommandFailed;

            ForceMoveTo(targetPosition, speed);
            return !_ctx.manualCommandFailed;
        }

        private void ForceMoveTo(Vector2 targetPosition, float speed)
        {
            if (_ctx.navHandler == null)
                return;

            _ctx.navHandler.SetBehaviour(PlatNavBehaviour.FollowTarget);
            _ctx.navHandler.SetTarget(null);
            _ctx.navHandler.Abort();
            _ctx.usingVisualChase = false;

            _ctx.manualDestination = targetPosition;
            _ctx.manualCommandFailed = !_ctx.navHandler.MoveTo(targetPosition, speed);
            _ctx.manualCommandActive = !_ctx.manualCommandFailed;
        }

        private bool HasCompletedManualMove()
        {
            if (!_ctx.manualCommandActive || _ctx.navHandler == null)
                return false;

            if (_ctx.navHandler.HasPath || _ctx.navHandler.State != PlatNavState.Idle)
                return false;

            ResetManualCommand();
            return true;
        }

        private void ResetManualCommand()
        {
            _ctx.manualCommandActive = false;
            _ctx.manualCommandFailed = false;
        }
    }
}
