using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
        
    public void PlayGame()
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (unlocked >= 3)
        {
            SceneManager.LoadScene("unlockLevel3");
        }
        else if (unlocked >= 2)
        {
            SceneManager.LoadScene("unlockLevel2");
        }
        else
        {
            SceneManager.LoadScene("unlockLevel1");
        }
    }


    public void OpenLevelGallery()
    {
        SceneManager.LoadScene("LevelGallery");
    }

    public void HowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }





    public void BackToLevelGallery()
    {
    SceneManager.LoadScene("LevelGallery");
    }

    public void OpenLevel1Info()
    {
    SceneManager.LoadScene("уровень 1");
    }
    public void OpenLevel2Info()
    {
    SceneManager.LoadScene("уровень 2");
    }
    public void OpenLevel3Info()
    {
    SceneManager.LoadScene("уровень 3");
    }





    public void UnlockLevel1()
    {
        SceneManager.LoadScene("Level1");
    }
    public void UnlockLevel2()
    {
        SceneManager.LoadScene("Level2");
    }
    public void UnlockLevel3()
    {
        SceneManager.LoadScene("Level3");
    }


}