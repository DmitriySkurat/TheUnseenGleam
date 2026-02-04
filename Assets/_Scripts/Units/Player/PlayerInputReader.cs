using UnityEngine;

public sealed class PlayerInputReader : MonoBehaviour
{
    public FrameInput ReadFrameInput(ScriptableStats stats)
    {
        var frameInput = new FrameInput
        {
            JumpDown = InputManager.JumpWasPressed,
            JumpHeld = InputManager.JumpIsHeld,
            RunHeld = InputManager.RunIsHeld,
            CrouchHeld = InputManager.CrouchIsHeld,
            Move = InputManager.Movement
        };

        if (stats.SnapInput)
        {
            frameInput.Move.x =
                Mathf.Abs(frameInput.Move.x) < stats.HorizontalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(frameInput.Move.x);

            frameInput.Move.y =
                Mathf.Abs(frameInput.Move.y) < stats.VerticalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(frameInput.Move.y);
        }

        return frameInput;
    }
}
