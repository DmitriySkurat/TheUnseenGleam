using UnityEngine;

namespace HSM {
    public abstract class Player2DState : State {
        const float MoveStopEpsilon = 0.001f;
        protected readonly PlayerContext2D Ctx;
        protected ScriptableStats Stats => Ctx.stats;
        protected Rigidbody2D Rigidbody => Ctx.rb;

        protected Player2DState(StateMachine m, State parent, PlayerContext2D ctx) : base(m, parent) {
            Ctx = ctx;
        }

        protected void HandleJump(float dt) {
            if (!Ctx.endedJumpEarly && !Ctx.IsGrounded && !Ctx.frameInput.JumpHeld && Ctx.rb.linearVelocity.y > 0) {
                Ctx.endedJumpEarly = true;
            }

            if (!Ctx.jumpToConsume && !Ctx.HasBufferedJump) return;

            if (Ctx.IsGrounded || Ctx.CanUseCoyote) ExecuteJump();

            Ctx.jumpToConsume = false;
        }

        void ExecuteJump() {
            Ctx.endedJumpEarly = false;
            Ctx.timeJumpWasPressed = 0;
            Ctx.bufferedJumpUsable = false;
            Ctx.coyoteUsable = false;

            if (Rigidbody == null) return;
            var velocity = Rigidbody.linearVelocity;
            velocity.y = Stats.JumpPower;
            Rigidbody.linearVelocity = velocity;
        }

        protected void HandleDirection(float dt) {
            if (Rigidbody == null) return;
            var velocity = Rigidbody.linearVelocity;

            if (Mathf.Abs(Ctx.frameInput.Move.x) < MoveStopEpsilon) {
                var deceleration = Ctx.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
                velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * dt);
            } else {
                velocity.x = Mathf.MoveTowards(
                    velocity.x,
                    Ctx.frameInput.Move.x * GetMaxSpeed(),
                    Stats.Acceleration * dt
                );
            }

            Rigidbody.linearVelocity = velocity;
        }

        protected void HandleGravity(float dt) {
            if (Rigidbody == null) return;
            var velocity = Rigidbody.linearVelocity;

            if (Ctx.IsGrounded && velocity.y <= 0f) {
                velocity.y = Stats.GroundingForce;
            } else {
                var inAirGravity = Stats.FallAcceleration;
                if (Ctx.endedJumpEarly && velocity.y > 0) inAirGravity *= Stats.JumpEndEarlyGravityModifier;
                velocity.y = Mathf.MoveTowards(velocity.y, -Stats.MaxFallSpeed, inAirGravity * dt);
            }

            Rigidbody.linearVelocity = velocity;
        }

        protected void ApplyMovement() {
            // Movement now applied directly to Rigidbody.linearVelocity in the handlers.
        }

        protected void SetCrouchCollider(bool isCrouching) {
            Ctx.SetCrouchCollider(isCrouching);
        }

        protected virtual float GetMaxSpeed() => Stats.MaxSpeed;
    }
}
