
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour 
{
    public GameObject DeathCanvas;
    public string menuScene = "MainMenu";

    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // Показываем экран смерти
        DeathCanvas.SetActive(true);

        // Останавливаем игру
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        // Возобновляем время
        Time.timeScale = 1f;

        // Перезапускаем текущий уровень
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        // Возвращаемся в главное меню
        SceneManager.LoadScene(menuScene);
    }
}
