using System;
using System.Collections;
using UnityEngine;

// Визуальные эффекты игрока. Не трогает физику/коллайдер: рисует копию спрайта в дочернем
// объекте "Visual" (повторяет sprite/flip исходного SpriteRenderer, так что будущие анимации
// участника 1 продолжат отображаться) и добавляет поверх неё squash/stretch, формы DNA и частицы.
public class PlayerFX : MonoBehaviour
{
    [Serializable]
    public class FormVisual
    {
        public DNAType type;
        public Sprite sprite;
        public float scale = 1f;
        public Vector2 offset;
    }

    [Header("DNA forms (Art/способности)")]
    [SerializeField] private FormVisual[] forms = new FormVisual[0];
    [SerializeField] private bool faceMoveDirection = true;
    [Tooltip("Локальная Y-координата «ног» спрайта — точка, относительно которой сжимается спрайт.")]
    [SerializeField] private float feetLocalY = -4.3f;
    [Tooltip("Ставит спрайт ровно на коллайдер (центр по X, «ноги» — на низ коллайдера).")]
    [SerializeField] private bool alignToCollider = true;

    [Header("Burst effects (prefabs)")]
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem highJumpBurst;
    [SerializeField] private ParticleSystem abilityBurst;
    [SerializeField] private ParticleSystem collectBurst;
    [SerializeField] private ParticleSystem splash;
    [SerializeField] private ParticleSystem fireMuzzle;
    [SerializeField] private ParticleSystem armorBlock;
    [SerializeField] private ParticleSystem deathBurst;
    [SerializeField] private ParticleSystem respawnBurst;

    [Header("Looping effects (children)")]
    [SerializeField] private ParticleSystem highJumpTrail;
    [SerializeField] private ParticleSystem swimBubbles;
    [SerializeField] private ParticleSystem armorParticles;
    [SerializeField] private SpriteRenderer armorGlow;
    [SerializeField] private SpriteRenderer armorRing;
    [SerializeField] private SpriteRenderer abilityGlow;

    [Header("Feel")]
    [SerializeField] private float squashReturnSpeed = 10f;
    [SerializeField] private float swimWobble = 0.07f;

    private SpriteRenderer source;
    private SpriteRenderer visual;
    private Rigidbody2D rb;
    private Collider2D body;
    private PlayerMovement movement;
    private PlayerAbilities abilities;
    private PlayerSwim swim;
    private PlayerFireAttack fire;
    private PlayerRespawn respawn;

    private Vector2 squash = Vector2.one;
    private Color flashColor;
    private float flashAmount;
    private float trailUntil;
    private bool wasGrounded;
    private float lastVelocityY;
    private float facing = 1f;
    private bool playingDeath;
    private float popScale = 1f;
    private float blinkUntil;
    private Vector3 armorGlowScale, armorRingScale, abilityGlowScale;
    private Vector2 baseOffset;

    public SpriteRenderer Visual => visual;

    void Awake()
    {
        source = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        movement = GetComponent<PlayerMovement>();
        abilities = GetComponent<PlayerAbilities>();
        swim = GetComponent<PlayerSwim>();
        fire = GetComponent<PlayerFireAttack>();
        respawn = GetComponent<PlayerRespawn>();

        CreateVisual();
        baseOffset = alignToCollider ? ComputeColliderOffset() : Vector2.zero;

        if (armorGlow != null) armorGlowScale = armorGlow.transform.localScale;
        if (armorRing != null) armorRingScale = armorRing.transform.localScale;
        if (abilityGlow != null) abilityGlowScale = abilityGlow.transform.localScale;
        SetLoop(highJumpTrail, false);
        SetLoop(swimBubbles, false);
        SetArmorAura(false);
        if (abilityGlow != null) abilityGlow.enabled = false;
    }

