
using UnityEngine;

public class BlueCirclePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerSwimming swimming =
            other.GetComponent<PlayerSwimming>();

        if (swimming != null)
        {
            swimming.UnlockSwimming();
            gameObject.SetActive(false);
        }
    }
}
