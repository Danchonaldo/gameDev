using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject levelCompletePanel;

    [Header("Level Flow")]
    [SerializeField] private string levelTitle = "LEVEL 1 - HIGH JUMP";
    [Tooltip("Сцена, которая загрузится после Exit (должна быть в Build Profiles).")]
    [SerializeField] private string nextLevelScene = "Level02";
    [Tooltip("Последний уровень: после Exit — GAME COMPLETE, без перехода дальше.")]
    [SerializeField] private bool isFinalLevel = false;
    [Tooltip("Первый уровень — для «Play again» после GAME COMPLETE.")]
    [SerializeField] private string firstLevelScene = "Level1";
    [SerializeField] private float nextLevelDelay = 2f;

    [Header("Restart")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    public static LevelManager Instance { get; private set; }

    // Статистика за всю игру (переживает загрузку сцен).
    public static int TotalFragments { get; private set; }
    public static int TotalDeaths { get; private set; }
    private static int fragmentsAtLevelStart;
    private static int deathsAtLevelStart;

    private Vector3? checkpoint;
    private bool completed;

    public string LevelTitle => levelTitle;
    public bool IsFinalLevel => isFinalLevel;
    public bool IsCompleted => completed;
    public int FragmentsInLevel { get; private set; }
    public int FragmentsCollected { get; private set; }

    public event System.Action StateChanged;          // фрагменты, чекпоинт
    public event System.Action<Vector3> CheckpointReached;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        fragmentsAtLevelStart = TotalFragments;
        deathsAtLevelStart = TotalDeaths;
        FragmentsInLevel = FindObjectsByType<DNAFragment>(FindObjectsInactive.Exclude).Length;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            if (completed && isFinalLevel) PlayAgain();
            else if (!completed) RestartLevel();
        }
        else if (completed && isFinalLevel && Input.GetKeyDown(KeyCode.Return))
        {
            PlayAgain();
        }
    }

    // ---------- Checkpoint / Respawn ----------

    public Vector3 GetRespawnPoint(Vector3 fallback)
    {
        return checkpoint ?? fallback;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpoint = position;
        CheckpointReached?.Invoke(position);
        StateChanged?.Invoke();
    }

    public void RegisterDeath()
    {
        TotalDeaths++;
    }

    // ---------- DNA Fragments ----------

    public void CollectFragment()
    {
        FragmentsCollected++;
        TotalFragments++;
        StateChanged?.Invoke();
    }

    // ---------- Exit ----------

    public void CompleteLevel()
    {
        if (completed) return;
        completed = true;

        if (isFinalLevel)
        {
            GameHUD hud = FindAnyObjectByType<GameHUD>();
            if (hud != null) hud.ShowGameComplete(TotalFragments, TotalDeaths);
            else if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
            Time.timeScale = 0f;
            return;
        }

        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
        Time.timeScale = 0f;

        if (!string.IsNullOrEmpty(nextLevelScene))
            StartCoroutine(LoadNextLevel());
    }

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(nextLevelDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelScene);
    }

    // ---------- Restart ----------

    public void RestartLevel()
    {
        // Фрагменты, собранные в этой попытке, не засчитываются дважды.
        TotalFragments = fragmentsAtLevelStart;
        TotalDeaths = deathsAtLevelStart;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlayAgain()
    {
        TotalFragments = 0;
        TotalDeaths = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelScene);
    }
}
