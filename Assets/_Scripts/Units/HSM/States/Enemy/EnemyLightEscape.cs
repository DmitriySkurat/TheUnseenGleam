using UnityEngine;

namespace HSM
{
    public class EnemyLightEscape : State
    {
        private readonly EnemyRoot root;
        private readonly EnemyContext ctx;

        private Vector2 _escapeDirection;
        private bool _exitedLight;
        private float _overrunTravelled;
        private Vector2 _overrunLastPos;
        private Vector2 _prevPos;
        private bool _flipped;
        private float _stuckTimer;

        private const float StuckThreshold = 0.02f;
        private const float StuckDuration  = 0.3f;

        public EnemyLightEscape(StateMachine m, EnemyRoot root) : base(m, root)
        {
            this.root = root;
            this.ctx = root.ctx;
            Add(new ColorPhaseActivity(ctx.renderer)
            {
                enterColor = Color.yellow,
            });
        }

        protected override void OnEnter()
        {
            _escapeDirection  = root.GetFacingDirection();
            _exitedLight      = false;
            _overrunTravelled = 0f;
            _flipped          = false;
            _stuckTimer       = 0f;

            Vector2 startPos  = ctx.selfTransform.position;
            _overrunLastPos   = startPos;
            _prevPos          = startPos;

            root.StopVisualChase();
            root.ResetWait();
            root.ResetManualCommand();
            if (ctx.nav != null)
                ctx.nav.Abort();
            ctx.hasKnownPlayerPosition = false;
        }

        protected override void OnUpdate(float deltaTime)
        {
            Vector2 currentPos = ctx.selfTransform.position;

            float movedX = Mathf.Abs(currentPos.x - _prevPos.x);
            if (movedX < StuckThreshold)
            {
                _stuckTimer += deltaTime;
                if (_stuckTimer >= StuckDuration)
                {
                    if (!_flipped)
                    {
                        _escapeDirection = -_escapeDirection;
                        _flipped         = true;
                        _stuckTimer      = 0f;
                    }
                    else
                    {
                        TransitionToPatrol();
                        return;
                    }
                }
            }
            else
            {
                _stuckTimer = 0f;
            }

            if (ctx.rb != null)
            {
                ctx.rb.linearVelocity = new Vector2(_escapeDirection.x * ctx.lightEscapeSpeed, ctx.rb.linearVelocity.y);
                Vector3 scale = ctx.selfTransform.localScale;
                scale.x = _escapeDirection.x >= 0f ? 1f : -1f;
                ctx.selfTransform.localScale = scale;
            }

            _prevPos = currentPos;

            if (!_exitedLight)
            {
                if (ctx.lightSensor == null || !ctx.lightSensor.IsBlinded(out _, out _))
                {
                    _exitedLight    = true;
                    _overrunLastPos = currentPos;
                }
                return;
            }

            _overrunTravelled += Vector2.Distance(currentPos, _overrunLastPos);
            _overrunLastPos    = currentPos;

            if (_overrunTravelled >= ctx.lightOverrunDistance)
                TransitionToPatrol();
        }

        private void TransitionToPatrol()
        {
            Machine.Sequencer.RequestTransition(this, Machine.GetState<EnemyReturningToPatrol>());
        }
    }
}
