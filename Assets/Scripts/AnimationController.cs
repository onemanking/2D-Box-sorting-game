using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private Coroutine animationCoroutine;

    internal void PlayAnimation(IState state, Action callback = null)
    {
        var animationName = state switch
        {
            IdleState => "Idle",
            SearchState => "Walk",
            FoundState => "Found",
            CollectState => "Collect",
            _ => throw new ArgumentException("Unsupported state type: " + state.GetType().Name)
        };

        PlayAnimation(animationName, callback);
    }

    internal void PlayAnimation(string animationName, Action callback = null)
    {
        if (string.IsNullOrEmpty(animationName))
        {
            throw new ArgumentException("Animation name cannot be null or empty", nameof(animationName));
        }

        animator ??= GetComponent<Animator>();

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (callback != null)
        {
            animator.Play(animationName, -1, 0);
            animationCoroutine = StartCoroutine(WaitForAnimationComplete(animationName, callback));
        }
        else
        {
            animator.Play(animationName, -1, 0);
        }
    }

    private IEnumerator WaitForAnimationComplete(string animationName, Action callback)
    {
        yield return null;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        }

        callback?.Invoke();
    }
}
