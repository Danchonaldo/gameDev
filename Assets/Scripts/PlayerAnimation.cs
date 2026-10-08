
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;
        }

        if (Mathf.Abs(rb.linearVelocity.y) > 0.15f)
        {
            animator.Play("BlobJump");
        }
        else if (Mathf.Abs(horizontal) > 0.1f)
        {
            animator.Play("BlobWalk");
        }
        else
        {
            animator.Play("BlobIdle");
        }
    }
}
