using UnityEngine;
using PlatNav;

namespace HSM
{
    public class EnemyChasing : State
    {
        private readonly EnemyContext _ctx;

        public EnemyChasing(StateMachine machine, State parent, EnemyContext ctx) : base(machine, parent)
        {
            _ctx = ctx;
        }

        protected override State GetInitialState() => null;

        protected override State GetTransition()
        {
            // Return to patrol if nothing is known about player
            if (!_ctx.hasKnownPlayerPosition)
            {
                return Machine.GetState<EnemySearching>();
            }

            return null;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _ctx.isWaiting = false;
            _ctx.waitTimer = 0f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_ctx.navHandler == null || _ctx.playerTransform == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            bool canSeePlayer = _ctx.vision != null && _ctx.vision.CanSeePlayer;

            if (canSeePlayer)
            {
                if (_ctx.vision != null && _ctx.vision.HasLastSeenPosition)
                {
                    _ctx.lastKnownPlayerPosition = _ctx.vision.LastSeenPosition;
                    _ctx.hasKnownPlayerPosition = true;
                }

                BeginVisualChase();
            }
            else
            {
                StopVisualChase();

                if (!_ctx.hasKnownPlayerPosition)
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                if (!TryMoveTo(_ctx.lastKnownPlayerPosition, _ctx.chaseSpeed))
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                if (HasCompletedManualMove())
                {
                    base.OnUpdate(deltaTime);
                    return;
                }
            }

            base.OnUpdate(deltaTime);
        }

        private void BeginVisualChase()
        {
            if (_ctx.navHandler == null || _ctx.playerTransform == null)
                return;

            if (_ctx.usingVisualChase)
                return;

            _ctx.navHandler.Abort();
            _ctx.navHandler.SetBehaviour(PlatNavBehaviour.FollowTarget);
            _ctx.navHandler.SetTarget(_ctx.playerTransform);
            _ctx.navHandler.MoveTo(_ctx.playerTransform.position, _ctx.chaseSpeed);
            _ctx.usingVisualChase = true;
            ResetManualCommand();
        }

        private void StopVisualChase()
        {
            if (_ctx.navHandler == null)
                return;

            _ctx.navHandler.SetTarget(null);
            if (_ctx.usingVisualChase)
                _ctx.navHandler.Abort();

            _ctx.usingVisualChase = false;
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
