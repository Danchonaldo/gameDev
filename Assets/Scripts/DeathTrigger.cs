
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerDeath player = other.GetComponent<PlayerDeath>();

        if (player != null)
        {
            player.Die();
        }
    }
}