    private void CreateVisual()
    {
        if (source == null) return;
        var go = new GameObject("Visual");
        go.transform.SetParent(transform, false);
        visual = go.AddComponent<SpriteRenderer>();
        visual.sprite = source.sprite;
        visual.sharedMaterial = source.sharedMaterial;
        visual.sortingLayerID = source.sortingLayerID;
        visual.sortingOrder = source.sortingOrder;
        visual.color = source.color;
        visual.maskInteraction = source.maskInteraction;
        source.enabled = false;
    }

    private Vector2 ComputeColliderOffset()
    {
        if (body is CapsuleCollider2D cap)
            return new Vector2(cap.offset.x, cap.offset.y - cap.size.y / 2f - feetLocalY);
        if (body is BoxCollider2D box)
            return new Vector2(box.offset.x, box.offset.y - box.size.y / 2f - feetLocalY);
        return Vector2.zero;
    }

    void OnEnable()
    {
        if (movement != null) movement.Jumped += OnJumped;
        if (abilities != null)
        {
            abilities.Activated += OnAbilityActivated;
            abilities.Deactivated += OnAbilityDeactivated;
            abilities.Collected += OnCollected;
        }
        if (swim != null)
        {
            swim.SwimmingChanged += OnSwimmingChanged;
            swim.Splashed += OnSplashed;
        }
        if (fire != null) fire.Fired += OnFired;
        if (respawn != null)
        {
            respawn.Died += OnDied;
            respawn.Respawned += OnRespawned;
        }
    }

    void OnDisable()
    {
        if (movement != null) movement.Jumped -= OnJumped;
        if (abilities != null)
        {
            abilities.Activated -= OnAbilityActivated;
            abilities.Deactivated -= OnAbilityDeactivated;
            abilities.Collected -= OnCollected;
        }
        if (swim != null)
        {
            swim.SwimmingChanged -= OnSwimmingChanged;
            swim.Splashed -= OnSplashed;
        }
        if (fire != null) fire.Fired -= OnFired;
        if (respawn != null)
        {
            respawn.Died -= OnDied;
            respawn.Respawned -= OnRespawned;
        }
    }

    // ---------------- Frame update ----------------

    void Update()
    {
        if (playingDeath) return;

        // Приземление: сжатие + пыль.
        bool grounded = movement != null && movement.IsGrounded;
        if (grounded && !wasGrounded && lastVelocityY < -4f)
        {
            float k = Mathf.Clamp01(-lastVelocityY / 18f);
            squash = new Vector2(1f + 0.35f * k, 1f - 0.3f * k);
            Spawn(jumpDust, Feet, null);
        }
        wasGrounded = grounded;
        if (rb != null) lastVelocityY = rb.linearVelocity.y;

        if (highJumpTrail != null && highJumpTrail.isEmitting && (Time.time > trailUntil || (rb != null && rb.linearVelocity.y <= 0f)))
            SetLoop(highJumpTrail, false);

        float h = GameInput.Horizontal;
        if (h > 0.1f) facing = 1f;
        else if (h < -0.1f) facing = -1f;
    }

    void LateUpdate()
    {
        if (visual == null || playingDeath) return;

        DNAType form = abilities != null ? abilities.Active : DNAType.None;
        FormVisual f = FindForm(form);
        float formScale = f != null ? f.scale : 1f;
        Vector2 offset = f != null ? f.offset : Vector2.zero;

        visual.sprite = f != null && f.sprite != null ? f.sprite : source.sprite;
        bool flip = faceMoveDirection && facing < 0f;
        visual.flipX = source.flipX ^ flip;
        if (flip) offset.x = -offset.x;

        squash = Vector2.Lerp(squash, Vector2.one, 1f - Mathf.Exp(-squashReturnSpeed * Time.deltaTime));
        popScale = Mathf.Lerp(popScale, 1f, 1f - Mathf.Exp(-12f * Time.deltaTime));

        Vector2 s = squash * popScale;
        if (swim != null && swim.IsSwimming)
        {
            float w = Mathf.Sin(Time.time * 9f) * swimWobble;
            s = new Vector2(s.x * (1f + w), s.y * (1f - w));
        }

        float sx = formScale * s.x;
        float sy = formScale * s.y;
        visual.transform.localScale = new Vector3(sx, sy, 1f);
        // Масштаб относительно «ног», чтобы спрайт не проваливался в землю.
        visual.transform.localPosition = new Vector3(baseOffset.x + offset.x, baseOffset.y + offset.y + feetLocalY * (formScale - sy), 0f);
        visual.transform.localRotation = Quaternion.identity;

        // Цвет: вспышка + мигание после Respawn.
        flashAmount = Mathf.MoveTowards(flashAmount, 0f, Time.deltaTime * 4f);
        Color c = Color.Lerp(source.color, flashColor, flashAmount);
        if (Time.time < blinkUntil) c.a = (Mathf.Repeat(Time.time * 10f, 1f) < 0.5f) ? 0.35f : 1f;
        visual.color = c;

        AnimateAuras();
    }

