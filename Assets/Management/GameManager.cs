using System.Collections;
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
    
    public int   coinPointValue = 100;  
    public float secondBonusValue = 10f; 
    public int   deathPenalty   = 50;
    
    public float secondPenaltyValue = 5f;

    
    private int finalScore;
    public  TMP_Text finalScoreText;
    
    private float runStartTime;



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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void ResetRunStats()
    {
        coinsCollected   = 0;
        deaths           = 0;
        totalTimeTaken   = 0f;
        finalScore       = 0;
    }


    public void StartGame()
    {
        if (currentLevel >= levels.levels.Length)
        {
            EndGame();
            return;
        }
        ResetRunStats();     

        GameOver = false;
        currentLevel++;
        runStartTime = Time.realtimeSinceStartup;
        SceneManager.LoadScene(levels.levels[1].level);
    }

    public void EndGame()
    {
        totalTimeTaken = Time.realtimeSinceStartup - runStartTime;
        currentLevel = -1;
        GameOver = true;
        SceneManager.LoadScene(endSceneName);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == endSceneName)
        {
            FindEndSceneTexts();
            StartDisplayingStats();
        }
    }
    
    private void FindEndSceneTexts()
    {
        coinsCollectedText = GameObject.Find("Coins")?.GetComponent<TMP_Text>();
        totalTimeTakenText = GameObject.Find("TotalTimeTaken")?.GetComponent<TMP_Text>();
        deathsText = GameObject.Find("Deaths")?.GetComponent<TMP_Text>();
    }
    
    private void StartDisplayingStats()
    {
        if (coinsCollectedText)
            StartCoroutine(CountUpText(coinsCollectedText, 0, coinsCollected, 2f, "Coins Collected: "));

        if (totalTimeTakenText)
            StartCoroutine(CountUpTimeText(totalTimeTakenText, 0f, totalTimeTaken, 2f, "Total Time Taken: "));

        if (deathsText)
            StartCoroutine(CountUpText(deathsText, 0, deaths, 2f, "Deaths: "));
    }
    
    private static string FormatSpeedrunTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int millis = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 1000f);
        return string.Format("{0:00}:{1:00}.{2:000}", minutes, secs, millis);
    }

    private IEnumerator CountUpTimeText(TMP_Text textComponent, float startValue, float endValue, float duration, string prefix)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float current = Mathf.Lerp(startValue, endValue, progress);
            textComponent.text = prefix + FormatSpeedrunTime(current);
            yield return null;
        }

        textComponent.text = prefix + FormatSpeedrunTime(endValue);
        CalculateAndDisplayFinalScore();
    }
    
    private IEnumerator CountUpText(TMP_Text textComponent, int startValue, int endValue, float duration, string supplementalText)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, progress));
            textComponent.text = supplementalText + currentValue;
            yield return null;
        }
        textComponent.text = supplementalText + endValue;
        CalculateAndDisplayFinalScore();
    }
    
    
    private void CalculateAndDisplayFinalScore()
    {
     
        int   coinScore      = coinsCollected * coinPointValue;
        float timePenalty    = totalTimeTaken * secondPenaltyValue;
        int   deathScore     = deaths * deathPenalty;   // already negative

       
        float rawScore = coinScore - timePenalty - deathScore;

     
        finalScore = Mathf.Max(0, Mathf.RoundToInt(rawScore));

        
        if (finalScoreText == null)
            finalScoreText = GameObject.Find("FinalScore")?.GetComponent<TMP_Text>();

        if (finalScoreText)
            finalScoreText.text = $"Final Score: {finalScore:N0}";

    }


    public void GoToMainMenu()
    {
        ResetRunStats();      

        GameOver = false;
        currentLevel = 0;
        SceneManager.LoadScene(0);
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
    public float levelTimeLimit;
    public int coins;
}