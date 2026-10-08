
using UnityEngine;

public class WaterZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerSwimming player = other.GetComponent<PlayerSwimming>();

        if (player != null)
            player.EnterWater();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerSwimming player = other.GetComponent<PlayerSwimming>();

        if (player != null)
            player.ExitWater();
    }
}
