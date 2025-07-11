using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public enum State
{
    Idle,
    Wander,
    Found,
    Collect,
    Sorting
}

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class NPCBased : MonoBehaviour
{
    [Header("Idle Settings")]
    [SerializeField] private float m_idleTimeMin = 1f;
    [SerializeField] private float m_idleTimeMax = 3f;

    [Header("Wander Settings")]
    [SerializeField] private float m_wanderSpeed = 2f;

    [Header("Found Settings")]
    [SerializeField] private float m_foundSpeed = 3f;

    [Header("Sorting Settings")]
    [SerializeField] private float m_sortingSpeed = 5f;

    [Header("References")]
    [SerializeField] private CircleCollider2D m_detectionCollider;
    [SerializeField] private Transform m_handTransform;

    [Header("Sorting Areas")]
    [SerializeField] private SortingArea[] m_sortingAreaArray;

    private State m_state = State.Idle;
    private State m_previousState;

    private Box currentBox;
    private Box foundedBox;
    private Rigidbody2D rigid2D;
    private Coroutine movementCoroutine;
    private Coroutine idleCoroutine;

    void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        ChangeState(State.Idle);
    }

    void Update()
    {
        switch (m_state)
        {
            case State.Idle:
                HandleIdleState();
                break;
            case State.Wander:
                HandleWanderState();
                break;
            case State.Found:
                HandleFoundState();
                break;
            case State.Collect:
                HandleCollectState();
                break;
            case State.Sorting:
                HandleSortingState();
                break;
        }
    }

    private void ChangeState(State newState)
    {
        if (m_state == newState) return;

        m_previousState = m_state;
        m_state = newState;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        Debug.Log($"State changed from {m_previousState} to {m_state}");
    }

    private void HandleIdleState()
    {
        if (foundedBox == null && currentBox == null)
        {
            if (idleCoroutine == null)
            {
                idleCoroutine = StartCoroutine(WaitToPatrol());
            }
        }
    }

    private IEnumerator WaitToPatrol()
    {
        float waitTime = UnityEngine.Random.Range(m_idleTimeMin, m_idleTimeMax);
        yield return new WaitForSeconds(waitTime);
        ChangeState(State.Wander);
    }

    private void HandleWanderState()
    {
        if (foundedBox == null)
        {
            var targetBox = FindNearestBox();
            if (targetBox == null)
            {
                if (movementCoroutine == null) StartPatrolMovement();
                else Debug.Log("Already patrolling, waiting for next box.");
            }
            else
            {
                foundedBox = targetBox;
                ChangeState(State.Found);
            }
        }
        else
        {
            ChangeState(State.Found);
        }

        Box FindNearestBox()
        {
            var colliders = Physics2D.OverlapCircleAll(m_detectionCollider.transform.position, m_detectionCollider.radius);
            foreach (var box in colliders.Select(x => x.GetComponent<Box>())
                                         .Where(x => x != null && x.IsOnGround())
                                         .OrderBy(x => Vector2.Distance(transform.position, x.transform.position)))
            {
                ChangeState(State.Found);
                return box;
            }

            return null;
        }
    }

    private void HandleFoundState()
    {
        if (foundedBox == null)
        {
            ChangeState(State.Wander);
            return;
        }

        if (movementCoroutine == null)
        {
            Debug.Log($"Found a box: {foundedBox.name}");
            movementCoroutine = StartCoroutine(GoToTarget(foundedBox.transform.position, m_foundSpeed, OnReachedBox));
        }

        void OnReachedBox()
        {
            movementCoroutine = null;
            ChangeState(State.Collect);
        }
    }

    private void HandleCollectState()
    {
        InteractWithBox(foundedBox);
    }

    private void HandleSortingState()
    {
        if (movementCoroutine != null)
        {
            return;
        }

        Debug.Log($"Sorting the box {currentBox.name}");

        var sortingArea = m_sortingAreaArray.FirstOrDefault(area => area.IsBoxAccepted(currentBox));
        if (sortingArea == null)
        {
            Debug.LogWarning($"No sorting area found for box {currentBox.name}");
            ChangeState(State.Idle);
        }
        else
        {
            movementCoroutine = StartCoroutine(GoToTarget(sortingArea.transform.position, m_sortingSpeed, () => OnReachedSortingArea(sortingArea)));
        }

        void OnReachedSortingArea(SortingArea sortingArea)
        {
            sortingArea.DepositBox(currentBox);
            currentBox = null;
            ChangeState(State.Idle);
        }
    }

    private void StartPatrolMovement()
    {
        var randomPosition = transform.position + new Vector3(
            UnityEngine.Random.Range(-m_detectionCollider.radius, m_detectionCollider.radius),
            0f
        );

        randomPosition = ClampToSortingArea(randomPosition);

        Debug.Log($"Patrolling to position: {randomPosition}");

        movementCoroutine = StartCoroutine(GoToTarget(randomPosition, m_wanderSpeed, OnReachedPatrolPoint));

        Vector3 ClampToSortingArea(Vector3 position)
        {
            var offset = Mathf.Abs(transform.localScale.x);
            var min = m_sortingAreaArray.Min(area => area.transform.position.x) + offset;
            var max = m_sortingAreaArray.Max(area => area.transform.position.x) - offset;
            return new Vector3(Mathf.Clamp(position.x, min, max), position.y, position.z);
        }

        void OnReachedPatrolPoint()
        {
            movementCoroutine = null;
            ChangeState(State.Idle);
        }
    }


    private void InteractWithBox(Box detectedBox)
    {
        currentBox = detectedBox;
        currentBox.transform.SetParent(transform);
        currentBox.SetHolding();
        currentBox.transform.position = m_handTransform.position;
        currentBox.transform.localRotation = Quaternion.identity;

        ChangeState(State.Sorting);

        this.foundedBox = null;
    }

    private IEnumerator GoToTarget(Vector2 position, float speed, Action onReached = null)
    {
        Debug.DrawLine(transform.position, position, Color.white, 2f);

        var isMovingRight = position.x > transform.position.x;
        FlipSprite(isMovingRight);

        while (Mathf.Abs(transform.position.x - position.x) > 0.5f)
        {
            rigid2D.position = Vector3.MoveTowards(transform.position, position, speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        onReached?.Invoke();

        void FlipSprite(bool faceRight)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);
            transform.localScale = scale;
        }
    }
}
