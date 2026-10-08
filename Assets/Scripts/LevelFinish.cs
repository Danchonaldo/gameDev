
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    public GameObject completeCanvas;
    public string levelSelectScene = "LevelSelect";

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed || !other.CompareTag("Player"))
            return;

        completed = true;

        // Разблокируем Level 2
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        PlayerPrefs.SetInt("UnlockedLevel", Mathf.Max(unlocked, 2));
        PlayerPrefs.Save();

        completeCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void FinishLevel()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("UnlockedLevel", 2);
        PlayerPrefs.Save();

        SceneManager.LoadScene("unlockLevel2");
    }

    public void ReplayLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}
