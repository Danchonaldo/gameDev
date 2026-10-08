using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 9.5f;

    [Header("Jump Feel")]
    [Tooltip("Базовая гравитация Rigidbody2D. Больше = быстрее и резче прыжок.")]
    [SerializeField] private float gravityScale = 2f;
    [Tooltip("Множитель гравитации при падении (падаем быстрее, чем взлетаем).")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;
    [Tooltip("Множитель гравитации, если Space отпущен на подъёме (короткий прыжок).")]
    [SerializeField] private float jumpGravityMultiplier = 1.8f;
    [Tooltip("Максимальная скорость падения.")]
    [SerializeField] private float maxFallSpeed = 18f;
    [Tooltip("Сила прыжка, пока активна способность High Jump.")]
    [SerializeField] private float highJumpForce = 15.5f;

    private Rigidbody2D rb;
    private PlayerAbilities abilities;
    private bool isGrounded = false;
    private bool jumpHeld;

    public bool IsGrounded => isGrounded;

    // true — вертикальной физикой управляет другой скрипт (например, плавание).
    public bool ExternalControl { get; set; }

    // Вызывается при каждом прыжке. Аргумент: true = High Jump.
    public event System.Action<bool> Jumped;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilities>();
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        // Движение
        float move = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            move * moveSpeed,
            rb.linearVelocity.y
        );

        jumpHeld = Input.GetKey(KeyCode.Space);

        // Один прыжок на одно нажатие Space
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !ExternalControl)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!ExternalControl)
        {
            ApplyJumpGravity(jumpHeld);
        }
    }

    public void Jump()
    {
        bool highJump = abilities != null && abilities.IsActive(DNAType.HighJump);
        float force = highJump ? highJumpForce : jumpForce;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            force
        );

        isGrounded = false;
        Jumped?.Invoke(highJump);
    }

    // Быстрый подъём, ещё более быстрое падение, короткий прыжок при отпущенном Space.
    public void ApplyJumpGravity(bool isJumpHeld)
    {
        float vy = rb.linearVelocity.y;
        float multiplier = 1f;

        if (vy < -0.01f)
            multiplier = fallGravityMultiplier;
        else if (vy > 0.01f && !isJumpHeld)
            multiplier = jumpGravityMultiplier;

        rb.gravityScale = gravityScale * multiplier;

        if (vy < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground")) return;

        // Земля — только контакт снизу. Касание боковой стороны стены больше не
        // даёт прыгать (иначе высокую стену можно «пропрыгать» без High Jump).
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("YellowTriangle"))
    {
        // High Jump теперь выдаётся как DNA-способность с ограничением по времени.
        if (abilities != null)
            abilities.Collect(DNAType.HighJump);
        else
            jumpForce = highJumpForce;

        Destroy(other.gameObject);

        Debug.Log("HIGH JUMP UNLOCKED!");
    }

    if (other.CompareTag("Finish"))
{
    Debug.Log("LEVEL COMPLETE!");

    LevelManager levelManager = FindAnyObjectByType<LevelManager>();
    if (levelManager != null)
        levelManager.CompleteLevel();
}
}
}
