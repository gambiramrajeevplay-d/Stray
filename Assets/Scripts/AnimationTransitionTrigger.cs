using System.Collections;
using UnityEngine;

public class AnimationTransitionTrigger : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation")]
    [SerializeField] private string triggerName = "ShowTransition";
    [SerializeField] private float delay = 1f;

    private void Start()
    {
        StartCoroutine(PlayAnimationAfterDelay());
    }

    private IEnumerator PlayAnimationAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}