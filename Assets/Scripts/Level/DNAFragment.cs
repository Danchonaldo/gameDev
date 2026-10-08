using UnityEngine;

// Коллекционный DNA Fragment: при сборе исчезает и увеличивает счётчик в LevelManager.
[RequireComponent(typeof(Collider2D))]
public class DNAFragment : MonoBehaviour
{
    [SerializeField] private ParticleSystem collectEffect;

    private bool collected;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null || player.IsDead) return;

        collected = true;
        if (LevelManager.Instance != null) LevelManager.Instance.CollectFragment();

        if (collectEffect != null)
        {
            ParticleSystem fx = Instantiate(collectEffect, transform.position, Quaternion.identity);
            fx.Play();
        }
        Destroy(gameObject);
    }
}
