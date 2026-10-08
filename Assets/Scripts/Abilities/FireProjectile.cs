using UnityEngine;

// Цель, которую можно поразить огнём (BreakableWall, уничтожаемые ловушки).
public interface IFireTarget
{
    void OnFireHit();
}

// Огненный снаряд Fire Attack.
[RequireComponent(typeof(Rigidbody2D))]
public class FireProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private Transform visual;

    private Rigidbody2D rb;
    private GameObject owner;
    private bool done;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(float direction, GameObject shooter)
    {
        owner = shooter;
        rb.linearVelocity = new Vector2(direction * speed, 0f);
        if (visual != null && direction < 0f)
        {
            Vector3 s = visual.localScale;
            visual.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (visual != null) visual.Rotate(0f, 0f, -spinSpeed * Time.deltaTime * Mathf.Sign(rb.linearVelocity.x));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (done || other.isTrigger && other.GetComponentInParent<IFireTarget>() == null) return;
        if (owner != null && other.transform.IsChildOf(owner.transform)) return;

        IFireTarget target = other.GetComponentInParent<IFireTarget>();
        if (target != null)
        {
            target.OnFireHit();
            Explode();
            return;
        }

        // Стены и земля гасят снаряд.
        if (!other.isTrigger) Explode();
    }

    private void Explode()
    {
        done = true;
        if (impactEffect != null)
        {
            ParticleSystem fx = Instantiate(impactEffect, transform.position, Quaternion.identity);
            fx.Play();
        }
        Destroy(gameObject);
    }
}
