using System;
using System.Collections;
using System.Linq;
using UnityEngine;

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
    [field: SerializeField] internal CircleCollider2D DetectionCollider { get; private set; }
    [field: SerializeField] internal Transform HandTransform { get; private set; }

    [Header("Sorting Areas")]
    [SerializeField] private SortingArea[] m_sortingAreaArray;

    private StateMachine stateMachine;

    internal Box CurrentBox { get; private set; }
    internal Box FoundedBox { get; private set; }
    private Rigidbody2D rigid2D;
    private Coroutine movementCoroutine;
    internal bool IsMoving => movementCoroutine != null;

    void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();

        stateMachine = new StateMachine();

        IdleState = new IdleState(this, stateMachine);

        InitSortingAreas();

        var offset = Mathf.Abs(transform.localScale.x);
        var wanderXMinLimit = m_sortingAreaArray.Min(area => area.transform.position.x) + offset;
        var wanderXMaxLimit = m_sortingAreaArray.Max(area => area.transform.position.x) - offset;
        WanderState = new WanderState(this, stateMachine, wanderXMinLimit, wanderXMaxLimit);

        FoundState = new FoundState(this, stateMachine);
        CollectState = new CollectState(this, stateMachine);
        SortingState = new SortingState(this, stateMachine, m_sortingAreaArray);

        stateMachine.Init(IdleState);
    }

    void Update()
    {
        stateMachine?.Update();
    }

    private void InitSortingAreas()
    {
        if (m_sortingAreaArray == null || m_sortingAreaArray.Length == 0)
        {
            m_sortingAreaArray = FindObjectsByType<SortingArea>(FindObjectsSortMode.None);
        }
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

    internal void SetFoundedBox(Box box) => FoundedBox = box;
    internal void SetCurrentBox(Box box) => CurrentBox = box;

    internal void StartMovementCoroutine(Vector2 target, float speed, Action onComplete = null)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(GoToTarget(target, speed, onComplete));
    }

    internal void StopMovementCoroutine()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }
}
