using UnityEngine;

[CreateAssetMenu(menuName = "Game/MeteoriteWaveData")]
public class MeteoriteWaveData : ScriptableObject
{
    [Header("Amount and duration of spawned wave")]
    public int Amount = 10;
    public float DurationSeconds = 2f;
    public float SpawnsPerSecond = 0f;

    [Header("Spawn time")]
    public int Day = 1;
    public int Hour = 0;

    private void OnValidate()
    {
        if (SpawnsPerSecond > 0f)
        {
            Amount = Mathf.CeilToInt(DurationSeconds * SpawnsPerSecond);
        }
    }
}
