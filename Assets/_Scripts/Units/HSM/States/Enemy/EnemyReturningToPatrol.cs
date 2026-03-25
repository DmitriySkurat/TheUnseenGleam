using UnityEngine;
using PlatNav;

namespace HSM
{
    public class EnemyReturningToPatrol : State
    {
        private readonly EnemyContext _ctx;

        public EnemyReturningToPatrol(StateMachine machine, State parent, EnemyContext ctx) : base(machine, parent)
        {
            _ctx = ctx;
        }

        protected override State GetInitialState() => null;

        protected override State GetTransition()
        {
            // Return to patrol when arrival is confirmed
            if (!_ctx.manualCommandActive && _ctx.navHandler != null && _ctx.navHandler.State == PlatNavState.Idle)
            {
                return Machine.GetState<EnemyPatrol>();
            }

            // If player is visible during return, cancel and chase
            if (_ctx.vision != null && _ctx.vision.CanSeePlayer && _ctx.playerTransform != null)
            {
                return Machine.GetState<EnemyChasing>();
            }

            return null;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            if (_ctx.navHandler == null)
                return;

            // Reset patrol index and direction
            _ctx.patrolDirection = 1;
            _ctx.patrolIndex = GetInitialPatrolIndex();

            // Get return point
            if (_ctx.patrolRoute == null || _ctx.patrolRoute.Length == 0)
            {
                _ctx.returnTarget = _ctx.patrolOrigin;
            }
            else
            {
                _ctx.returnTarget = _ctx.patrolRoute[0];
            }

            ForceMoveTo(_ctx.returnTarget, _ctx.returnSpeed);
            ResetManualCommand();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_ctx.navHandler == null)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            if (!TryMoveTo(_ctx.returnTarget, _ctx.returnSpeed))
            {
                base.OnUpdate(deltaTime);
                return;
            }

            base.OnUpdate(deltaTime);
        }

        private int GetInitialPatrolIndex()
        {
            if (_ctx.patrolRoute == null || _ctx.patrolRoute.Length == 0)
                return 0;

            return GetClosestIndex(_ctx.patrolRoute, _ctx.transform != null ? _ctx.transform.position : Vector2.zero);
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

        private void ResetManualCommand()
        {
            _ctx.manualCommandActive = false;
            _ctx.manualCommandFailed = false;
        }

        private static int GetClosestIndex(Vector2[] points, Vector2 worldPosition)
        {
            int bestIndex = 0;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < points.Length; i++)
            {
                float sqrDistance = (points[i] - worldPosition).sqrMagnitude;
                if (sqrDistance >= bestDistance)
                    continue;

                bestDistance = sqrDistance;
                bestIndex = i;
            }

            return bestIndex;
        }
    }
}
