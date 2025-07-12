using System.Linq;
using UnityEngine;

public class SortingState : IState
{
    private readonly NPCBased npc;
    private readonly StateMachine stateMachine;
    private readonly SortingArea[] sortingAreaArray;
    private readonly float sortingSpeed;

    public SortingState(NPCBased npc, StateMachine stateMachine, SortingArea[] sortingAreaArray)
    {
        this.npc = npc;
        this.stateMachine = stateMachine;
        this.sortingAreaArray = sortingAreaArray;
        sortingSpeed = npc.StateConfig.SortingSpeed;
    }

    void IState.Enter()
    {
        Debug.Log("Entering Sorting State");
        if (!ValidateSorting())
        {
            stateMachine.ChangeState(npc.WanderState);
            return;
        }

        var sortingArea = FindCompatibleSortingArea();
        if (sortingArea == null)
        {
            Debug.LogWarning($"No sorting area found for box {npc.CurrentBox.name}");
            stateMachine.ChangeState(npc.IdleState);
        }
        else
        {
            npc.StartMovementCoroutine(sortingArea.transform.position, sortingSpeed, () => OnReachedSortingArea(sortingArea));
        }
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Sorting State");

        npc.StopMovementCoroutine();
    }

    void IState.Update()
    {

    }

    private bool ValidateSorting()
    {
        if (npc.CurrentBox == null)
        {
            Debug.LogWarning("Cannot sort: No box is currently held");
            return false;
        }

        if (sortingAreaArray == null || sortingAreaArray.Length == 0)
        {
            Debug.LogError("Cannot sort: No sorting areas available");
            return false;
        }

        return true;
    }

    private SortingArea FindCompatibleSortingArea()
    {
        return sortingAreaArray
            .Where(x => x.IsBoxAccepted(npc.CurrentBox))
            .OrderBy(x => Vector2.Distance(npc.transform.position, x.transform.position))
            .FirstOrDefault();
    }

    private void OnReachedSortingArea(SortingArea sortingArea)
    {
        sortingArea.DepositBox(npc.CurrentBox);
        npc.SetCurrentBox(null);
        stateMachine.ChangeState(npc.IdleState);
    }
}