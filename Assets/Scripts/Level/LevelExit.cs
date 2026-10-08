using UnityEngine;

// Exit уровня: игрок касается — LevelManager завершает уровень
// (Level 1 → Level 2 → Level 3 → GAME COMPLETE).
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [SerializeField] private ParticleSystem exitEffect;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawn player = other.GetComponentInParent<PlayerRespawn>();
        if (player == null || player.IsDead) return;

        LevelManager levelManager = LevelManager.Instance;
        if (levelManager == null || levelManager.IsCompleted) return;

        if (exitEffect != null) exitEffect.Play();
        levelManager.CompleteLevel();
    }
}
