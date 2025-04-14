using UnityEngine;
using Unity.Netcode;

public class Character : NetworkBehaviour
{
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    protected void AnimatorChange(string temp, bool trigger)
    {
        if(trigger)
        {
            animator.SetTrigger(temp);
        }
        else
        {
            animator.SetBool(temp, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
