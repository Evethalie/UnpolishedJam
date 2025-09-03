using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float bounceY = 16f;
    public float rearmAfterSeconds = 0.05f; // tiny cooldown

    void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponent<PlayerMovement>();
        if (pm == null) return;

        pm.ApplyBounce(bounceY);
    }
}
