using UnityEngine;

public class GoToNextLevel : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D collision)
  {
    GameManager.instance.NextLevel();
  }
}
