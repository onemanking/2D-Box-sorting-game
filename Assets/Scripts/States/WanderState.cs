using System.Linq;
using UnityEngine;

public class WanderState : IState
{
    private NPCBased npc;
    private CircleCollider2D detectionCollider;
    private float wanderSpeed;
    private float warderXMinLimit;
    private float warderXMaxLimit;
    private Coroutine movementCoroutine;

    public WanderState(NPCBased npc, CircleCollider2D detectionCollider, float wanderSpeed, float warderXMinLimit, float warderXMaxLimit)
    {
        this.npc = npc;
        this.detectionCollider = detectionCollider;
        this.wanderSpeed = wanderSpeed;
        this.warderXMinLimit = warderXMinLimit;
        this.warderXMaxLimit = warderXMaxLimit;
    }

    void IState.Enter()
    {
        Debug.Log("Entering Wander State");
        if (movementCoroutine != null)
        {
            npc.StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    void IState.Update()
    {
        if (npc.FoundedBox == null)
        {
            var targetBox = FindNearestBox();
            if (!targetBox)
            {
                StartPatrolMovement();
            }
            else
            {
                npc.SetFoundedBox(targetBox);
                npc.ChangeState(npc.FoundState);
            }
        }
        else
        {
            npc.ChangeState(npc.FoundState);
        }
    }

    void IState.Exit()
    {
        Debug.Log("Exiting Wander State");
        if (movementCoroutine != null)
        {
            npc.StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    private Box FindNearestBox()
    {
        var colliders = Physics2D.OverlapCircleAll(detectionCollider.transform.position, detectionCollider.radius);
        foreach (var box in colliders.Select(x => x.GetComponent<Box>())
                                     .Where(x => x != null && x.IsOnGround())
                                     .OrderBy(x => Vector2.Distance(npc.transform.position, x.transform.position)))
        {
            npc.ChangeState(npc.FoundState);
            return box;
        }

        return null;
    }

    private void StartPatrolMovement()
    {
        var randomPosition = npc.transform.position + new Vector3(
            Random.Range(-detectionCollider.radius, detectionCollider.radius),
            0f
        );

        randomPosition = ClampToLimitArea(randomPosition);

        Debug.Log($"Patrolling to position: {randomPosition}");

        movementCoroutine = npc.StartCoroutine(npc.GoToTarget(randomPosition, wanderSpeed, OnReachedPatrolPoint));

        Vector3 ClampToLimitArea(Vector3 position)
        {
            return new Vector3(Mathf.Clamp(position.x, warderXMinLimit, warderXMaxLimit), position.y, position.z);
        }

        void OnReachedPatrolPoint()
        {
            npc.ChangeState(npc.IdleState);
        }
    }
}
