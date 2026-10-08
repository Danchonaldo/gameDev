using UnityEngine;

// Ловушка: шипы, шипастые шары, падение за карту.
// Убивает игрока при касании. Armor защищает (игрок отскакивает), если armorProtects = true.
public class Hazard : MonoBehaviour, IFireTarget
{
    [Tooltip("Armor защищает от этой ловушки (для пропасти/DeathZone выключить).")]
    [SerializeField] private bool armorProtects = true;
    [Tooltip("Скорость отскока, когда Armor блокирует урон.")]
    [SerializeField] private float armorBounce = 9f;
    [Tooltip("Можно уничтожить Fire Attack.")]
    [SerializeField] private bool destroyedByFire = false;
    [SerializeField] private ParticleSystem destroyEffect;
    [SerializeField] private string deathCause = "Hazard";

    private float nextBounceTime;

    void OnTriggerEnter2D(Collider2D other) => Touch(other);
    void OnTriggerStay2D(Collider2D other) => Touch(other);
    void OnCollisionEnter2D(Collision2D collision) => Touch(collision.collider);
    void OnCollisionStay2D(Collision2D collision) => Touch(collision.collider);

    private void Touch(Collider2D other)
    {
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null || player.IsDead) return;

        if (armorProtects)
        {
            PlayerAbilities abilities = player.GetComponent<PlayerAbilities>();
            if (abilities != null && abilities.IsActive(DNAType.Armor))
            {
                if (Time.time >= nextBounceTime)
                {
                    nextBounceTime = Time.time + 0.15f;
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, armorBounce);
                    PlayerFX fx = player.GetComponent<PlayerFX>();
                    if (fx != null) fx.PlayArmorBlock();
                }
                return;
            }
        }

        player.Die(deathCause, !armorProtects);
    }

    public void OnFireHit()
    {
        if (!destroyedByFire) return;
        if (destroyEffect != null)
        {
            ParticleSystem fx = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            fx.Play();
        }
        Destroy(gameObject);
    }
}
