using UnityEngine;

public abstract class PlayerStateBase : HsmState
{
    private const float MoveStopEpsilon = 0.001f;
    protected readonly PlayerController Controller;
    protected readonly ScriptableStats Stats;
    protected readonly Rigidbody2D Rigidbody;
    protected readonly CapsuleCollider2D Collider;

    protected PlayerStateBase(PlayerController controller, Hsm hsm) : base(hsm)
    {
        Controller = controller;
        Stats = controller.Stats;
        Rigidbody = controller.Rigidbody;
        Collider = controller.Collider;
    }

    protected void HandleJump()
    {
        if (!Controller.EndedJumpEarly && !Controller.IsGrounded && !Controller.CurrentFrameInput.JumpHeld && Rigidbody.linearVelocity.y > 0)
        {
            Controller.EndedJumpEarly = true;
        }

        if (!Controller.JumpToConsume && !Controller.HasBufferedJump) return;

        if (Controller.IsGrounded || Controller.CanUseCoyote) ExecuteJump();

        Controller.JumpToConsume = false;
    }

    private void ExecuteJump()
    {
        Controller.EndedJumpEarly = false;
        Controller.TimeJumpWasPressed = 0;
        Controller.BufferedJumpUsable = false;
        Controller.CoyoteUsable = false;

        var velocity = Controller.FrameVelocity;
        velocity.y = Stats.JumpPower;
        Controller.FrameVelocity = velocity;

        Controller.NotifyJumped();
    }

    protected void HandleDirection()
    {
        var velocity = Controller.FrameVelocity;

        if (Mathf.Abs(Controller.CurrentFrameInput.Move.x) < MoveStopEpsilon)
        {
            var deceleration = Controller.IsGrounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, Controller.CurrentFrameInput.Move.x * GetMaxSpeed(), Stats.Acceleration * Time.fixedDeltaTime);
        }

        Controller.FrameVelocity = velocity;
    }

    protected void HandleGravity()
    {
        var velocity = Controller.FrameVelocity;

        if (Controller.IsGrounded && velocity.y <= 0f)
        {
            velocity.y = Stats.GroundingForce;
        }
        else
        {
            var inAirGravity = Stats.FallAcceleration;
            if (Controller.EndedJumpEarly && velocity.y > 0) inAirGravity *= Stats.JumpEndEarlyGravityModifier;
            velocity.y = Mathf.MoveTowards(velocity.y, -Stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }

        Controller.FrameVelocity = velocity;
    }

    protected void ApplyMovement()
    {
        Rigidbody.linearVelocity = Controller.FrameVelocity;
    }

    protected virtual float GetMaxSpeed()
    {
        return Stats.MaxSpeed;
    }
}
