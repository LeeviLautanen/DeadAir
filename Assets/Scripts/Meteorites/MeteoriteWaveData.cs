using UnityEngine;

[CreateAssetMenu(menuName = "Game/MeteoriteWaveData")]
public class MeteoriteWaveData : ScriptableObject
{
    [Header("Amount and duration of spawned wave")]
    public int Amount = 10;
    public float Duration = 2f;

    [Header("Spawn time")]
    public int Day = 1;
    public int Hour = 0;
}
