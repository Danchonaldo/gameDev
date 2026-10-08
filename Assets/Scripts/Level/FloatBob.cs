using UnityEngine;

// Плавное покачивание и «дыхание» коллекционных предметов.
public class FloatBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.15f;
    [SerializeField] private float frequency = 1.5f;
    [SerializeField] private float pulse = 0.08f;
    [SerializeField] private float spin = 0f;

    private Vector3 startLocalPos;
    private Vector3 startScale;
    private float phase;

    void Awake()
    {
        startLocalPos = transform.localPosition;
        startScale = transform.localScale;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float t = Time.time * frequency * Mathf.PI * 2f + phase;
        transform.localPosition = startLocalPos + Vector3.up * Mathf.Sin(t) * amplitude;
        transform.localScale = startScale * (1f + Mathf.Sin(t * 2f) * pulse);
        if (spin != 0f) transform.Rotate(0f, 0f, spin * Time.deltaTime);
    }
}
