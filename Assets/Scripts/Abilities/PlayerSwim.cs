using System;
using UnityEngine;

// Плавание (DNA: Blue + Circle).
// В воде без активного Swim игрок тонет (смерть → Respawn). Со Swim — плывёт:
// Space / W / ↑ — вверх, S / ↓ — вниз. Всплыл с зажатым «вверх» — выпрыгивает из воды.
// Если время Swim закончилось под водой — игрок тонет.
public class PlayerSwim : MonoBehaviour
{
    [SerializeField] private float swimGravityScale = 0.35f;
    [SerializeField] private float swimUpSpeed = 5f;
    [SerializeField] private float swimDownSpeed = 4f;
    [SerializeField] private float maxSinkSpeed = 1.5f;
    [Tooltip("Скорость выпрыгивания из воды у поверхности.")]
    [SerializeField] private float exitJumpSpeed = 10f;
    [Tooltip("Гравитация во время выпрыгивания (одинаковая для Space и W/↑).")]
    [SerializeField] private float exitJumpGravityScale = 2f;
    [Tooltip("Глубина погружения (от низа коллайдера), после которой игрок считается в воде.")]
    [SerializeField] private float submergeDepth = 0.35f;
    [Tooltip("Сколько секунд без Swim можно пробыть в воде.")]
    [SerializeField] private float drownDelay = 0.2f;

    private Rigidbody2D rb;
    private Collider2D body;
    private PlayerMovement movement;
    private PlayerAbilities abilities;
    private PlayerRespawn respawn;

    private WaterZone water;
    private int waterOverlaps;
    private float drownTimer;
    private bool exitJumping;

    public bool IsSubmerged { get; private set; }
    public bool IsSwimming { get; private set; }

    public event Action<Vector3> Splashed;          // вход в воду
    public event Action<bool> SwimmingChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        movement = GetComponent<PlayerMovement>();
        abilities = GetComponent<PlayerAbilities>();
        respawn = GetComponent<PlayerRespawn>();
        if (respawn != null) respawn.Respawned += OnRespawned;
    }

    void OnDestroy()
    {
        if (respawn != null) respawn.Respawned -= OnRespawned;
    }

    private void OnRespawned()
    {
        waterOverlaps = 0;
        water = null;
        IsSubmerged = false;
        drownTimer = 0f;
        exitJumping = false;
        SetSwimming(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        WaterZone zone = other.GetComponent<WaterZone>();
        if (zone == null) return;
        waterOverlaps++;
        water = zone;
        Splashed?.Invoke(new Vector3(transform.position.x, zone.SurfaceY, 0f));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<WaterZone>() == null) return;
        waterOverlaps = Mathf.Max(0, waterOverlaps - 1);
        if (waterOverlaps == 0) water = null;
    }

    void Update()
    {
        if (respawn != null && respawn.IsDead) return;

        bool wasSubmerged = IsSubmerged;
        IsSubmerged = water != null && body != null &&
                      water.SurfaceY - body.bounds.min.y > submergeDepth;

        bool canSwim = abilities != null && abilities.IsActive(DNAType.Swim);

        if (IsSubmerged && !canSwim)
        {
            drownTimer += Time.deltaTime;
            if (drownTimer >= drownDelay && respawn != null)
            {
                respawn.Die("Water", true);
                return;
            }
        }
        else
        {
            drownTimer = 0f;
        }

        // Всплыли с зажатым «вверх» — выпрыгиваем на берег/платформу.
        if (IsSwimming && wasSubmerged && !IsSubmerged && GameInput.UpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, exitJumpSpeed);
            exitJumping = true;
        }

        SetSwimming(IsSubmerged && canSwim);
        if (movement != null) movement.ExternalControl = IsSwimming || exitJumping;
    }

    void FixedUpdate()
    {
        if (exitJumping)
        {
            // Выпрыгивание: обычная гравитация до верхней точки, потом управление возвращается PlayerMovement.
            if (IsSwimming || rb.linearVelocity.y <= 0f) exitJumping = false;
            else rb.gravityScale = exitJumpGravityScale;
        }

        if (!IsSwimming) return;

        rb.gravityScale = swimGravityScale;
        float vy = rb.linearVelocity.y;

        if (GameInput.UpHeld) vy = Mathf.MoveTowards(vy, swimUpSpeed, 40f * Time.fixedDeltaTime);
        else if (GameInput.DownHeld) vy = Mathf.MoveTowards(vy, -swimDownSpeed, 40f * Time.fixedDeltaTime);
        else vy = Mathf.Max(Mathf.MoveTowards(vy, 0f, 12f * Time.fixedDeltaTime), -maxSinkSpeed);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, vy);
    }

    private void SetSwimming(bool value)
    {
        if (IsSwimming == value) return;
        IsSwimming = value;
        if (movement != null) movement.ExternalControl = value;
        SwimmingChanged?.Invoke(value);
    }
}
