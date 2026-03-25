using UnityEngine;
using PlatNav;

namespace HSM
{
    public class EnemySearching : State
    {
        private readonly EnemyContext _ctx;

        public EnemySearching(StateMachine machine, State parent, EnemyContext ctx) : base(machine, parent)
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

            // If search duration expired, return to patrol
            if (_ctx.searchTimer <= 0f)
            {
                return Machine.GetState<EnemyPatrol>();
            }

            return null;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _ctx.searchTimer = _ctx.searchDuration;
            _ctx.isWaiting = false;
            _ctx.waitTimer = 0f;

            Vector2 offset = Vector2.right * _ctx.searchHalfWidth;
            Vector2 searchCenter = _ctx.lastKnownPlayerPosition;
            _ctx.searchRoute[0] = searchCenter - offset;
            _ctx.searchRoute[1] = searchCenter + offset;
            _ctx.searchDirection = 1;
            _ctx.searchIndex = GetClosestIndex(_ctx.searchRoute, _ctx.transform != null ? _ctx.transform.position : searchCenter);

            ForceMoveTo(_ctx.searchRoute[_ctx.searchIndex], _ctx.searchSpeed);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_ctx.navHandler == null)
            {
                _ctx.searchTimer -= deltaTime;
                base.OnUpdate(deltaTime);
                return;
            }

            _ctx.searchTimer -= deltaTime;

            if (_ctx.isWaiting)
            {
                if (!UpdateWaitTimer())
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                AdvanceSearchIndex();
                ForceMoveTo(_ctx.searchRoute[_ctx.searchIndex], _ctx.searchSpeed);
                base.OnUpdate(deltaTime);
                return;
            }

            if (!TryMoveTo(_ctx.searchRoute[_ctx.searchIndex], _ctx.searchSpeed))
            {
                BeginWait(_ctx.searchWaitTime);
            }
            else if (HasCompletedManualMove())
            {
                BeginWait(_ctx.searchWaitTime);
            }

            base.OnUpdate(deltaTime);
        }

        private void AdvanceSearchIndex()
        {
            _ctx.searchIndex = GetNextBounceIndex(_ctx.searchIndex, ref _ctx.searchDirection, _ctx.searchRoute.Length);
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

        private static int GetNextBounceIndex(int currentIndex, ref int direction, int length)
        {
            if (length <= 1)
                return 0;

            int nextIndex = currentIndex + direction;
            if (nextIndex >= length || nextIndex < 0)
            {
                direction *= -1;
                nextIndex = currentIndex + direction;
            }

            return Mathf.Clamp(nextIndex, 0, length - 1);
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
