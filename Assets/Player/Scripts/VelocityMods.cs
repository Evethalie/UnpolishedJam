using UnityEngine;

public class VelocityMods : MonoBehaviour
{
    
    public float addX;              
    public float addY;             
    public float maxXOverride = float.NaN; 

    
    public float impulseX;
    public float impulseY;
    public bool  setYOnce;           
    public float setY;

   
    public void AddConveyor(float x) => addX += x;
    public void RemoveConveyor(float x) => addX -= x;

    public void AddWind(float x) => addX += x;
    public void RemoveWind(float x) => addX -= x;

    public void Impulse(float x, float y) { impulseX += x; impulseY += y; }
    public void SetY(float y) { setYOnce = true; setY = y; }

    public void ClearOneFrame()
    {
        impulseX = 0f;
        impulseY = 0f;
        setYOnce = false;
    }
}
