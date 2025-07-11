using UnityEngine;

public class IdleState : IState
{
    private readonly NPCBased npc;
    private float idleTimeMin;
    private float idleTimeMax;
    private float idleTimer;
    private float idleWaitTime;

    public IdleState(NPCBased npc, float idleTimeMin, float idleTimeMax)
    {
        this.npc = npc;
        this.idleTimeMin = idleTimeMin;
        this.idleTimeMax = idleTimeMax;
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
            npc.ChangeState(npc.WanderState);
        }
    }
}
