using System.Collections.Generic;
using PlatNav;
using UnityEngine;

namespace HSM
{
    public class EnemyRoot : State
    {
        public readonly EnemyPatrol Patrol;
        public readonly EnemyChasing Chasing;
        public readonly EnemyInvestigating Investigating;
        public readonly EnemySearching Searching;
        public readonly EnemyReturningToPatrol ReturningToPatrol;
        public readonly EnemyLightEscape LightEscape;

        internal readonly EnemyContext ctx;

        public EnemyRoot(StateMachine m, EnemyContext ctx) : base(m, null)
        {
            this.ctx = ctx;
            Patrol = new EnemyPatrol(m, this);
            Chasing = new EnemyChasing(m, this);
            Investigating = new EnemyInvestigating(m, this);
            Searching = new EnemySearching(m, this);
            ReturningToPatrol = new EnemyReturningToPatrol(m, this);
            LightEscape = new EnemyLightEscape(m, this);
        }

        protected override State GetInitialState() => Patrol;

        protected override void OnEnter()
        {
            if (ctx.hearing != null)
                ctx.hearing.OnHeard += HandleHeard;
        }

        protected override void OnExit()
        {
            if (ctx.hearing != null)
                ctx.hearing.OnHeard -= HandleHeard;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (ctx.IsTraversingLink)
            {
                if (ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null)
                    UpdateKnownPlayerPosition(ctx.playerTransform.position);
                return;
            }

            if (ActiveChild != null)
                ActiveChild.Update(deltaTime);

            if (Machine?.Sequencer == null || Machine.Sequencer.IsTransitioning)
                return;

            var lightEscape = Machine.GetState<EnemyLightEscape>();
            bool isEscaping = Machine.Root.Leaf() == lightEscape;
            if (!isEscaping && ctx.lightSensor != null && ctx.lightSensor.IsBlinded(out _, out _))
            {
                Machine.Sequencer.RequestTransition(Machine.Root.Leaf(), lightEscape);
                return;
            }

            if (isEscaping)
                return;

            bool canSeePlayer = ctx.vision != null && ctx.vision.CanSeePlayer && ctx.playerTransform != null;
            if (!canSeePlayer)
                return;

            UpdateKnownPlayerPosition(ctx.playerTransform.position);

            var chasing = Machine.GetState<EnemyChasing>();
            if (Machine.Root.Leaf() == chasing)
                return;

            BeginVisualChase();
            Machine.Sequencer.RequestTransition(Machine.Root.Leaf(), chasing);
        }

        private void HandleHeard(NoiseEvent noiseEvent, float loudness)
        {
            if (noiseEvent.Source == null || noiseEvent.Source == ctx.selfTransform.gameObject)
                return;

            bool isPlayerNoise = ctx.playerTransform != null
                && noiseEvent.Source == ctx.playerTransform.gameObject;

            if (isPlayerNoise && ctx.hasDetectedPlayer)
            {
                UpdateKnownPlayerPosition(noiseEvent.Position);

                if (ctx.IsTraversingLink)
                    return;

                Machine.Sequencer.RequestTransition(
                    Machine.Root.Leaf(),
                    Machine.GetState<EnemyChasing>());
                return;
            }

            if (Machine.Root.Leaf() is EnemyChasing && !isPlayerNoise)
                return;

            if (ctx.IsTraversingLink)
                return;

            ctx.investigationTarget = noiseEvent.Position;
            Machine.Sequencer.RequestTransition(
                Machine.Root.Leaf(),
                Machine.GetState<EnemyInvestigating>());
        }

        // ===== MOVEMENT =====

        public bool TryMoveTo(Vector2 targetPosition, float speed)
        {
            if (ctx.nav == null)
                return false;

            float minRetarget = Mathf.Max(0.01f, ctx.retargetDistance);
            bool needsNew = !ctx.manualCommandActive
                || ctx.manualCommandFailed
                || (ctx.manualDestination - targetPosition).sqrMagnitude > minRetarget * minRetarget;

            if (!needsNew)
                return !ctx.manualCommandFailed;

            ForceMoveTo(targetPosition, speed);
            return !ctx.manualCommandFailed;
        }

