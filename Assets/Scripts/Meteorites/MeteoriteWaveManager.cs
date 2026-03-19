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
    private int nextWaveIndex = 0;

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
            float impactPos = Random.Range(SpawnRange.x, SpawnRange.y);
            float angle = Random.Range(-SpawnAngleRange, SpawnAngleRange);
            SpawnMeteorite(impactPos, angle);

            // Wait the interval before spawning the next one (no wait after last spawn)
            if (i < spawnAmount - 1)
            {
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
            newReduction -= 0.0625f; // Flat reduction per interceptor
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

    private void SpawnMeteorite(float targetImpactX, float angle)
    {
        float angleRadians = angle * Mathf.Deg2Rad;
        float horizontalOffset = Mathf.Tan(angleRadians) * SpawnHeight;
        float spawnX = targetImpactX - horizontalOffset;

        Vector3 spawnPosition = new(spawnX, SpawnHeight, -1);
        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;
        GameObject meteor = Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);

        // Randomize rotation
        float rotationSpeed = Random.Range(RotationSpeedRange.x, RotationSpeedRange.y);
        rotationSpeed = Random.value > 0.5f ? -rotationSpeed : rotationSpeed;
        meteor.GetComponent<Rigidbody2D>().angularVelocity = rotationSpeed;

        // Randomize speed
        Meteorite meteoriteComponent = meteor.GetComponent<Meteorite>();
        float speedChange = Random.Range(SpeedRandomizationRange.x, SpeedRandomizationRange.y);
        meteoriteComponent.Speed += meteoriteComponent.Speed * speedChange;
    }
}
