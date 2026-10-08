using UnityEngine;

// Двигает объект туда-обратно между стартовой точкой и start + offset (для подвижных ловушек).
public class PingPongMover : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0f, 3f);
    [Tooltip("Время прохода в одну сторону (сек).")]
    [SerializeField] private float travelTime = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float phase = 0f;
    [Tooltip("Вращение визуала (градусов/сек).")]
    [SerializeField] private float spin = 0f;
    [SerializeField] private Transform spinTarget;

    private Vector3 start;
    private Rigidbody2D rb;

    void Awake()
    {
        start = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float t = (Time.time / Mathf.Max(0.01f, travelTime)) + phase * 2f;
        float k = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI); // плавно 0..1..0
        Vector3 target = start + (Vector3)(offset * k);
        if (rb != null) rb.MovePosition(target);
        else transform.position = target;
    }

    void Update()
    {
        if (spin != 0f)
            (spinTarget != null ? spinTarget : transform).Rotate(0f, 0f, spin * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 a = Application.isPlaying ? start : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(a, a + (Vector3)offset);
    }
}
