using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeteoriteWaveManager : MonoBehaviour
{
    public System.Action<(int, int, int)?> OnNextWaveInfoUpdated;
    public GameObject MeteoritePrefab;
    public Vector2 SpawnRange = new(-10, 10);
    public float SpawnHeight = 40f;
    public float SpawnAngleRange = 20f;
    public Vector2 RotationSpeedRange = new(30f, 180f);
    public Vector2 SpeedRandomizationRange = new(0.8f, 1.1f);

    private static readonly Logger log = new(nameof(MeteoriteWaveManager));
    private TimeManager timeManager;
    private TechManager techManager;
    private List<MeteoriteWaveData> attackWaves = new();
    private Dictionary<Building, bool> interceptors = new();
    private int interceptorCount;
    [SerializeField] private float upgradeReductionMult = 1f;
    [SerializeField] private float totalReductionMult = 1f;
    [SerializeField] private float impactSpotSize = 1f;
    [SerializeField] private float secondHitChance = 0.3f;
    [SerializeField] private float thirdHitChance = 0.1f;
    [SerializeField] private float fourthOrMoreHitChance = 0.01f;
    private int nextWaveIndex = 0;
    private readonly List<int>[] impactBuckets = { new(), new(), new(), new() };
    private int[] impactSpotHitCounts = System.Array.Empty<int>();
    private int waveSpotCount;
    private float meteoriteImpactSize;
    private float waveSecondHitChance;
    private float waveThirdHitChance;
    private float waveFourthOrMoreHitChance;

    private async void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        techManager = FindFirstObjectByType<TechManager>();

        attackWaves = await Utility.LoadAllByLabel<MeteoriteWaveData>("AttackWaveSO");
        attackWaves = attackWaves
            .OrderBy(x => x.Day)
            .ThenBy(x => x.Hour)
            .ToList();
        foreach (MeteoriteWaveData wave in attackWaves)
        {
            timeManager.ScheduleEvent(() => HandleWaveSpawn(wave), wave.Day, wave.Hour);
        }

        TechManager.OnResearchCompleted += HandleResearchCompleted;
        OnNextWaveInfoUpdated?.Invoke(GetNextWaveData());
    }

    public (int, int, int)? GetNextWaveData()
    {
        if (nextWaveIndex >= attackWaves.Count)
        {
            return null;
        }

        MeteoriteWaveData nextWave = attackWaves[nextWaveIndex];
        return (GetSpawnAmountWithMult(nextWave), nextWave.Day, nextWave.Hour);
    }

    public void SetInterceptorState(Building interceptor, bool isOperational)
    {
        interceptors[interceptor] = isOperational;
        UpdateMeteoriteAmountMult();
    }

    public void HandleWaveSpawn(MeteoriteWaveData wave)
    {
        int spawnAmount = GetSpawnAmountWithMult(wave);
        log.Info($"Spawning wave with base amount {wave.Amount}, total multiplier {totalReductionMult}, final spawn amount {spawnAmount}");
        StartCoroutine(SpawnWave(spawnAmount, wave.Duration));
    }

    private IEnumerator SpawnWave(int spawnAmount, float spawnDuration)
    {
        if (spawnAmount <= 0)
            yield break;

        ResetImpactSelectionForWave();

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
            float impactPos = GetWeightedImpactX();
            float angle = Random.Range(-SpawnAngleRange, SpawnAngleRange);
            SpawnMeteorite(impactPos, angle);

            // Wait the interval before spawning the next one (no wait after last spawn)
            if (i < spawnAmount - 1)
            {
                // Wait until unpaused
                while (timeManager.IsPaused)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(intervals[i]);
            }
        }

        nextWaveIndex++;
        OnNextWaveInfoUpdated?.Invoke(GetNextWaveData());
    }

    private void UpdateMeteoriteAmountMult()
    {
        // Prevent two calls from the same lab counting as two
        interceptorCount = interceptors.Values.Count(v => v);

        if (interceptorCount <= 0)
        {
            totalReductionMult = 1f;
            return;
        }

        float newReduction = 1f; // Start from 1, no multiplier
        for (int i = 1; i < interceptorCount + 1; i++)
        {
            newReduction -= 0.1f; // Flat reduction per interceptor
        }

        newReduction = Mathf.Clamp(newReduction, 0.5f, 1f); // Cap the reduction at 50%

        totalReductionMult = newReduction / upgradeReductionMult;
        OnNextWaveInfoUpdated?.Invoke(GetNextWaveData());
        log.Info($"Raw meteorite reduction: {newReduction}, upgrade multiplier: {upgradeReductionMult}, final reduction: {totalReductionMult}, intercepter count: {interceptorCount}");
    }

    private int GetSpawnAmountWithMult(MeteoriteWaveData wave)
    {
        return Mathf.FloorToInt(wave.Amount * totalReductionMult);
    }

    private void HandleResearchCompleted()
    {
        upgradeReductionMult = techManager.GetModifiedValue(1f, ModifierType.InterceptorEffectiveness, "interceptor_cannon");
        UpdateMeteoriteAmountMult();
    }

    private void ResetImpactSelectionForWave()
    {
        meteoriteImpactSize = Mathf.Max(0.01f, impactSpotSize);
        waveSpotCount = Mathf.Max(1, Mathf.CeilToInt((SpawnRange.y - SpawnRange.x) / meteoriteImpactSize));
        waveSecondHitChance = Mathf.Clamp01(secondHitChance);
        waveThirdHitChance = Mathf.Clamp01(thirdHitChance);
        waveFourthOrMoreHitChance = Mathf.Clamp01(fourthOrMoreHitChance);

        if (impactSpotHitCounts.Length < waveSpotCount)
        {
            impactSpotHitCounts = new int[waveSpotCount];
        }
        System.Array.Clear(impactSpotHitCounts, 0, waveSpotCount);

        for (int i = 0; i < impactBuckets.Length; i++)
        {
            impactBuckets[i].Clear();
        }

        List<int> firstHitBucket = impactBuckets[0];
        for (int i = 0; i < waveSpotCount; i++)
        {
            firstHitBucket.Add(i);
        }
    }

    private float GetWeightedImpactX()
    {
        // Get a bucket based on weighted random
        int chosenBucket = GetWeightedBucketIndex();
        List<int> bucket = impactBuckets[chosenBucket];

        if (bucket.Count == 0)
        {
            log.Warning($"Meteorite wave bucket {chosenBucket} was empty, falling back to random spawn.");
            return Random.Range(SpawnRange.x, SpawnRange.y);
        }

        // Pick a random index from the bucket and remove it
        int pickedIndexInBucket = Random.Range(0, bucket.Count);
        int chosenSpotIndex = bucket[pickedIndexInBucket];
        int lastIndex = bucket.Count - 1;
        bucket[pickedIndexInBucket] = bucket[lastIndex];
        bucket.RemoveAt(lastIndex);

        // Update the bucket of the spot that got hit
        int previousHits = impactSpotHitCounts[chosenSpotIndex];
        int newHits = previousHits + 1;
        impactSpotHitCounts[chosenSpotIndex] = newHits;
        int newBucketIndex = Mathf.Clamp(newHits, 0, impactBuckets.Length - 1);
        impactBuckets[newBucketIndex].Add(chosenSpotIndex);

        float minX = SpawnRange.x + (chosenSpotIndex * meteoriteImpactSize);
        float maxX = Mathf.Min(minX + meteoriteImpactSize, SpawnRange.y);
        return Random.Range(minX, maxX);
    }

    private int GetWeightedBucketIndex()
    {
        float firstWeight = impactBuckets[0].Count;
        float secondWeight = impactBuckets[1].Count * waveSecondHitChance;
        float thirdWeight = impactBuckets[2].Count * waveThirdHitChance;
        float fourthWeight = impactBuckets[3].Count * waveFourthOrMoreHitChance;
        float totalWeight = firstWeight + secondWeight + thirdWeight + fourthWeight;

        if (totalWeight <= 0f)
        {
            for (int i = 0; i < impactBuckets.Length; i++)
            {
                if (impactBuckets[i].Count > 0)
                    return i;
            }

            return 0;
        }

        float randomValue = Random.value * totalWeight;
        if (randomValue < firstWeight)
            return 0;

        randomValue -= firstWeight;
        if (randomValue < secondWeight)
            return 1;

        randomValue -= secondWeight;
        if (randomValue < thirdWeight)
            return 2;

        return 3;
    }

    private void SpawnMeteorite(float targetImpactX, float angle)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float horizontalOffset = Mathf.Tan(angleRadians) * SpawnHeight;
        float spawnX = targetImpactX - horizontalOffset;

        Vector3 spawnPosition = new(spawnX, SpawnHeight, -1);
        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;
        GameObject meteor = Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);

        // Randomize rotation
        Meteorite meteoriteComponent = meteor.GetComponent<Meteorite>();
        float rotationSpeed = Random.Range(RotationSpeedRange.x, RotationSpeedRange.y);
        rotationSpeed = Random.value > 0.5f ? -rotationSpeed : rotationSpeed;
        meteoriteComponent.RotationSpeed = rotationSpeed;

        // Randomize speed
        float speedChange = Random.Range(SpeedRandomizationRange.x, SpeedRandomizationRange.y);
        meteoriteComponent.Speed += meteoriteComponent.Speed * speedChange;
    }
}
