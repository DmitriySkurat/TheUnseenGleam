using PlatNav;
using UnityEngine;

namespace HSM {
    public class AgentSearch : State
    {
        readonly AgentContext ctx;

        bool _navigating;
        bool _wandering;
        bool _waitingAtWanderPoint;
        bool _seenPlayerDuringLink;

        Vector2[] _wanderPoints;
        int _wanderIndex;

        public AgentSearch(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.SetTarget(null);
            ctx.searchWaitTimer   = 0f;
            ctx.searchWanderTimer = 0f;
            ctx.pendingNoiseAlert = false;
            ctx.alertPending      = false;

            _navigating              = false;
            _wandering               = false;
            _waitingAtWanderPoint    = false;
            _seenPlayerDuringLink    = false;
            _wanderPoints            = null;
            _wanderIndex             = 0;

            StartNavigatingToSearchPoint();
            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Track player sighting during a jump so GetTransition can act after landing
            if (ctx.IsTraversingLink && ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                _seenPlayerDuringLink = true;
                ctx.suspicionPosition = ctx.vision.LastSeenPosition;
            }

            // Alert from another agent — перенаправляем поиск к точке тревоги
            if (ctx.alertPending && !ctx.IsTraversingLink)
            {
                ctx.alertPending = false;
                if (!ctx.isBlindedByEnvironment)
                {
                    ctx.suspicionPosition = ctx.alertPosition;
                    _wandering            = false;
                    _waitingAtWanderPoint = false;
                    StartNavigatingToSearchPoint();
                }
            }

            // New noise arrived — defer until jump finishes to avoid interrupting traversal
            if (ctx.pendingNoiseAlert && !ctx.IsTraversingLink)
            {
                ctx.pendingNoiseAlert = false;
                // Ослеплены окружением — шум не меняет маршрут
                if (!ctx.isBlindedByEnvironment)
                {
                    ctx.suspicionPosition = ctx.pendingNoisePosition;
                    _wandering            = false;
                    _waitingAtWanderPoint = false;
                    StartNavigatingToSearchPoint();
                }
            }
            else if (_wandering)
            {
                UpdateWander(deltaTime);
            }
            else if (_navigating)
            {
                ctx.nav.SetSpeed(ctx.isBlindedByEnvironment
                    ? ctx.stats.BlindedByEnvironmentSpeed
                    : ctx.stats.SearchSpeed);
                ctx.nav.Tick(deltaTime);

                if (ctx.nav.State == PlatNavState.Idle)
                {
                    _navigating = false;
                    StartWandering();
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.nav.Abort();
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Игрок слепит агента — переходим в BlindedByPlayer
            if (ctx.isBlindedByPlayer)
                return Machine != null ? Machine.GetState<AgentBlindedByPlayer>() : null;

            // Chase if player is currently visible OR was spotted during the last jump
            if ((ctx.vision != null && ctx.vision.CanSeePlayer) || _seenPlayerDuringLink)
            {
                _seenPlayerDuringLink = false;
                return Machine != null ? Machine.GetState<AgentChase>() : null;
            }

            // Игрок прижат к стене в свету — зрение заблокировано, но позиция известна по шуму.
            // Если агент вплотную — хватаем напрямую, не ждём восстановления зрения.
            if (ctx.vision != null && ctx.vision.IsPlayerExposedInLight
                && ctx.playerTransform != null
                && ctx.grabCooldownTimer <= 0f)
            {
                float dist = Vector2.Distance(ctx.transform.position, ctx.playerTransform.position);
                bool playerAlreadyGrabbed = ctx.playerCtx != null && ctx.playerCtx.isGrabbed;
                if (dist <= ctx.stats.AttackRange && !playerAlreadyGrabbed)
                    return Machine != null ? Machine.GetState<AgentGrabPlayer>() : null;
            }

            if (_wandering && ctx.searchWanderTimer <= 0f)
                return Machine != null ? Machine.GetState<AgentReturnToPatrol>() : null;

            return null;
        }

        void UpdateWander(float deltaTime)
        {
            ctx.searchWanderTimer -= deltaTime;

            if (_waitingAtWanderPoint)
            {
                ctx.searchWaitTimer -= deltaTime;
                if (ctx.searchWaitTimer <= 0f)
                {
                    _waitingAtWanderPoint = false;
                    AdvanceWanderPoint();
                }
            }
            else if (_navigating)
            {
                ctx.nav.SetSpeed(ctx.isBlindedByEnvironment
                    ? ctx.stats.BlindedByEnvironmentSpeed
                    : ctx.stats.SearchSpeed);
                ctx.nav.Tick(deltaTime);

                if (ctx.nav.State == PlatNavState.Idle)
                {
                    _navigating           = false;
                    _waitingAtWanderPoint = true;
                    ctx.searchWaitTimer   = ctx.stats.SearchWaitTime;
                }
            }
            else
            {
                // Path to wander point failed — skip to next
                AdvanceWanderPoint();
            }
        }

        void StartNavigatingToSearchPoint()
        {
            _navigating = ctx.nav.MoveTo(ctx.suspicionPosition, ctx.stats.SearchSpeed);

            if (!_navigating)
                StartWandering();
        }

        void StartWandering()
        {
            float dist = ctx.stats.SearchWanderDistance;
            _wanderPoints = new Vector2[]
            {
                ctx.suspicionPosition + Vector2.left  * dist,
                ctx.suspicionPosition + Vector2.right * dist,
            };
            _wanderIndex          = 0;
            ctx.searchWanderTimer = ctx.stats.SearchWanderDuration;
            _wandering            = true;
            _waitingAtWanderPoint = false;

            NavigateToCurrentWanderPoint();
        }

        void NavigateToCurrentWanderPoint()
        {
            _navigating = ctx.nav.MoveTo(_wanderPoints[_wanderIndex], ctx.stats.SearchSpeed);
        }

        void AdvanceWanderPoint()
        {
            _wanderIndex = (_wanderIndex + 1) % _wanderPoints.Length;
            NavigateToCurrentWanderPoint();
        }
    }
}
