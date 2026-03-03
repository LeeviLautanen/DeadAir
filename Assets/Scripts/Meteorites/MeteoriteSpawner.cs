using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeteoriteSpawner : MonoBehaviour
{
    public GameObject MeteoritePrefab;
    public Vector2 SpawnRange = new(-10, 10);
    public float SpawnHeight = 40f;
    public float SpawnAngleRange = 20f;
    private List<MeteoriteWaveData> attackWaves = new();

    private TimeManager timeManager;

    private async void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();

        attackWaves = await Utility.LoadAllByLabel<MeteoriteWaveData>("AttackWaveSO");
        foreach (MeteoriteWaveData wave in attackWaves)
        {
            timeManager.ScheduleEvent(() => StartCoroutine(SpawnWave(wave.Amount, wave.Duration)), wave.Day, wave.Hour);
        }
    }

    private IEnumerator SpawnWave(int spawnAmount, float spawnDuration)
    {
        if (spawnAmount <= 0)
            yield break;

        // Generate random proportions and normalize to spawnDuration so intervals sum to spawnDuration
        float[] props = new float[spawnAmount];
        float sum = 0f;
        for (int i = 0; i < spawnAmount; i++)
        {
            props[i] = Random.value;
            sum += props[i];
        }

        float[] intervals = new float[spawnAmount];
        if (sum > 0f)
        {
            for (int i = 0; i < spawnAmount; i++)
                intervals[i] = props[i] / sum * spawnDuration;
        }
        else
        {
            // fallback: even spacing
            float even = spawnDuration / spawnAmount;
            for (int i = 0; i < spawnAmount; i++)
                intervals[i] = even;
        }

        for (int i = 0; i < spawnAmount; i++)
        {
            float spawnPos = Random.Range(SpawnRange.x, SpawnRange.y);
            float angle = Random.Range(-SpawnAngleRange, SpawnAngleRange);
            SpawnMeteorite(spawnPos, angle);

            // Wait the interval before spawning the next one (no wait after last spawn)
            if (i < spawnAmount - 1)
                yield return new WaitForSeconds(intervals[i]);
        }
    }

    private void SpawnMeteorite(float position, float angle)
    {
        Vector3 spawnPosition = new(position, SpawnHeight, -1);
        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;
        Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);
    }
}
