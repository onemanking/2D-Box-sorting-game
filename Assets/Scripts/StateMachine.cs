using UnityEngine;

public class StateMachine
{
    internal IState CurrentState { get; private set; }
    internal IState PreviousState { get; private set; }

    internal void Init(IState state)
    {
        CurrentState = state;
        PreviousState = null;

        CurrentState.Enter();

        Debug.Log($"State Machine initialized with state: {CurrentState}");
    }

    internal void ChangeState(IState state)
    {
        if (CurrentState == state) return;

        CurrentState?.Exit();

        PreviousState = CurrentState;

        CurrentState = state;

        CurrentState?.Enter();

        Debug.Log($"State changed from {PreviousState} to {CurrentState}");
    }

    internal void Update()
    {
        CurrentState?.Update();
    }

    internal void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}

