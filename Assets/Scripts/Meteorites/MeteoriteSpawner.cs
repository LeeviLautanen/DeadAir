using UnityEngine;
using System.Collections;

public class MeteoriteSpawner : MonoBehaviour
{
    public GameObject MeteoritePrefab;
    public Vector2 SpawnRange = new(-10, 10);
    public float SpawnAngleRange = 30f;
    public int spawnAmount = 5;
    public float spawnInterval = 5f;
    public float spawnDuration = 2f; // total time (seconds) over which a wave's meteorites will be spawned

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            yield return StartCoroutine(SpawnWave());
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnWave()
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
        Vector3 spawnPosition = new(position, 40, -1);
        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;
        Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);
    }
}
