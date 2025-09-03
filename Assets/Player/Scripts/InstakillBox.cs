using UnityEngine;

public class InstakillBox : MonoBehaviour
{
    public PlayerRespawn respawn;

    private void Awake()
    {
        respawn = FindAnyObjectByType<PlayerRespawn>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            respawn.TotalRespawn();
            //Debug.Log("Respawn");
        }
        
    }
}
