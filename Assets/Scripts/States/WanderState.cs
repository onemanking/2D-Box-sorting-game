using System.Linq;
using UnityEngine;

public class WanderState : IState
{
    private readonly NPCBased npc;
    private readonly StateMachine stateMachine;
    private readonly CircleCollider2D detectionCollider;
    private readonly float wanderSpeed;
    private readonly float wanderXMinLimit;
    private readonly float wanderXMaxLimit;

    public WanderState(NPCBased npc, StateMachine stateMachine, float wanderXMinLimit, float wanderXMaxLimit)
    {
        this.npc = npc;
        this.stateMachine = stateMachine;
        this.wanderXMinLimit = wanderXMinLimit;
        this.wanderXMaxLimit = wanderXMaxLimit;
        detectionCollider = npc.DetectionCollider;
        wanderSpeed = npc.StateConfig.WanderSpeed;
    }

    void IState.Enter()
    {
        Debug.Log("Entering Wander State");
        npc.StopMovementCoroutine();
        StartPatrolMovement();
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Wander State");
        npc.StopMovementCoroutine();
    }

    void IState.Update()
    {
        // if (npc.FoundedBox == null)
        // {
        //     var targetBox = FindNearestBox();
        //     if (!targetBox)
        //     {
        //         StartPatrolMovement();
        //     }
        //     else
        //     {
        //         npc.SetFoundedBox(targetBox);
        //         stateMachine.ChangeState(npc.FoundState);
        //     }
        // }
        // else
        // {
        //     stateMachine.ChangeState(npc.FoundState);
        // }

        // Only check for boxes while wandering, not when standing still
        if (!npc.IsMoving)
        {
            stateMachine.ChangeState(npc.IdleState);
            return;
        }

        // Check for boxes during movement
        var targetBox = FindNearestBox();
        Debug.Log($"Found box: {targetBox?.name ?? "None"}");
        if (targetBox != null)
        {
            npc.SetFoundedBox(targetBox);
            stateMachine.ChangeState(npc.FoundState);
        }
    }

    // private Box FindNearestBox()
    // {
    //     var colliders = Physics2D.OverlapCircleAll(detectionCollider.transform.position, detectionCollider.radius);
    //     foreach (var box in colliders.Select(x => x.GetComponent<Box>())
    //                                  .Where(x => x != null && x.IsOnGround())
    //                                  .OrderBy(x => Vector2.Distance(npc.transform.position, x.transform.position)))
    //     {
    //         stateMachine.ChangeState(npc.FoundState);
    //         return box;
    //     }

    //     return null;
    // }

    private Box FindNearestBox()
    {
        var colliders = Physics2D.OverlapCircleAll(detectionCollider.transform.position, detectionCollider.radius);

        return colliders
            .Select(col => col.GetComponent<Box>())
            .Where(box => box != null && box.IsOnGround())
            .OrderBy(box => Vector2.Distance(npc.transform.position, box.transform.position))
            .FirstOrDefault();
    }

    private void StartPatrolMovement()
    {
        // var randomPosition = GenerateRandomPatrolPosition();

        // randomPosition = ClampToLimitArea(randomPosition);

        // Debug.Log($"Patrolling to position: {randomPosition}");

        // npc.StartMovementCoroutine(randomPosition, wanderSpeed, OnReachedPatrolPoint);

        // Vector3 ClampToLimitArea(Vector3 position)
        // {
        //     return new Vector3(Mathf.Clamp(position.x, wanderXMinLimit, wanderXMaxLimit), position.y, position.z);
        // }

        if (npc.IsMoving) return;

        var randomPosition = GenerateRandomPatrolPosition();

        Debug.Log($"Patrolling to position: {randomPosition}");
        npc.StartMovementCoroutine(randomPosition, wanderSpeed, OnReachedPatrolPoint);
    }

    private Vector3 GenerateRandomPatrolPosition()
    {
        var randomOffset = new Vector3(
            Random.Range(-detectionCollider.radius, detectionCollider.radius),
            0f
        );

        var targetPosition = npc.transform.position + randomOffset;

        targetPosition.x = Mathf.Clamp(targetPosition.x, wanderXMinLimit, wanderXMaxLimit);

        return targetPosition;
    }

    private void OnReachedPatrolPoint()
    {
        stateMachine.ChangeState(npc.IdleState);
    }
}
