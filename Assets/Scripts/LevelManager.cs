using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject levelCompletePanel;

    public void CompleteLevel()
    {
        levelCompletePanel.SetActive(true);
        Time.timeScale = 0f;
    }
}