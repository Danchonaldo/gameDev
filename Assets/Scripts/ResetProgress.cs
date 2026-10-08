
using UnityEngine;

public class ResetProgress : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1);
        PlayerPrefs.Save();
    }
}
