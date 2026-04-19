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
        float   _climbStartTime;
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

            // Stand: нижний край капсулы чуть выше уровня угла, тело за стеной.
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
            if (!_isClimbingUp) {
                ctx.velocity = Vector2.zero;

                if (ctx.input.Move.y > ctx.stats.VerticalDeadZoneThreshold) {
                    _isClimbingUp  = true;
                    _climbStartTime = Time.time;
                    ctx.anim.Play("LedgeClimb");
                }
            } else {
                float t = Mathf.Clamp01((Time.time - _climbStartTime) / ctx.stats.LedgeClimbDuration);
                if (ctx.rb != null)
                    ctx.rb.position = Vector2.Lerp(_hangPosition, _standPosition, t);
                ctx.velocity = Vector2.zero;
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition() {
            // S — отпустить уступ
            if (!_isClimbingUp && ctx.input.Move.y < -ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Airborne>();

            // Подтянулись до конца
            if (_isClimbingUp && Time.time >= _climbStartTime + ctx.stats.LedgeClimbDuration) {
                _completedClimb = true;
                return Machine?.GetState<Grounded>();
            }

            return null;
        }
    }
}
