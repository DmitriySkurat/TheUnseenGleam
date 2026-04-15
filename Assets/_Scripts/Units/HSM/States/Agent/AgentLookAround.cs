using UnityEngine;

namespace HSM {
    /// <summary>
    /// Подсостояние AgentPatrol: агент останавливается и осматривается по случайному таймеру.
    /// Ждёт LookAroundTurnDelay секунд → поворачивается → стоит LookAroundDuration секунд → возвращается в AgentPatrolWalk.
    ///
    /// Внешние триггеры (игрок, шум, тревога) обрабатываются родителем AgentPatrol — здесь не дублируются.
    /// </summary>
    public class AgentLookAround : State
    {
        readonly AgentContext ctx;

        enum Phase { WaitBeforeTurn, Looking }
        Phase _phase;
        float _phaseTimer;

        public AgentLookAround(StateMachine m, State parent, AgentContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
        }

        protected override void OnEnter()
        {
            ctx.nav.SetTarget(null);
            ctx.nav.Abort();

            _phase      = Phase.WaitBeforeTurn;
            _phaseTimer = ctx.stats.LookAroundTurnDelay;

            base.OnEnter();
        }

        protected override void OnUpdate(float deltaTime)
        {
            _phaseTimer -= deltaTime;

            if (_phase == Phase.WaitBeforeTurn && _phaseTimer <= 0f)
            {
                FlipDirection();
                _phase      = Phase.Looking;
                _phaseTimer = ctx.stats.LookAroundDuration;
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (_phase == Phase.Looking && _phaseTimer <= 0f)
                return Machine?.GetState<AgentPatrolWalk>();

            return null;
        }

        void FlipDirection()
        {
            var scale = ctx.transform.localScale;
            scale.x = -scale.x;
            ctx.transform.localScale = scale;
        }
    }
}
