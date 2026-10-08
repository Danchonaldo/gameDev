
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 9f;
    public float highJumpForce = 14f;
    public float highJumpDuration = 3f;

    private bool hasHighJump = false;
    private float highJumpTimer = 0f;

    private Rigidbody2D rb;
    private PlayerAnimation playerAnimation;

    private float moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float currentJump = hasHighJump
                ? highJumpForce
                : jumpForce;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                currentJump
            );

            isGrounded = false;
        }

        if (hasHighJump)
        {
            highJumpTimer -= Time.deltaTime;

            if (highJumpTimer <= 0f)
            {
                hasHighJump = false;
                highJumpTimer = 0f;

                if (playerAnimation != null)
                    playerAnimation.SetHighJumpActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    public void UnlockHighJump()
    {
        hasHighJump = true;
        highJumpTimer = highJumpDuration;

        if (playerAnimation != null)
            playerAnimation.SetHighJumpActive(true);
    }
}
