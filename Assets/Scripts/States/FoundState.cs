using UnityEngine;

public class FoundState : IState
{
    private readonly NPCBased npc;
    private readonly StateMachine stateMachine;
    private readonly float foundSpeed;

    public FoundState(NPCBased npc, StateMachine stateMachine)
    {
        this.npc = npc;
        this.stateMachine = stateMachine;
        foundSpeed = npc.StateConfig.FoundSpeed;
    }

    void IState.Enter()
    {
        Debug.Log("Entering Found State");

        if (npc.FoundedBox == null)
        {
            stateMachine.ChangeState(npc.WanderState);
        }
        else
        {
            Debug.Log($"Found Box: {npc.FoundedBox.name}");
            var targetPosition = npc.FoundedBox.transform.position;
            npc.StartMovementCoroutine(targetPosition, foundSpeed, OnReachedBox);
        }
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Found State");
    }

    void IState.Update()
    {
        if (npc.FoundedBox == null)
        {
            stateMachine.ChangeState(npc.WanderState);
        }
    }

    private void OnReachedBox()
    {
        stateMachine.ChangeState(npc.CollectState);
    }
}