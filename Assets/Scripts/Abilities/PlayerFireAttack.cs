using System;
using UnityEngine;

// Fire Attack (DNA: Red + Triangle). Пока способность активна — [F] выпускает огненный снаряд.
// Снаряд разрушает BreakableWall и уничтожаемые огнём ловушки.
public class PlayerFireAttack : MonoBehaviour
{
    [SerializeField] private FireProjectile projectilePrefab;
    [SerializeField] private KeyCode fireKey = KeyCode.F;
    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0.9f, 0.1f);

    private PlayerAbilities abilities;
    private PlayerRespawn respawn;
    private Collider2D body;
    private float nextFireTime;

    public float Facing { get; private set; } = 1f;

    public event Action<Vector3, float> Fired; // позиция выстрела, направление (+1/-1)

    void Awake()
    {
        abilities = GetComponent<PlayerAbilities>();
        respawn = GetComponent<PlayerRespawn>();
        body = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        float h = GameInput.Horizontal;
        if (h > 0.1f) Facing = 1f;
        else if (h < -0.1f) Facing = -1f;

        if (respawn != null && respawn.IsDead) return;
        if (abilities == null || !abilities.IsActive(DNAType.Fire)) return;

        if (GameInput.GetKeyDown(fireKey) && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        nextFireTime = Time.time + fireCooldown;
        Vector3 center = body != null ? (Vector3)body.bounds.center : transform.position;
        Vector3 pos = center + new Vector3(spawnOffset.x * Facing, spawnOffset.y, 0f);

        if (projectilePrefab != null)
        {
            FireProjectile p = Instantiate(projectilePrefab, pos, Quaternion.identity);
            p.Launch(Facing, gameObject);
        }

        Fired?.Invoke(pos, Facing);
    }
}
