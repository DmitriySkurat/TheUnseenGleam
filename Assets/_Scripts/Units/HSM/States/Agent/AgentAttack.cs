using UnityEngine;

namespace HSM {
    public class AgentAttack : State
    {
        readonly AgentContext ctx;

        bool _firstHitDone;

        public AgentAttack(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.Abort();

            ctx.attackFirstHitTimer = ctx.stats.AttackFirstHitDelay;
            ctx.attackRepeatTimer   = 0f;
            _firstHitDone           = false;

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!_firstHitDone)
            {
                ctx.attackFirstHitTimer -= deltaTime;
                if (ctx.attackFirstHitTimer <= 0f)
                {
                    Hit();
                    _firstHitDone         = true;
                    ctx.attackRepeatTimer = ctx.stats.AttackRepeatInterval;
                }
            }
            else
            {
                ctx.attackRepeatTimer -= deltaTime;
                if (ctx.attackRepeatTimer <= 0f)
                {
                    Hit();
                    ctx.attackRepeatTimer = ctx.stats.AttackRepeatInterval;
                }
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (ctx.IsTraversingLink) return null;

            // Lost sight of player → prediction chase
            if (ctx.vision == null || !ctx.vision.CanSeePlayer)
            {
                ctx.suspicionPosition = ctx.vision != null
                    ? ctx.vision.LastSeenPosition
                    : (Vector2)ctx.transform.position;
                return Machine?.GetState<AgentPredictionChase>();
            }

            // Player moved out of attack range → back to chase
            float dist = Vector2.Distance(ctx.transform.position, ctx.playerTransform.position);
            if (dist > ctx.stats.AttackRange)
                return Machine?.GetState<AgentChase>();

            return null;
        }

        void Hit()
        {
            ctx.playerHealth?.TakeDamage(ctx.stats.AttackDamage);
        }
    }
}
