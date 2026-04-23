using UnityEngine;

namespace HSM
{
    public class LedgeClimb : State
    {
        readonly PlayerContext ctx;

        bool _isClimbingUp;
        bool _completedClimb;
        float _climbStartTime;
        Vector2 _hangPosition;
        Vector2 _standPosition;

        public LedgeClimb(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new AnimatorPlayActivity(ctx.anim, PlayerAnimations.JumpWallgrab));
        }

        protected override void OnEnter()
        {
            _isClimbingUp = false;
            _completedClimb = false;
            _climbStartTime = float.MinValue;

            ctx.isLedgeGrabbing = true;
            ctx.velocity = Vector2.zero;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;

            float dir = ctx.ledgeFacingRight ? 1f : -1f;
            var capsule = ctx.coll as CapsuleCollider2D;
            float halfW = capsule != null ? capsule.size.x * 0.5f : 0.25f;
            float halfH = capsule != null ? capsule.size.y * 0.5f : 0.7f;
            float offsetY = capsule != null ? capsule.offset.y : 0f;

            _hangPosition = new Vector2(
                ctx.ledgeCornerPosition.x - dir * (halfW + ctx.stats.LedgeHangOffsetX),
                ctx.ledgeCornerPosition.y - offsetY - halfH
            );

            _standPosition = new Vector2(
                ctx.ledgeCornerPosition.x + dir * (halfW + ctx.stats.LedgeStandOffsetX),
                ctx.ledgeCornerPosition.y - offsetY + halfH + ctx.stats.LedgeStandOffsetY
            );

            SnapToPosition(_hangPosition);

            base.OnEnter();
        }

        protected override void OnExit()
        {
            ctx.isLedgeGrabbing = false;

            if (_completedClimb)
                ctx.grounded = true;

            base.OnExit();
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.velocity = Vector2.zero;
            SnapToPosition(_hangPosition);

            if (!_isClimbingUp && ctx.input.Move.y > ctx.stats.VerticalDeadZoneThreshold)
            {
                _isClimbingUp = true;
                _climbStartTime = Time.time;
                ctx.anim.Play(PlayerAnimations.WallgrabClime);
            }

            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!_isClimbingUp && ctx.input.Move.y < -ctx.stats.VerticalDeadZoneThreshold)
                return Machine?.GetState<Airborne>();

            if (_isClimbingUp && Time.time >= _climbStartTime + ctx.stats.LedgeClimbTeleportDelay)
            {
                SnapToPosition(_standPosition);
                _completedClimb = true;
                return Machine?.GetState<Grounded>();
            }

            return null;
        }

        void SnapToPosition(Vector2 position)
        {
            if (ctx.rb != null)
            {
                ctx.rb.linearVelocity = Vector2.zero;
                ctx.rb.position = position;
                return;
            }

            if (ctx.transform != null)
            {
                Vector3 current = ctx.transform.position;
                ctx.transform.position = new Vector3(position.x, position.y, current.z);
            }
        }
    }
}
