using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class NPCBased : MonoBehaviour
{
    [Header("State Configuration")]
    [field: SerializeField] internal NPCBaseStateConfigData StateConfig { get; private set; }

    [Header("References")]
    [field: SerializeField] internal CircleCollider2D DetectionCollider { get; private set; }
    [field: SerializeField] internal Transform HandTransform { get; private set; }

    [Header("Sorting Areas")]
    [SerializeField] private SortingArea[] m_sortingAreaArray;

    internal IdleState IdleState { get; private set; }
    internal SearchState SearchState { get; private set; }
    internal FoundState FoundState { get; private set; }
    internal CollectState CollectState { get; private set; }
    internal SortingState SortingState { get; private set; }

    internal Box CurrentBox { get; private set; }
    internal Box FoundedBox { get; private set; }
    internal AnimationController AnimationController { get; private set; }
    internal bool IsMoving => movementCoroutine != null;

    private StateMachine stateMachine;
    private Rigidbody2D rigid2D;
    private Coroutine movementCoroutine;

    void Start()
    {
        rigid2D = GetComponent<Rigidbody2D>();
        AnimationController = GetComponent<AnimationController>();

        stateMachine = new StateMachine();

        IdleState = new IdleState(this, stateMachine);

        InitSortingAreas();

        var offset = Mathf.Abs(transform.localScale.x);
        var searchXMinLimit = m_sortingAreaArray.Min(area => area.transform.position.x) + offset;
        var searchXMaxLimit = m_sortingAreaArray.Max(area => area.transform.position.x) - offset;
        SearchState = new SearchState(this, stateMachine, searchXMinLimit, searchXMaxLimit);

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

        PlayAnimation("Walk");

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
            float targetYRotation = faceRight ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
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

    internal void PlayAnimation(string animationName, Action callback = null)
    {
        AnimationController ??= GetComponent<AnimationController>();

        AnimationController.PlayAnimation(animationName, callback);
    }

    internal void PlayAnimation(IState state, Action callback = null)
    {
        AnimationController ??= GetComponent<AnimationController>();

        AnimationController.PlayAnimation(state, callback);
    }
}
