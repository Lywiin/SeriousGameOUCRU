using UnityEngine;
using UnityEngine.UI;

public class TextBox : MonoBehaviour
{
    private Animator animator;
    private Button button;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        button = GetComponentInChildren<Button>();

        if (button) button.interactable = false;
    }


    public void SetActive()
    {
        gameObject.SetActive(true);
    }

    public void SetInactive()
    {
        gameObject.SetActive(false);
    }

    public void SetInteractable()
    {
        if (button)
        {
            button.interactable = true;
            animator.SetTrigger("ShineText");
        }
    }

    public void FadeOut()
    {
        if (button && AudioManager.Instance) AudioManager.Instance.Play("Select1");
        animator.SetTrigger("FadeOutTextBox");
    }
}
