
using UnityEngine;

public class CollectState : IState
{
    private readonly NPCBased npc;
    private readonly StateMachine stateMachine;

    public CollectState(NPCBased npc, StateMachine stateMachine)
    {
        this.npc = npc;
        this.stateMachine = stateMachine;
    }

    void IState.Enter()
    {
        Debug.Log("Entering Collect State");

        if (!ValidateCollection())
        {
            stateMachine.ChangeState(npc.SearchState);
            return;
        }

        CollectBox();
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Collect State");
    }

    void IState.Update()
    {
    }

    private bool ValidateCollection()
    {
        if (npc.FoundedBox == null)
        {
            Debug.LogWarning("Cannot collect: No box found");
            return false;
        }

        if (npc.CurrentBox != null)
        {
            Debug.LogWarning("Cannot collect: Already holding a box");
            return false;
        }

        return true;
    }

    private void CollectBox()
    {
        var collectBox = npc.FoundedBox;
        var handTransform = npc.HandTransform;

        npc.PlayAnimation(this, () =>
        {
            npc.SetCurrentBox(collectBox);
            npc.CurrentBox.SetHolding(handTransform);

            stateMachine.ChangeState(npc.SortingState);

            npc.SetFoundedBox(null);
            Debug.Log($"Collected box: {collectBox.name}");
        });
    }
}