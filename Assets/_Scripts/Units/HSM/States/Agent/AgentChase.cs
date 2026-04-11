using UnityEngine;

namespace HSM {
    public class AgentChase : State
    {
        readonly AgentContext ctx;

        public AgentChase(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.chaseVisionLostTimer  = 0f;
            ctx.grabOccurredInChase   = false; // новая погоня — первый захват будет с полной задержкой

            // Set speed once; auto-repath in Tick will keep using this value
            ctx.nav.SetSpeed(ctx.stats.ChaseSpeed);
            // Hand the player transform to PlatNavHandler so it re-paths automatically
            ctx.nav.SetTarget(ctx.playerTransform);

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            // Отсчёт кулдауна между захватами
            if (ctx.grabCooldownTimer > 0f)
                ctx.grabCooldownTimer -= deltaTime;

            // Keep suspicionPosition and player velocity up-to-date so PredictionChase knows where to go
            if (ctx.vision != null && ctx.vision.CanSeePlayer)
            {
                ctx.suspicionPosition         = ctx.vision.LastSeenPosition;
                ctx.chaseVisionLostTimer      = 0f;
                if (ctx.playerRb != null)
                    ctx.predictionPlayerVelocity = ctx.playerRb.linearVelocity;
            }
            else if (ctx.isBlindedByPlayer)
            {
                // Игрок слепит агента — агент его «видит» через свет, таймер не растёт
                ctx.suspicionPosition    = ctx.blindingSourcePosition;
                ctx.chaseVisionLostTimer = 0f;
            }
            else
            {
                ctx.chaseVisionLostTimer += deltaTime;
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
            if (ctx.IsTraversingLink) return null;

            if (ctx.chaseVisionLostTimer >= ctx.stats.ChaseVisionGraceTime)
                return Machine != null ? Machine.GetState<AgentPredictionChase>() : null;

            if (ctx.grabCooldownTimer <= 0f && ctx.playerTransform != null)
            {
                float dist = Vector2.Distance(ctx.transform.position, ctx.playerTransform.position);
                bool inRange = dist <= ctx.stats.AttackRange;

                // Видим игрока, или только что потеряли его в пределах окна,
                // или ослеплены окружением но вплотную — всё равно хватаем.
                bool canSense = (ctx.vision != null && ctx.vision.CanSeePlayer)
                    || ctx.chaseVisionLostTimer < ctx.stats.GrabProximityGraceWindow
                    || (inRange && !ctx.isBlindedByPlayer);

                if (inRange && canSense)
                    return Machine != null ? Machine.GetState<AgentGrabPlayer>() : null;
            }

            return null;
        }
    }
}