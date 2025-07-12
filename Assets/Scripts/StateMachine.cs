using UnityEngine;

public class StateMachine
{
    internal IState CurrentState { get; private set; }
    internal IState PreviousState { get; private set; }

    internal bool InStateTransition { get; private set; }


    public StateMachine()
    {
    }

    internal void ChangeState(IState state)
    {
        if (CurrentState == state || InStateTransition) return;

        InStateTransition = true;

        CurrentState?.Exit();

        PreviousState = CurrentState;

        CurrentState = state;

        CurrentState?.Enter();

        Debug.Log($"State changed from {PreviousState} to {CurrentState}");
    }

    internal void Update()
    {
        if (InStateTransition && CurrentState != null)
        {
            CurrentState.Update();
        }
    }

    internal void FixedUpdate()
    {
        if (InStateTransition && CurrentState != null)
        {
            CurrentState.FixedUpdate();
        }
    }
}

