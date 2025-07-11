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
    public IdleState IdleState { get; private set; }
    public WanderState WanderState { get; private set; }
    public FoundState FoundState { get; private set; }
    public CollectState CollectState { get; private set; }
    public SortingState SortingState { get; private set; }

    [Header("State Configuration")]
    [field: SerializeField] internal NPCBaseStateConfigData StateConfig { get; private set; }
    [Header("References")]
    [SerializeField] private CircleCollider2D m_detectionCollider;
    [SerializeField] private Transform m_handTransform;

    [Header("Sorting Areas")]
    [SerializeField] private SortingArea[] m_sortingAreaArray;

    private State m_state = State.Idle;
    private State m_previousState;

    internal Box CurrentBox;
    internal Box FoundedBox;
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

        HandleUpdateState();
    }

    private void HandleUpdateState()
    {
        if (CurrentState != null && inStateTransition)
        {
            CurrentState.Update();
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
        if (FoundedBox == null && CurrentBox == null)
        {
            if (idleCoroutine == null)
            {
                idleCoroutine = StartCoroutine(WaitToPatrol());
            }
        }
    }

    private IEnumerator WaitToPatrol()
    {
        float waitTime = UnityEngine.Random.Range(StateConfig.IdleTimeMin, StateConfig.IdleTimeMax);
        yield return new WaitForSeconds(waitTime);
        ChangeState(State.Wander);
    }

    private void HandleWanderState()
    {
        var colliders = Physics2D.OverlapCircleAll(m_detectionCollider.transform.position, m_detectionCollider.radius);
        foreach (var box in colliders.Select(x => x.GetComponent<Box>())
                                     .Where(x => x != null && x.IsOnGround())
                                     .OrderBy(x => Vector2.Distance(transform.position, x.transform.position)))
        {
            FoundedBox = box;
            ChangeState(State.Found);
            return;
        }

        if (movementCoroutine == null)
        {
            StartPatrolMovement();
        }
    }

    private void HandleFoundState()
    {
        if (FoundedBox == null)
        {
            ChangeState(State.Wander);
            return;
        }

        if (movementCoroutine == null)
        {
            Debug.Log($"Found a box: {FoundedBox.name}");
            movementCoroutine = StartCoroutine(GoToTarget(FoundedBox.transform.position, StateConfig.FoundSpeed, OnReachedBox));
        }

        void OnReachedBox()
        {
            movementCoroutine = null;
            ChangeState(State.Collect);
        }
    }

    private void HandleCollectState()
    {
        InteractWithBox(FoundedBox);
    }

    private void HandleSortingState()
    {
        if (movementCoroutine != null)
        {
            return;
        }

        Debug.Log($"Sorting the box {CurrentBox.name}");

        var sortingArea = m_sortingAreaArray.FirstOrDefault(area => area.IsBoxAccepted(CurrentBox));
        if (sortingArea == null)
        {
            Debug.LogWarning($"No sorting area found for box {CurrentBox.name}");
            ChangeState(State.Idle);
        }
        else
        {
            movementCoroutine = StartCoroutine(GoToTarget(sortingArea.transform.position, StateConfig.SortingSpeed, () => OnReachedSortingArea(sortingArea)));
        }

        void OnReachedSortingArea(SortingArea sortingArea)
        {
            sortingArea.DepositBox(CurrentBox);
            CurrentBox = null;
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

        movementCoroutine = StartCoroutine(GoToTarget(randomPosition, StateConfig.WanderSpeed, OnReachedPatrolPoint));

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
        CurrentBox = detectedBox;
        CurrentBox.transform.SetParent(transform);
        CurrentBox.SetHolding();
        CurrentBox.transform.position = m_handTransform.position;
        CurrentBox.transform.localRotation = Quaternion.identity;

        ChangeState(State.Sorting);

        this.FoundedBox = null;
    }

    internal IEnumerator GoToTarget(Vector2 position, float speed, Action onReached = null)
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

    internal IState CurrentState { get; private set; }
    internal IState PreviousState { get; private set; }

    internal void SetFoundedBox(Box box) => FoundedBox = box;

    internal void SetCurrentBox(Box box) => CurrentBox = box;

    private bool inStateTransition = false;
    internal void ChangeState(IState state)
    {
        if (CurrentState == state || inStateTransition) return;

        inStateTransition = true;

        CurrentState?.Exit();

        PreviousState = CurrentState;

        CurrentState = state;

        CurrentState?.Enter();

        Debug.Log($"State changed from {PreviousState} to {CurrentState}");
    }
}
