
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    public GameObject completeCanvas;

    
    public int currentLevel = 1;

    
    public string nextScene = "unlockLevel2";

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed || !other.CompareTag("Player"))
            return;

        completed = true;

        // Сохраняем открытие следующего уровня
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        PlayerPrefs.SetInt(
            "UnlockedLevel",
            Mathf.Max(unlocked, Mathf.Min(currentLevel + 1, 3))
        );

        PlayerPrefs.Save();

        if (completeCanvas != null)
            completeCanvas.SetActive(true);

        Time.timeScale = 0f;
    }

    public void FinishLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(nextScene);
    }

    public void ReplayLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
