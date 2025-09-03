using UnityEngine;

public class WindZone : MonoBehaviour
{
    public float windX = 2f;

    void OnTriggerEnter2D(Collider2D other)
    {
        var mods = other.GetComponent<VelocityMods>();
        if (mods) mods.addX += windX;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var mods = other.GetComponent<VelocityMods>();
        if (mods) mods.addX -= windX;
    }
}
