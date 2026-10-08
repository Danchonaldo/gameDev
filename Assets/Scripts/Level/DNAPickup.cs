using UnityEngine;

// DNA-способность на уровне (Swim / Fire / Armor / High Jump).
// Желтый треугольник Level 1 работает через тег YellowTriangle в PlayerMovement — этот скрипт для остальных.
[RequireComponent(typeof(Collider2D))]
public class DNAPickup : MonoBehaviour
{
    [SerializeField] private DNAType type = DNAType.Swim;
    [SerializeField] private ParticleSystem collectEffect;

    public DNAType Type => type;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerAbilities abilities = other.GetComponentInParent<PlayerAbilities>();
        if (abilities == null) return;
        PlayerRespawn respawn = abilities.GetComponent<PlayerRespawn>();
        if (respawn != null && respawn.IsDead) return;

        abilities.Collect(type);

        if (collectEffect != null)
        {
            ParticleSystem fx = Instantiate(collectEffect, transform.position, Quaternion.identity);
            var main = fx.main;
            main.startColor = DNATypeInfo.GetColor(type);
            fx.Play();
        }
        Destroy(gameObject);
    }
}
