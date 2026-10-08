using UnityEngine;

// Вода: триггер-зона. Логику плавания/утопления выполняет PlayerSwim.
// Визуал (спрайт воды) мягко покачивается, чтобы вода выглядела «живой».
[RequireComponent(typeof(BoxCollider2D))]
public class WaterZone : MonoBehaviour
{
    [SerializeField] private Transform waveVisual;
    [SerializeField] private float waveAmplitude = 0.05f;
    [SerializeField] private float waveSpeed = 1.2f;

    private BoxCollider2D box;
    private Vector3 visualStart;

    public float SurfaceY => box.bounds.max.y;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (waveVisual != null) visualStart = waveVisual.localPosition;
    }

    void Update()
    {
        if (waveVisual == null) return;
        waveVisual.localPosition = visualStart + Vector3.up * Mathf.Sin(Time.time * waveSpeed * Mathf.PI * 2f) * waveAmplitude;
    }
}
