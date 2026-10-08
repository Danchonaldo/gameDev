using System;
using System.Collections.Generic;
using UnityEngine;

// DNA-способности с ограничением по времени.
// Подобрал DNA → способность READY → [E] → ACTIVE (abilityDuration) → TIME OVER → COOLDOWN → снова READY.
// Одновременно активна одна способность (блоб принимает одну форму). [Q] — сменить выбранную DNA.
public class PlayerAbilities : MonoBehaviour
{
    public enum AbilityState { Locked, Ready, Active, Cooldown }

    [Header("Timer")]
    [Tooltip("Сколько секунд работает способность после активации.")]
    [SerializeField] private float abilityDuration = 10f;
    [Tooltip("Перезарядка после окончания способности.")]
    [SerializeField] private float cooldown = 3f;

    [Header("Input")]
    [SerializeField] private KeyCode activateKey = KeyCode.E;
    [SerializeField] private KeyCode switchKey = KeyCode.Q;

    private readonly List<DNAType> owned = new List<DNAType>();
    private readonly Dictionary<DNAType, float> cooldownLeft = new Dictionary<DNAType, float>();
    private DNAType selected = DNAType.None;
    private DNAType active = DNAType.None;
    private float timeLeft;

    public event Action<DNAType> Collected;
    public event Action<DNAType> Activated;
    public event Action<DNAType> Expired;      // время вышло
    public event Action<DNAType> Deactivated;  // любое выключение (время, смена, смерть)
    public event Action<DNAType> ActivationFailed;

    public IReadOnlyList<DNAType> Owned => owned;
    public DNAType Selected => selected;
    public DNAType Active => active;
    public float TimeLeft => timeLeft;
    public float Duration => abilityDuration;
    public float CooldownDuration => cooldown;
    public bool InputEnabled { get; set; } = true;

    public bool IsActive(DNAType type) => type != DNAType.None && active == type;
    public bool Has(DNAType type) => owned.Contains(type);

    public float GetCooldownLeft(DNAType type)
    {
        return cooldownLeft.TryGetValue(type, out float t) ? t : 0f;
    }

    public AbilityState GetState(DNAType type)
    {
        if (!Has(type)) return AbilityState.Locked;
        if (active == type) return AbilityState.Active;
        if (GetCooldownLeft(type) > 0f) return AbilityState.Cooldown;
        return AbilityState.Ready;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (InputEnabled)
        {
            if (GameInput.GetKeyDown(switchKey)) CycleSelected();
            if (GameInput.GetKeyDown(activateKey)) TryActivate(selected);
        }

        if (active != DNAType.None)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                DNAType expired = active;
                EndActive(true);
                Expired?.Invoke(expired);
            }
        }

        if (cooldownLeft.Count > 0)
        {
            var keys = new List<DNAType>(cooldownLeft.Keys);
            foreach (var key in keys)
            {
                float t = cooldownLeft[key] - Time.deltaTime;
                if (t <= 0f) cooldownLeft.Remove(key);
                else cooldownLeft[key] = t;
            }
        }
    }

    // Получить DNA (подбор). Способность становится доступной и выбранной.
    public void Collect(DNAType type)
    {
        if (type == DNAType.None) return;
        if (!owned.Contains(type)) owned.Add(type);
        cooldownLeft.Remove(type);
        selected = type;
        Collected?.Invoke(type);
    }

    public bool TryActivate(DNAType type)
    {
        if (GetState(type) != AbilityState.Ready)
        {
            if (type != DNAType.None && active != type) ActivationFailed?.Invoke(type);
            return false;
        }

        if (active != DNAType.None) EndActive(true);

        selected = type;
        active = type;
        timeLeft = abilityDuration;
        Activated?.Invoke(type);
        return true;
    }

    public void Select(DNAType type)
    {
        if (Has(type)) selected = type;
    }

    public void CycleSelected()
    {
        if (owned.Count == 0) return;
        int index = owned.IndexOf(selected);
        selected = owned[(index + 1) % owned.Count];
    }

    // Выключить текущую способность (по умолчанию — с перезарядкой).
    public void EndActive(bool startCooldown)
    {
        if (active == DNAType.None) return;
        DNAType ended = active;
        active = DNAType.None;
        timeLeft = 0f;
        if (startCooldown && cooldown > 0f) cooldownLeft[ended] = cooldown;
        Deactivated?.Invoke(ended);
    }

    // После смерти: способность выключается, перезарядки сбрасываются (DNA остаются).
    public void ResetAfterDeath()
    {
        EndActive(false);
        cooldownLeft.Clear();
    }
}
