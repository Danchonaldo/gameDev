
using UnityEngine;

public class HighJumpPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.UnlockHighJump();
                Destroy(gameObject);
            }
        }
    }
}
