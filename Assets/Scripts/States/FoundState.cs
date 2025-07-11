using UnityEngine;

public class FoundState : IState
{
    void IState.Enter()
    {
        Debug.Log("Entering Found State");
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Found State");
    }

    void IState.Update()
    {

    }
}