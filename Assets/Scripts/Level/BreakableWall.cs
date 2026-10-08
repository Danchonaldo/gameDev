using UnityEngine;

// Разрушаемая стена: ломается от Fire Attack.
public class BreakableWall : MonoBehaviour, IFireTarget
{
    [Tooltip("Сколько попаданий выдерживает стена.")]
    [SerializeField] private int hitPoints = 2;
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem breakEffect;

    private float shakeUntil;
    private Vector3 visualStart;

    void Awake()
    {
        if (visual != null) visualStart = visual.transform.localPosition;
    }

    void Update()
    {
        if (visual == null) return;
        visual.transform.localPosition = Time.time < shakeUntil
            ? visualStart + (Vector3)(Random.insideUnitCircle * 0.06f)
            : visualStart;
    }

    public void OnFireHit()
    {
        hitPoints--;
        if (hitPoints > 0)
        {
            shakeUntil = Time.time + 0.25f;
            if (visual != null) visual.color = Color.Lerp(visual.color, new Color(1f, 0.5f, 0.3f), 0.5f);
            if (hitEffect != null) hitEffect.Play();
            return;
        }

        if (breakEffect != null)
        {
            ParticleSystem fx = Instantiate(breakEffect, transform.position, Quaternion.identity);
            fx.Play();
        }
        Destroy(gameObject);
    }
}
