using UnityEngine;
using PlatNav;
using UnityEngine.Rendering.Universal;

namespace HSM
{
    public class EnemyPatrol : State
    {
        private readonly EnemyContext _ctx;

        public EnemyPatrol(StateMachine machine, State parent, EnemyContext ctx) : base(machine, parent)
        {
            _ctx = ctx;
        }

        protected override State GetInitialState() => null;

        protected override State GetTransition()
        {
            // Check if player is visible - transition to Chase
            if (_ctx.vision != null && _ctx.vision.CanSeePlayer && _ctx.playerTransform != null)
            {
                return Machine.GetState<EnemyChasing>();
            }

            return null;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_ctx.navHandler == null || _ctx.patrolRoute == null || _ctx.patrolRoute.Length == 0)
            {
                base.OnUpdate(deltaTime);
                return;
            }

            // Handle light response
            if (TryHandlePatrolLightResponse())
            {
                base.OnUpdate(deltaTime);
                return;
            }

            // Handle waiting at patrol point
            if (_ctx.isWaiting)
            {
                if (!UpdateWaitTimer())
                {
                    base.OnUpdate(deltaTime);
                    return;
                }

                TryMoveTo(_ctx.patrolRoute[_ctx.patrolIndex], _ctx.patrolSpeed);
            }

            // Move to patrol point
            if (!TryMoveTo(_ctx.patrolRoute[_ctx.patrolIndex], _ctx.patrolSpeed))
            {
                AdvancePatrolIndex();
                BeginWait(_ctx.patrolWaitTime);
            }
            else if (HasCompletedManualMove())
            {
                AdvancePatrolIndex();
                BeginWait(_ctx.patrolWaitTime);
            }

            base.OnUpdate(deltaTime);
        }

        private void AdvancePatrolIndex()
        {
            if (_ctx.patrolRoute == null || _ctx.patrolRoute.Length <= 1)
            {
                _ctx.patrolIndex = 0;
                return;
            }

            _ctx.patrolIndex = GetNextBounceIndex(_ctx.patrolIndex, ref _ctx.patrolDirection, _ctx.patrolRoute.Length);
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

        private bool TryHandlePatrolLightResponse()
        {
            if (_ctx.lightSensor == null)
                return false;

            if (!_ctx.lightSensor.IsBlinded(out Light2D strongestLight, out _))
                return false;

            bool isMirrorLight = strongestLight != null && strongestLight.GetComponent<MirrorLightSource>() != null;
            bool isStandingStill = _ctx.isWaiting || (_ctx.navHandler != null && _ctx.navHandler.State == PlatNavState.Idle && !_ctx.navHandler.HasPath);

            if (isMirrorLight && isStandingStill)
            {
                ResetManualCommand();
                return true;
            }

            if (isMirrorLight)
                return false;

            _ctx.isWaiting = false;
            _ctx.waitTimer = 0f;
            Vector2 escapeTarget = (_ctx.transform != null ? (Vector2)_ctx.transform.position : Vector2.zero) + GetFacingDirection() * _ctx.lightEscapeDistance;
            TryMoveTo(escapeTarget, _ctx.lightEscapeSpeed);
            return true;
        }

        private Vector2 GetFacingDirection()
        {
            if (_ctx.transform == null)
                return Vector2.right;
            return _ctx.transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
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
    }
}
