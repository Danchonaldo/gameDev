
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public SpriteRenderer levelBackground;

    public float smoothSpeed = 5f;
    public float offsetX = 2f;

    private Camera cam;
    private float fixedY;

    void Start()
    {
        cam = GetComponent<Camera>();
        fixedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null || levelBackground == null) return;

        float halfWidth = cam.orthographicSize * cam.aspect;

        float leftEdge = levelBackground.bounds.min.x;
        float rightEdge = levelBackground.bounds.max.x;

        float desiredX = target.position.x + offsetX;

        // Не позволяем камере выходить за фон
        if (rightEdge - leftEdge > halfWidth * 2f)
        {
            desiredX = Mathf.Clamp(
                desiredX,
                leftEdge + halfWidth,
                rightEdge - halfWidth
            );
        }
        else
        {
            desiredX = (leftEdge + rightEdge) / 2f;
        }

        Vector3 desiredPosition = new Vector3(
            desiredX,
            fixedY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Mathf.Clamp01(smoothSpeed * Time.deltaTime)
        );
    }
}
