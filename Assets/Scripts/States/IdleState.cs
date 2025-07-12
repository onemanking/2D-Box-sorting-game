using UnityEngine;

public class IdleState : IState
{
    private readonly NPCBased npc;
    private readonly StateMachine stateMachine;
    private readonly float idleTimeMin;
    private readonly float idleTimeMax;
    private float idleTimer;
    private float idleWaitTime;

    public IdleState(NPCBased npc, StateMachine stateMachine)
    {
        this.npc = npc;
        this.stateMachine = stateMachine;
        idleTimeMin = npc.StateConfig.IdleTimeMin;
        idleTimeMax = npc.StateConfig.IdleTimeMax;
    }

    void IState.Enter()
    {
        idleWaitTime = Random.Range(idleTimeMin, idleTimeMax);
        idleTimer = 0f;
        Debug.Log($"Entering Idle State. Waiting for {idleWaitTime} seconds before transitioning to Wander State.");
    }

    void IState.Exit()
    {
        idleTimer = 0f;
    }

    void IState.Update()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleWaitTime)
        {
            stateMachine.ChangeState(npc.WanderState);
        }
    }
}
