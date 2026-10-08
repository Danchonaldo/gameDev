
using UnityEngine;

public class PlayerSwimming : MonoBehaviour
{
    public float swimSpeed = 4f;
    public float swimUpSpeed = 5f;
    public float waterGravity = 1.5f;
    public float swimmingDuration = 10f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerAnimation playerAnimation;

    private float normalGravity;
    private bool swimmingUnlocked = false;
    private bool inWater = false;
    private float swimmingTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimation>();

        normalGravity = rb.gravityScale;
    }

    public void UnlockSwimming()
    {
        swimmingUnlocked = true;
        swimmingTimer = swimmingDuration;

        if (playerAnimation != null)
            playerAnimation.SetSwimmingActive(true);

        if (inWater)
            StartSwimming();
    }

    public void EnterWater()
    {
        inWater = true;

        if (playerAnimation != null)
            playerAnimation.SetInWater(true);

        if (swimmingUnlocked)
            StartSwimming();
    }

    public void ExitWater()
    {
        inWater = false;
        rb.gravityScale = normalGravity;

        if (movement != null)
            movement.enabled = true;

        if (playerAnimation != null)
            playerAnimation.SetInWater(false);
    }

    void StartSwimming()
    {
        rb.gravityScale = waterGravity;

        if (movement != null)
            movement.enabled = false;
    }

    void Update()
    {
        // Отсчёт 10 секунд
        if (swimmingUnlocked)
        {
            swimmingTimer -= Time.deltaTime;

            if (swimmingTimer <= 0f)
            {
                swimmingUnlocked = false;
                swimmingTimer = 0f;

                rb.gravityScale = normalGravity;

                if (movement != null)
                    movement.enabled = true;

                if (playerAnimation != null)
                    playerAnimation.SetSwimmingActive(false);
            }
        }

        // Плавание, только пока способность активна
        if (!inWater || !swimmingUnlocked)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(
                horizontal * swimSpeed,
                swimUpSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                horizontal * swimSpeed,
                rb.linearVelocity.y
            );
        }
    }
}
