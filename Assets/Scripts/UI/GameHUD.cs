using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD: таймер способности, счётчик DNA Fragments, сообщения, экран GAME COMPLETE.
public class GameHUD : MonoBehaviour
{
    [Header("Ability")]
    [SerializeField] private GameObject abilityPanel;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilityStatus;
    [SerializeField] private RectTransform abilityBarFill;
    [SerializeField] private Image abilityBarImage;
    [SerializeField] private TMP_Text abilityList;
    [SerializeField] private Sprite highJumpIcon;
    [SerializeField] private Sprite swimIcon;
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite armorIcon;

    [Header("DNA Fragments")]
    [SerializeField] private GameObject fragmentPanel;
    [SerializeField] private TMP_Text fragmentText;

    [Header("Messages")]
    [SerializeField] private TMP_Text levelTitle;
    [SerializeField] private TMP_Text toast;
    [SerializeField] private float titleTime = 2.5f;
    [SerializeField] private float toastTime = 2.2f;

    [Header("Game Complete")]
    [SerializeField] private GameObject gameCompletePanel;
    [SerializeField] private TMP_Text gameCompleteStats;

    private PlayerAbilities abilities;
    private LevelManager levelManager;
    private float titleLeft;
    private float toastLeft;

    void Start()
    {
        abilities = FindAnyObjectByType<PlayerAbilities>();
        levelManager = LevelManager.Instance != null ? LevelManager.Instance : FindAnyObjectByType<LevelManager>();

        if (abilities != null)
        {
            abilities.Collected += OnCollected;
            abilities.Expired += OnExpired;
            abilities.Activated += OnActivated;
            abilities.ActivationFailed += OnActivationFailed;
        }
        if (levelManager != null)
        {
            levelManager.StateChanged += RefreshFragments;
            levelManager.CheckpointReached += OnCheckpoint;
            if (levelTitle != null) levelTitle.text = levelManager.LevelTitle;
        }

        titleLeft = titleTime;
        if (toast != null) toast.alpha = 0f;
        if (gameCompletePanel != null) gameCompletePanel.SetActive(false);
        RefreshFragments();
    }

    void OnDestroy()
    {
        if (abilities != null)
        {
            abilities.Collected -= OnCollected;
            abilities.Expired -= OnExpired;
            abilities.Activated -= OnActivated;
            abilities.ActivationFailed -= OnActivationFailed;
        }
        if (levelManager != null)
        {
            levelManager.StateChanged -= RefreshFragments;
            levelManager.CheckpointReached -= OnCheckpoint;
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (levelTitle != null)
        {
            titleLeft -= dt;
            levelTitle.alpha = Mathf.Clamp01(titleLeft / 0.6f);
        }
        if (toast != null)
        {
            toastLeft -= dt;
            toast.alpha = Mathf.Clamp01(toastLeft / 0.4f);
        }

        UpdateAbility();
    }

    // ---------------- Ability ----------------

    private void UpdateAbility()
    {
        if (abilityPanel == null) return;
        bool show = abilities != null && abilities.Owned.Count > 0;
        if (abilityPanel.activeSelf != show) abilityPanel.SetActive(show);
        if (!show) return;

        DNAType shown = abilities.Active != DNAType.None ? abilities.Active : abilities.Selected;
        Color color = DNATypeInfo.GetColor(shown);
        float fill;
        string status;

        switch (abilities.GetState(shown))
        {
            case PlayerAbilities.AbilityState.Active:
                fill = abilities.TimeLeft / Mathf.Max(0.01f, abilities.Duration);
                status = $"ACTIVE  {abilities.TimeLeft:0.0}s";
                if (shown == DNAType.Fire) status += "   [F] shoot";
                break;
            case PlayerAbilities.AbilityState.Cooldown:
                float cd = abilities.GetCooldownLeft(shown);
                fill = 1f - cd / Mathf.Max(0.01f, abilities.CooldownDuration);
                status = $"COOLDOWN  {cd:0.0}s";
                color = Color.Lerp(color, Color.gray, 0.6f);
                break;
            default:
                fill = 1f;
                status = "READY  -  press [E]";
                float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 6f);
                color *= new Color(pulse, pulse, pulse, 1f);
                break;
        }

        if (abilityName != null)
        {
            abilityName.text = DNATypeInfo.DisplayName(shown);
            abilityName.color = DNATypeInfo.GetColor(shown);
        }
        if (abilityStatus != null) abilityStatus.text = status;
        if (abilityBarFill != null) abilityBarFill.anchorMax = new Vector2(Mathf.Clamp01(fill), 1f);
        if (abilityBarImage != null) abilityBarImage.color = color;
        if (abilityIcon != null)
        {
            abilityIcon.sprite = GetIcon(shown);
            abilityIcon.enabled = abilityIcon.sprite != null;
        }

        if (abilityList != null)
        {
            if (abilities.Owned.Count > 1)
            {
                var sb = new System.Text.StringBuilder("[Q] switch:  ");
                foreach (var t in abilities.Owned)
                {
                    string hex = ColorUtility.ToHtmlStringRGB(DNATypeInfo.GetColor(t));
                    string name = DNATypeInfo.DisplayName(t);
                    sb.Append(t == abilities.Selected ? $"<color=#{hex}><b>[{name}]</b></color>  " : $"<color=#{hex}>{name}</color>  ");
                }
                abilityList.text = sb.ToString();
            }
            else
            {
                abilityList.text = "";
            }
        }
    }

