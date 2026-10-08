
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool highJumpActive = false;
    private bool swimmingActive = false;
    private bool inWater = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetHighJumpActive(bool active)
    {
        highJumpActive = active;
    }

    public void SetSwimmingActive(bool active)
    {
        swimmingActive = active;
    }

    public void SetInWater(bool active)
    {
        inWater = active;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0)
        {
            spriteRenderer.flipX = horizontal < 0;
        }

        bool jumping = Mathf.Abs(rb.linearVelocity.y) > 0.15f;
        bool walking = Mathf.Abs(horizontal) > 0.1f;

        if (swimmingActive && inWater)
        {
            PlayAnimation("BlueBlobSwim");
        }
        else if (highJumpActive)
        {
            if (jumping)
                PlayAnimation("HighJumpJump");
            else
                PlayAnimation("HighJumpIdle");
        }
        else if (swimmingActive)
        {
            PlayAnimation("BlueBlobIdle");
        }
        else
        {
            if (jumping)
                PlayAnimation("BlobJump");
            else if (walking)
                PlayAnimation("BlobWalk");
            else
                PlayAnimation("BlobIdle");
        }
    }

    void PlayAnimation(string animationName)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0)
            .IsName(animationName))
        {
            animator.Play(animationName);
        }
    }
}
