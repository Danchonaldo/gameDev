using UnityEngine;

// Чекпоинт: игрок прошёл через флаг → после смерти появляется здесь.
// Если ни один чекпоинт не активирован — Respawn на старте.
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer flag;
    [SerializeField] private Color inactiveColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color activeColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private ParticleSystem activateEffect;
    [Tooltip("Точка появления относительно флага.")]
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.2f);

    private bool activated;

    void Start()
    {
        if (flag != null) flag.color = inactiveColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null || player.IsDead) return;

        activated = true;
        if (flag != null) flag.color = activeColor;
        if (activateEffect != null) activateEffect.Play();

        if (LevelManager.Instance != null)
            LevelManager.Instance.SetCheckpoint(transform.position + (Vector3)respawnOffset);
    }
}