    private Sprite GetIcon(DNAType type)
    {
        switch (type)
        {
            case DNAType.HighJump: return highJumpIcon;
            case DNAType.Swim: return swimIcon;
            case DNAType.Fire: return fireIcon;
            case DNAType.Armor: return armorIcon;
            default: return null;
        }
    }

    private void OnCollected(DNAType type)
    {
        ShowToast($"{DNATypeInfo.DisplayName(type)} DNA!   Press [E] to use", DNATypeInfo.GetColor(type));
    }

    private void OnActivated(DNAType type)
    {
        string hint = type == DNAType.Swim ? "Swim: [Space]/[W] up, [S] down"
                    : type == DNAType.Fire ? "Fire: press [F] to shoot"
                    : type == DNAType.Armor ? "Armor: spikes can't hurt you"
                    : "High Jump: jump higher!";
        ShowToast(hint, DNATypeInfo.GetColor(type));
    }

    private void OnExpired(DNAType type)
    {
        ShowToast($"{DNATypeInfo.DisplayName(type)}: TIME OVER", new Color(1f, 0.6f, 0.4f));
    }

    private void OnActivationFailed(DNAType type)
    {
        if (abilities != null && abilities.GetState(type) == PlayerAbilities.AbilityState.Cooldown)
            ShowToast($"{DNATypeInfo.DisplayName(type)} is recharging...", Color.gray);
    }

    // ---------------- Fragments / checkpoint ----------------

    private void RefreshFragments()
    {
        if (levelManager == null || fragmentPanel == null) return;
        bool show = levelManager.FragmentsInLevel > 0;
        fragmentPanel.SetActive(show);
        if (show && fragmentText != null)
            fragmentText.text = $"DNA  {levelManager.FragmentsCollected}/{levelManager.FragmentsInLevel}";
    }

    private void OnCheckpoint(Vector3 position)
    {
        ShowToast("CHECKPOINT!", new Color(0.4f, 1f, 0.5f));
    }

    public void ShowToast(string message, Color color)
    {
        if (toast == null) return;
        toast.text = message;
        toast.color = color;
        toastLeft = toastTime;
        toast.alpha = 1f;
    }

    // ---------------- Game complete ----------------

    public void ShowGameComplete(int fragments, int deaths)
    {
        if (gameCompletePanel != null) gameCompletePanel.SetActive(true);
        if (gameCompleteStats != null)
            gameCompleteStats.text = $"DNA Fragments collected: {fragments}\nDeaths: {deaths}\n\nPress [R] or [Enter] to play again";
    }
}
