using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text corpseText;
    public TMP_Text levelText;
    public float levelTimer;
    public static UIManager instance;
    public bool respawnTriggered;
    public PlayerRespawn playerRespawn;
    //public MMSMPlaylistManager playlistManager;
   // public GameObject PauseMenu;
   // public GameObject musicOffSprite;
   // public GameObject sfxOffSprite;
   public LevelContainer levels;
   public int level;

    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
       // if(SceneManager.GetActiveScene().buildIndex == 0) return;
       if(GameManager.instance.GameOver) return;
        levelTimer = GameManager.instance.levels.levels[GameManager.instance.currentLevel].levelTimeLimit;
    }
    void Update()
    {
        
      //  if(SceneManager.GetActiveScene().buildIndex == 0) return;
      if(GameManager.instance.GameOver) return;
      if(SceneManager.GetActiveScene().buildIndex == 16 || SceneManager.GetActiveScene().buildIndex == 0) return;
        levelTimer -= Time.deltaTime;
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
      
         if (levelTimer <= levels.levels[level].levelTimeLimit * 0.5)
         {
             timerText.color = Color.red;
         }
         else
         {
             timerText.color = Color.white;
         }
        if (levelTimer <= 0f)
        {
            levelTimer = 0f;
            DisplayTime(levelTimer);      
            if (!respawnTriggered)      
            {
                respawnTriggered = true;
                playerRespawn.TotalRespawn();
            }
            return;
        }

        corpseText.text =
            $"{playerRespawn.currentCorpses}/{levels.levels[level].maxCorpses} Corpses";
        // Regular update
        DisplayTime(levelTimer);
        UpdateHearts();

       
    }
    
    void DisplayTime(float timeToDisplay)
    {
       float minutes = Mathf.FloorToInt(timeToDisplay / 60);
       float seconds = Mathf.FloorToInt(timeToDisplay % 60);
       int milliseconds = Mathf.FloorToInt((timeToDisplay * 1000f) % 1000f);

       
       timerText.text = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
    
    void UpdateHearts()
    {
      
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        //PauseMenu.SetActive(true);
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
       // PauseMenu.SetActive(false);
    }

    public void StartGame()
    {
        GameManager.instance.StartGame();
    }

    public void MuteOrUnmuteMusic()
    {
        if (MMSoundManager.Instance.IsMuted(MMSoundManager.MMSoundManagerTracks.Music))
        {
            MMSoundManager.Instance.UnmuteMusic();
           
        }
        else
        {
            MMSoundManager.Instance.MuteMusic();
           
        }
    }

    public void MuteOrUnmuteSFX()
    {
        if (MMSoundManager.Instance.IsMuted(MMSoundManager.MMSoundManagerTracks.Sfx))
        {
            MMSoundManager.Instance.UnmuteSfx();
            
        }
        else
        {
            MMSoundManager.Instance.MuteSfx();
        }
    }
    
   
}
