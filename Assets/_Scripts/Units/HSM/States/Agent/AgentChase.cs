using UnityEngine;
using PlatNav;

namespace HSM {
    public class AgentChase : State
    {
        readonly AgentContext ctx;

        bool _inMemoryPhase;
        bool _memoryNavReached;

        public AgentChase(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            _inMemoryPhase = false;
            _memoryNavReached = false;
            ctx.chaseMemoryTimer = 0f;

            ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
            ctx.nav.SetTarget(ctx.playerTransform);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            bool canSee = ctx.vision != null && ctx.vision.CanSeePlayer;

            if (canSee)
            {
                ctx.suspicionPosition = ctx.vision.LastSeenPosition;

                if (ctx.playerRb != null)
                    ctx.chaseLastKnownVelocity = ctx.playerRb.linearVelocity;

                if (_inMemoryPhase)
                {
                    // Regained sight during memory phase — resume direct chase
                    _inMemoryPhase = false;
                    _memoryNavReached = false;
                    ctx.chaseMemoryTimer = 0f;
                    ctx.nav.Abort();
                    ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
                    ctx.nav.SetTarget(ctx.playerTransform);
                }
            }
            else if (!_inMemoryPhase)
            {
                // Just lost sight — enter memory phase and navigate to predicted position
                _inMemoryPhase = true;
                ctx.chaseMemoryTimer = ctx.stats.ChaseMemoryTime;

                ctx.chasePredictedPosition = ComputePredictedPosition();
                ctx.suspicionPosition = ctx.chasePredictedPosition;

                ctx.nav.SetTarget(null);
                bool started = ctx.nav.MoveTo(ctx.chasePredictedPosition, ctx.stats.ChaseSpeed);
                if (!started)
                    _memoryNavReached = true;
            }
            else
            {
                // Memory phase — decrement timer and detect when destination is reached
                ctx.chaseMemoryTimer -= deltaTime;

                if (!_memoryNavReached && ctx.nav.State == PlatNavState.Idle)
                    _memoryNavReached = true;
            }

            ctx.nav.Tick(deltaTime);

            base.OnUpdate(deltaTime);
        }

        protected override void OnExit()
        {
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();
            base.OnExit();
        }

        protected override State GetTransition()
        {
            if (_inMemoryPhase && (ctx.chaseMemoryTimer <= 0f || _memoryNavReached))
                return Machine != null ? Machine.GetState<AgentSearch>() : null;

            return null;
        }

        // Predicts where the player will be after losing sight.
        //
        // Jumping up  (vel_y > threshold): kinematic apex — cap horizon at peak so
        //   the target lands at the highest reachable point, not underground.
        // Falling     (vel_y < -threshold): kinematic formula always overshoots
        //   because it ignores ground collisions, so we raycast for the actual floor.
        // Grounded / near-zero vel_y: keep Y fixed, predict only horizontally.
        Vector2 ComputePredictedPosition()
        {
            float velX = ctx.chaseLastKnownVelocity.x;
            float velY = ctx.chaseLastKnownVelocity.y;
            float t    = ctx.stats.ChaseMemoryTime;

            float predictedX = ctx.suspicionPosition.x + velX * t;
            float predictedY = ctx.suspicionPosition.y;

            const float kAirborneThreshold = 1f;

            if (velY > kAirborneThreshold)
            {
                // Jumping up — predict to apex
                float gravityY = GravityY();
                if (gravityY < 0f)
                    t = Mathf.Min(t, -velY / gravityY);

                predictedX = ctx.suspicionPosition.x + velX * t;
                predictedY = ctx.suspicionPosition.y + velY * t + 0.5f * gravityY * t * t;
            }
            else if (velY < -kAirborneThreshold)
            {
                // Falling — kinematic formula ignores ground, so raycast for landing spot
                var hit = Physics2D.Raycast(
                    new Vector2(predictedX, ctx.suspicionPosition.y),
                    Vector2.down,
                    200f,
                    ctx.stats.GroundLayer
                );
                if (hit.collider != null)
                    predictedY = hit.point.y;
            }

            return new Vector2(predictedX, predictedY);
        }

        float GravityY() => ctx.playerRb != null
            ? ctx.playerRb.gravityScale * Physics2D.gravity.y
            : Physics2D.gravity.y;
    }
}
