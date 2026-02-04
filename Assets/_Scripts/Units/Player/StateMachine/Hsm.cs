using UnityEngine;

public sealed class Hsm
{
    private HsmState _currentState;

    public void Initialize(HsmState rootState)
    {
        SetCurrentState(rootState);
    }

    public void Update()
    {
        _currentState?.Update();
    }

    public void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }

    internal void SetCurrentState(HsmState state)
    {
        if (_currentState == state) return;

        _currentState?.Exit();
        _currentState = state;
        _currentState?.Enter();
    }
}

public abstract class HsmState
{
    private HsmState _superState;
    private HsmState _subState;
    protected readonly Hsm Hsm;

    protected bool IsRootState { get; set; }

    protected HsmState(Hsm hsm)
    {
        Hsm = hsm;
    }

    public void Enter()
    {
        OnEnter();
        InitializeSubState();
        _subState?.Enter();
    }

    public void Exit()
    {
        _subState?.Exit();
        OnExit();
    }

    public void Update()
    {
        OnUpdate();
        _subState?.Update();
    }

    public void FixedUpdate()
    {
        OnFixedUpdate();
        _subState?.FixedUpdate();
    }

    protected void SwitchState(HsmState newState)
    {
        if (IsRootState)
        {
            Hsm.SetCurrentState(newState);
            return;
        }

        _superState?.SwitchState(newState);
    }

    protected void SwitchSubState(HsmState newSubState)
    {
        if (_subState == newSubState) return;

        _subState?.Exit();
        _subState = newSubState;
        _subState?.SetSuperState(this);
        _subState?.Enter();
    }

    protected void SetSuperState(HsmState newSuperState)
    {
        _superState = newSuperState;
    }

    protected void SetSubState(HsmState newSubState)
    {
        _subState = newSubState;
        _subState?.SetSuperState(this);
    }

    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void InitializeSubState() { }
}
