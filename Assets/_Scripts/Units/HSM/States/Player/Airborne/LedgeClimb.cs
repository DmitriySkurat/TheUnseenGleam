using UnityEngine;

namespace HSM {
    /// <summary>
    /// Игрок цепляется за уступ в воздухе.
    /// Вход:  из Airborne когда ctx.canGrabLedge == true.
    /// Висение: W — подтянуться, S — отпустить (→ Airborne).
    /// Выход: по окончании подтягивания → Grounded.
    /// </summary>
    public class LedgeClimb : State {
        readonly PlayerContext ctx;

        bool    _isClimbingUp;
        bool    _completedClimb;
        float   _riseStartTime;
        Vector2 _hangPosition;
        Vector2 _standPosition;

        public LedgeClimb(StateMachine m, State parent, PlayerContext ctx) : base(m, parent) {
            this.ctx = ctx;
            //Add(new AnimatorBoolActivity(ctx.anim, "LedgeGrab", true, false));
            
            Add(new AnimatorPlayActivity(ctx.anim, "JumpWallgrab"));
        }

        protected override void OnEnter() {
            _isClimbingUp   = false;
            _completedClimb = false;
            ctx.isLedgeGrabbing = true;
            ctx.velocity        = Vector2.zero;
            ctx.currentStaminaDrainMultiplier       = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            float dir = ctx.ledgeFacingRight ? 1f : -1f;

            // Вычисляем позиции через реальные размеры коллайдера, чтобы избежать
            // попадания внутрь геометрии.
            var capsule   = ctx.coll as CapsuleCollider2D;
            float halfW   = capsule != null ? capsule.size.x * 0.5f : 0.25f;
            float halfH   = capsule != null ? capsule.size.y * 0.5f : 0.7f;
            float offsetY = capsule != null ? capsule.offset.y       : 0f;

            // Hang: верхний край капсулы на уровне угла уступа, тело за стеной.
            _hangPosition = new Vector2(
                ctx.ledgeCornerPosition.x - dir * (halfW + ctx.stats.LedgeHangOffsetX),
                ctx.ledgeCornerPosition.y - offsetY - halfH
            );

            // Stand: на платформе, чуть дальше от края.
            _standPosition = new Vector2(
                ctx.ledgeCornerPosition.x + dir * (halfW + ctx.stats.LedgeStandOffsetX),
                ctx.ledgeCornerPosition.y - offsetY + halfH + ctx.stats.LedgeStandOffsetY
            );


            if (ctx.rb != null) {
                ctx.rb.linearVelocity = Vector2.zero;
                ctx.rb.position       = _hangPosition;
            }

            base.OnEnter();
        }

        protected override void OnExit() {
            ctx.isLedgeGrabbing = false;
            // Если подтянулись до конца — сообщаем физике, что игрок на земле,
            // чтобы PlayerRoot не вернул нас мгновенно в Airborne за один кадр.
            if (_completedClimb)
                ctx.grounded = true;
            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime) {
            ctx.velocity = Vector2.zero;

            if (!_isClimbingUp) {
                if (ctx.rb != null) ctx.rb.position = _hangPosition;

                if (ctx.input.Move.y > ctx.stats.VerticalDeadZoneThreshold) {
                    _isClimbingUp  = true;
                    _riseStartTime = Time.time;
                    ctx.anim.Play("WallgrabClime");
                }
            } else {
                // Y: подъём вверх, начинается сразу.
                float riseT = Mathf.Clamp01((Time.time - _riseStartTime) / ctx.stats.LedgeClimbRiseDuration);

                // X: движение вбок, начинается после LedgeClimbSideDelay.
                float sideElapsed = Time.time - _riseStartTime - ctx.stats.LedgeClimbSideDelay;
                float sideT = Mathf.Clamp01(sideElapsed / ctx.stats.LedgeClimbSideDuration);

                if (ctx.rb != null)
                    ctx.rb.position = new Vector2(
                        Mathf.Lerp(_hangPosition.x, _standPosition.x, sideT),
                        Mathf.Lerp(_hangPosition.y, _standPosition.y, riseT)
                    );
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition() {
            if (!_isClimbingUp && ctx.input.Move.y < -ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Airborne>();

            if (_isClimbingUp) {
                float riseEnd = _riseStartTime + ctx.stats.LedgeClimbRiseDuration;
                float sideEnd = _riseStartTime + ctx.stats.LedgeClimbSideDelay + ctx.stats.LedgeClimbSideDuration;
                if (Time.time >= Mathf.Max(riseEnd, sideEnd)) {
                    _completedClimb = true;
                    return Machine?.GetState<Grounded>();
                }
            }

            return null;
        }
    }
}
