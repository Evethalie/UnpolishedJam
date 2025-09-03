using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public LevelContainer levels;
    public int currentLevel = 0;
    public string endSceneName = "End";
    public bool GameOver;
    
    public int coinsCollected;
    public float totalTimeTaken;
    public int deaths;
    
    public TMP_Text coinsCollectedText;
    public TMP_Text totalTimeTakenText;
    public TMP_Text deathsText;
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
        if (currentLevel >= levels.levels.Length)
        {
            EndGame();
            return;
        }
        GameOver = false;
        currentLevel++;
        SceneManager.LoadScene(levels.levels[1].level);
    }

    public void EndGame()
    {
        currentLevel = -1;
        GameOver = true;
        SceneManager.LoadScene(endSceneName);
    }

    public void GoToMainMenu()
    {
        GameOver = false;
        currentLevel = 0;
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            totalTimeTaken += levels.levels[currentLevel].levelTimeLimit - UIManager.instance.levelTimer;
        }
      
        currentLevel++;
        SceneManager.LoadScene(currentLevel);
    }
}

[System.Serializable]
public struct Level
{
    public int level;
    public int maxCorpses; 
    public float levelTimeLimit;
}