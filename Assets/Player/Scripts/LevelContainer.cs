using UnityEngine;

[CreateAssetMenu(fileName = "LevelContainer", menuName = "Scriptable Objects/LevelContainer")]
public class LevelContainer : ScriptableObject
{
    public Level[] levels;
}
