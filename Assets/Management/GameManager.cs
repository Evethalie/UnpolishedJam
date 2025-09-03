using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public LevelContainer levels;
    public int currentLevel = 0;
    public float levelTimer;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        currentLevel++;
        SceneManager.LoadScene(levels.levels[1].level);
    }

    public void NextLevel()
    {
        currentLevel++;
        SceneManager.LoadScene(currentLevel);
    }
}

[System.Serializable]
public struct Level
{
    public int level;
    public int maxCorpses;
    [FormerlySerializedAs("maxTime")] public float levelTimeLimit;
}