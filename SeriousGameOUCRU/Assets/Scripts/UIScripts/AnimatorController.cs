using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }

    public void EnableAnimator()
    {
        Debug.Log("ENABLE");
        animator.enabled = true;
    }
}
