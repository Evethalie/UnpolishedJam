using System;
using System.Collections;
using System.Collections.Generic;
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
    public int   deathPenalty   = 50;
    [SerializeField] private int  speedrunTimeBonusPoints = 50_000; // NEW: overall weight of time bonus
    [SerializeField] private bool clampNonNegativeScore   = true;
    
   // public float secondPenaltyValue = 5f;

    
    private int finalScore;
    public  TMP_Text finalScoreText;
    
    private float runStartTime;
    private bool runActive = false;
    private HashSet<string> collectedCoinKeys = new HashSet<string>();


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
        collectedCoinKeys.Clear();
    }


    public void StartGame()
    {
        if (levels.levels == null || levels.levels.Length < 2)
        {
            EndGame();
            return;
        }

        ResetRunStats();     
        GameOver = false;

        currentLevel = 1;                           
        runStartTime = Time.realtimeSinceStartup;   
        runActive    = true;

        SceneManager.LoadScene(levels.levels[currentLevel].level);
    }

    public void EndGame()
    {
        if (!runActive)
        {
            runStartTime = Time.realtimeSinceStartup;
            runActive = true;
        }
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
        
        else
        {
           
            if (!runActive && levels.levels != null && levels.levels.Length > 1)
            {
                int firstPlayableBuildIndex = levels.levels[1].level; 
                if (scene.buildIndex == firstPlayableBuildIndex)
                {
                    runStartTime = Time.realtimeSinceStartup;
                    runActive = true;
                }
            }
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
     
        int coinScore = coinsCollected * coinPointValue; 
        
        float totalParTime = 0f;
        if (levels?.levels != null && levels.levels.Length > 0) 
        {
            for (int i = 0; i < levels.levels.Length; i++) 
            {
                if (i == 0) continue; 
                totalParTime += Mathf.Max(0f, levels.levels[i].levelTimeLimit); 
            }
        }
        
        float timeBonus = 0f; 
        if (totalTimeTaken > 0f && totalParTime > 0f)
        {
            float ratio = totalParTime / totalTimeTaken; 
            timeBonus = speedrunTimeBonusPoints * Mathf.Max(0f, ratio);
        }

      
        int deathScore = deaths * deathPenalty; 

       
        float raw = coinScore + timeBonus - deathScore; 

       
        finalScore = Mathf.RoundToInt(clampNonNegativeScore ? Mathf.Max(0f, raw) : raw); // NEW

        
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
        // if (levels.levels[currentLevel].coins > 0 &&
        //     GameObject.FindObjectsByType<Coin>(FindObjectsSortMode.None).Length <= 0)
        // {
        //     coinsCollected += levels.levels[currentLevel].coins;
        // }
        currentLevel++;
        if (currentLevel >= levels.levels.Length)
        {
            EndGame();
            return;
        }

        
        SceneManager.LoadScene(levels.levels[currentLevel].level);
    }
    
    public bool HasCollected(string coinKey) => collectedCoinKeys.Contains(coinKey);

    public bool MarkCollected(string coinKey)
    {
        bool added = collectedCoinKeys.Add(coinKey);
        if (added) coinsCollected = collectedCoinKeys.Count;
        return added;
    }

    public void ResetCoinsForCurrentScene()
    {
        string scenePrefix = SceneManager.GetActiveScene().name + "/";
        collectedCoinKeys.RemoveWhere(k => k.StartsWith(scenePrefix, StringComparison.Ordinal));
        coinsCollected = collectedCoinKeys.Count;
    }
}

[System.Serializable]
public struct Level
{
    public int level;
    public int maxCorpses; 
    public float levelTimeLimit;
    public int coins;
    public bool coinCollected;
}