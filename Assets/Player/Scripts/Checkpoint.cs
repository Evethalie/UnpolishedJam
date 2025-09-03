using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer flagSprite;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerRespawn playerRespawn))
        {
            playerRespawn.playerRespawnPoint = this.transform;
            flagSprite.color = Color.green;
        }
    }
}
