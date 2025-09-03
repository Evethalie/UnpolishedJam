using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
   public int maxCorpses;
   public int currentCorpses;
   public Transform playerRespawnPoint;
   public GameObject playerPlatform;
   public Rigidbody2D playerBody;
   public float respawnCost = 2f;
   public event Action onManualRespawn;
   public MMFeedbacks cloneFeedback;
   public MMFeedbacks deathFeedback;
   

   private void Awake()
   {
      playerBody =  GetComponent<Rigidbody2D>();
      if (playerRespawnPoint == null)
      {
         playerRespawnPoint = GameObject.Find("RespawnPoint").transform;
      }
   }

   private void Start()
   {
      if(GameManager.instance.GameOver) return;
      maxCorpses = GameManager.instance.levels.levels[GameManager.instance.currentLevel].maxCorpses;
   }
   
   public void OnManualRespawn(InputAction.CallbackContext ctx)
   {
      if (!ctx.performed) return;   
      DoManualRespawn();
   }

   public void TotalRespawn()
   {
      GameManager.instance.deaths++;
      deathFeedback.PlayFeedbacks();
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
   }

   public void DoManualRespawn()
   {
      StartCoroutine(RespawnRoutine());
      
   }

   private IEnumerator RespawnRoutine()
   {
      if(currentCorpses >= maxCorpses) yield break;
      cloneFeedback?.PlayFeedbacks();
      Instantiate(playerPlatform, transform.position, transform.rotation);
      currentCorpses++;
      if (UIManager.instance != null) UIManager.instance.levelTimer -= respawnCost;
      playerBody.simulated = false;
      transform.position = playerRespawnPoint.position;   
      playerBody.linearVelocity = Vector2.zero;
      playerBody.angularVelocity = 0f;

      yield return new WaitForFixedUpdate();           
      
      playerBody.simulated = true;
      onManualRespawn?.Invoke();
   }
   
}