        public void ForceMoveTo(Vector2 targetPosition, float speed)
        {
            if (ctx.nav == null)
                return;

            ctx.nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();
            ctx.usingVisualChase = false;

            ctx.manualDestination = targetPosition;
            ctx.manualCommandFailed = !ctx.nav.MoveTo(targetPosition, speed);
            ctx.manualCommandActive = !ctx.manualCommandFailed;
        }

        public bool HasCompletedManualMove()
        {
            if (!ctx.manualCommandActive || ctx.nav == null)
                return false;

            if (ctx.nav.HasPath || ctx.nav.State != PlatNavState.Idle)
                return false;

            ResetManualCommand();
            return true;
        }

        public void BeginWait(float duration)
        {
            ctx.isWaiting = duration > 0f;
            ctx.waitTimer = duration;
            ResetManualCommand();
        }

        public bool UpdateWaitTimer()
        {
            if (!ctx.isWaiting)
                return false;

            ctx.waitTimer -= Time.deltaTime;
            if (ctx.waitTimer > 0f)
                return false;

            ctx.isWaiting = false;
            ctx.waitTimer = 0f;
            return true;
        }

        public void ResetWait()
        {
            ctx.isWaiting = false;
            ctx.waitTimer = 0f;
        }

        public void ResetManualCommand()
        {
            ctx.manualCommandActive = false;
            ctx.manualCommandFailed = false;
        }

        // ===== CHASE =====

        public void BeginVisualChase()
        {
            if (ctx.nav == null || ctx.playerTransform == null)
                return;

            if (ctx.usingVisualChase)
                return;

            ctx.nav.Abort();
            ctx.nav.SetBehaviour(PlatNavBehaviour.FollowTarget);
            ctx.nav.SetTarget(ctx.playerTransform);
            ctx.nav.MoveTo(ctx.playerTransform.position, ctx.chaseSpeed);
            ctx.usingVisualChase = true;
            ResetManualCommand();
        }

        public void StopVisualChase()
        {
            if (ctx.nav == null)
                return;

            ctx.nav.SetTarget(null);
            if (ctx.usingVisualChase)
                ctx.nav.Abort();

            ctx.usingVisualChase = false;
        }

        public void UpdateKnownPlayerPosition(Vector2 position)
        {
            if (ctx.lightSensor != null && ctx.lightSensor.IsBlindedAt(position, out _, out _))
                return;

            ctx.lastKnownPlayerPosition = position;
            ctx.hasKnownPlayerPosition = true;
            ctx.hasDetectedPlayer = true;
        }

        // ===== UTILITY =====

        public Vector2 GetFacingDirection()
            => ctx.selfTransform.localScale.x >= 0f ? Vector2.right : Vector2.left;

        public int GetInitialPatrolIndex()
        {
            if (ctx.patrolRoute == null || ctx.patrolRoute.Length == 0)
                return 0;

            return GetClosestIndex(ctx.patrolRoute, ctx.selfTransform.position);
        }

        public static int GetClosestIndex(IReadOnlyList<Vector2> points, Vector2 worldPosition)
        {
            int best = 0;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                float sqr = (points[i] - worldPosition).sqrMagnitude;
                if (sqr >= bestSqr)
                    continue;

                bestSqr = sqr;
                best = i;
            }

            return best;
        }

        public static int GetNextBounceIndex(int current, ref int direction, int length)
        {
            if (length <= 1)
                return 0;

            int next = current + direction;
            if (next >= length || next < 0)
            {
                direction *= -1;
                next = current + direction;
            }

            return Mathf.Clamp(next, 0, length - 1);
        }
    }
}