    private void AnimateAuras()
    {
        float t = Time.time;
        if (armorGlow != null && armorGlow.enabled)
            armorGlow.transform.localScale = armorGlowScale * (1f + Mathf.Sin(t * 5f) * 0.08f);
        if (armorRing != null && armorRing.enabled)
        {
            armorRing.transform.localScale = armorRingScale * (1f + Mathf.Sin(t * 3f) * 0.05f);
            armorRing.transform.Rotate(0f, 0f, 90f * Time.deltaTime);
            Color rc = armorRing.color;
            rc.a = 0.55f + Mathf.Sin(t * 6f) * 0.25f;
            armorRing.color = rc;
        }
        if (abilityGlow != null && abilityGlow.enabled)
        {
            abilityGlow.transform.localScale = abilityGlowScale * (1f + Mathf.Sin(t * 4f) * 0.1f);
            if (abilities != null && abilities.Active != DNAType.None && abilities.TimeLeft < 3f)
            {
                // Последние 3 секунды — свечение мигает: «время заканчивается».
                Color gc = abilityGlow.color;
                gc.a = Mathf.Repeat(t * 4f, 1f) < 0.5f ? 0.15f : 0.55f;
                abilityGlow.color = gc;
            }
        }
    }

    // ---------------- Events ----------------

    private void OnJumped(bool highJump)
    {
        if (highJump)
        {
            squash = new Vector2(0.6f, 1.5f);
            Flash(DNATypeInfo.GetColor(DNAType.HighJump), 0.8f);
            Spawn(highJumpBurst, Feet, null);
            trailUntil = Time.time + 0.8f;
            SetLoop(highJumpTrail, true);
        }
        else
        {
            squash = new Vector2(0.78f, 1.25f);
        }
        Spawn(jumpDust, Feet, null);
    }

    private void OnAbilityActivated(DNAType type)
    {
        Color c = DNATypeInfo.GetColor(type);
        popScale = 1.3f;
        Flash(c, 1f);
        Spawn(abilityBurst, Center, c);
        if (abilityGlow != null)
        {
            abilityGlow.enabled = true;
            abilityGlow.color = new Color(c.r, c.g, c.b, 0.45f);
        }
        if (type == DNAType.Armor) SetArmorAura(true);
    }

    private void OnAbilityDeactivated(DNAType type)
    {
        if (abilityGlow != null) abilityGlow.enabled = false;
        if (type == DNAType.Armor) SetArmorAura(false);
        if (!playingDeath && (respawn == null || !respawn.IsDead))
        {
            popScale = 0.85f;
            Spawn(abilityBurst, Center, new Color(0.8f, 0.8f, 0.8f, 0.8f));
        }
    }

    private void OnCollected(DNAType type)
    {
        Color c = DNATypeInfo.GetColor(type);
        Flash(c, 0.6f);
        Spawn(collectBurst, Center, c);
    }

    private void OnSwimmingChanged(bool swimming)
    {
        SetLoop(swimBubbles, swimming);
    }

