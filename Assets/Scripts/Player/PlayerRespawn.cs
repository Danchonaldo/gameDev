using System;
using System.Collections;
using UnityEngine;

// Смерть и Respawn игрока: Death Effect → короткая задержка → Respawn на чекпоинте (или на старте).
// Визуальную часть (анимацию смерти) проигрывает PlayerFX по событию Died.
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("Длительность анимации смерти.")]
    [SerializeField] private float deathAnimationTime = 0.8f;
    [Tooltip("Пауза после анимации перед Respawn.")]
    [SerializeField] private float respawnDelay = 0.25f;
    [Tooltip("Неуязвимость к ловушкам сразу после Respawn.")]
    [SerializeField] private float invulnerabilityTime = 1f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerAbilities abilities;
    private Vector3 startPosition;
    private float invulnerableUntil;

    public bool IsDead { get; private set; }
    public bool IsInvulnerable => Time.time < invulnerableUntil;
    public float DeathAnimationTime => deathAnimationTime;
    public float InvulnerabilityTime => invulnerabilityTime;

    public event Action<string> Died;   // аргумент — причина ("Hazard", "Water", "Fall")
    public event Action Respawned;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        abilities = GetComponent<PlayerAbilities>();
        startPosition = transform.position;
    }

    public Vector3 RespawnPoint
    {
        get
        {
            LevelManager lm = LevelManager.Instance;
            return lm != null ? lm.GetRespawnPoint(startPosition) : startPosition;
        }
    }

    // ignoreInvulnerability = true для воды и падения за карту.
    public void Die(string cause, bool ignoreInvulnerability = false)
    {
        if (IsDead) return;
        if (IsInvulnerable && !ignoreInvulnerability) return;
        StartCoroutine(DeathRoutine(cause));
    }

    private IEnumerator DeathRoutine(string cause)
    {
        IsDead = true;

        if (movement != null)
        {
            movement.enabled = false;
            movement.ExternalControl = false;
        }
        if (abilities != null)
        {
            abilities.ResetAfterDeath();
            abilities.InputEnabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (LevelManager.Instance != null) LevelManager.Instance.RegisterDeath();
        Died?.Invoke(cause);

        yield return new WaitForSeconds(deathAnimationTime + respawnDelay);

        transform.position = RespawnPoint;
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }
        if (movement != null) movement.enabled = true;
        if (abilities != null) abilities.InputEnabled = true;

        invulnerableUntil = Time.time + invulnerabilityTime;
        IsDead = false;
        Respawned?.Invoke();
    }
}
