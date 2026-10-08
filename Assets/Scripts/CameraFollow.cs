using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    public float offsetX = 3f;

    [Header("Vertical follow (optional)")]
    [Tooltip("Следовать за игроком по Y (для высоких уровней). Выключено — камера держит свою высоту.")]
    public bool followY = false;
    public float offsetY = 1f;
    public float minY = -100f;
    public float maxY = 100f;

    void LateUpdate()
    {
        if (player == null) return;

        float y = followY
            ? Mathf.Clamp(player.position.y + offsetY, minY, maxY)
            : transform.position.y;

        Vector3 targetPosition = new Vector3(
            player.position.x + offsetX,
            y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