    private void OnSplashed(Vector3 surfacePoint)
    {
        Spawn(splash, surfacePoint, null);
    }

    private void OnFired(Vector3 position, float direction)
    {
        squash = new Vector2(1.2f, 0.85f);
        Flash(DNATypeInfo.GetColor(DNAType.Fire), 0.7f);
        ParticleSystem fx = Spawn(fireMuzzle, position, null);
        if (fx != null && direction < 0f) fx.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
    }

    public void PlayArmorBlock()
    {
        squash = new Vector2(1.25f, 0.8f);
        Flash(Color.white, 0.8f);
        Spawn(armorBlock, Feet, null);
    }

    private void OnDied(string cause)
    {
        StartCoroutine(DeathAnimation(cause));
    }

    private IEnumerator DeathAnimation(string cause)
    {
        playingDeath = true;
        SetLoop(highJumpTrail, false);
        SetLoop(swimBubbles, false);
        SetArmorAura(false);
        if (abilityGlow != null) abilityGlow.enabled = false;

        float duration = respawn != null ? respawn.DeathAnimationTime : 0.8f;
        Transform vt = visual.transform;
        Vector3 baseScale = vt.localScale;
        Color baseColor = source.color;
        Color hurt = cause == "Water" ? new Color(0.4f, 0.7f, 1f) : new Color(1f, 0.3f, 0.3f);

        // 1) Короткий «удар»: раздувание и вспышка.
        float hit = Mathf.Min(0.15f, duration * 0.25f);
        for (float t = 0f; t < hit; t += Time.deltaTime)
        {
            float k = t / hit;
            vt.localScale = baseScale * (1f + 0.3f * k);
            visual.color = Color.Lerp(Color.white, hurt, k);
            yield return null;
        }

        Spawn(deathBurst, Center, hurt);

        // 2) Вращение, сжатие и исчезновение.
        float rest = duration - hit;
        for (float t = 0f; t < rest; t += Time.deltaTime)
        {
            float k = t / rest;
            vt.localScale = baseScale * Mathf.Lerp(1.3f, 0f, k * k);
            vt.localRotation = Quaternion.Euler(0f, 0f, 720f * k);
            Color c = hurt;
            c.a = 1f - k;
            visual.color = c;
            yield return null;
        }

        vt.localScale = Vector3.zero;
        visual.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }

    private void OnRespawned()
    {
        playingDeath = false;
        squash = Vector2.one;
        popScale = 0.01f;
        flashAmount = 0f;
        blinkUntil = Time.time + (respawn != null ? respawn.InvulnerabilityTime : 1f);
        Spawn(respawnBurst, Center, null);
    }

    // ---------------- Helpers ----------------

    private Vector3 Feet => body != null ? new Vector3(body.bounds.center.x, body.bounds.min.y, 0f) : transform.position;
    private Vector3 Center => body != null ? body.bounds.center : transform.position;

    private FormVisual FindForm(DNAType type)
    {
        if (type == DNAType.None || forms == null) return null;
        foreach (var f in forms)
            if (f != null && f.type == type) return f;
        return null;
    }

    private void Flash(Color color, float amount)
    {
        flashColor = color;
        flashAmount = Mathf.Max(flashAmount, amount);
    }

    private ParticleSystem Spawn(ParticleSystem prefab, Vector3 position, Color? color)
    {
        if (prefab == null) return null;
        ParticleSystem fx = Instantiate(prefab, position, prefab.transform.rotation);
        if (color.HasValue)
        {
            var main = fx.main;
            main.startColor = color.Value;
        }
        fx.Play();
        return fx;
    }

    private static void SetLoop(ParticleSystem ps, bool on)
    {
        if (ps == null) return;
        if (on && !ps.isEmitting) ps.Play();
        else if (!on && ps.isEmitting) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void SetArmorAura(bool on)
    {
        if (armorGlow != null) armorGlow.enabled = on;
        if (armorRing != null) armorRing.enabled = on;
        SetLoop(armorParticles, on);
    }
}
