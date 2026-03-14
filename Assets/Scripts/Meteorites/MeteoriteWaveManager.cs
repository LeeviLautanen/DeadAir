using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeteoriteWaveManager : MonoBehaviour
{
    public GameObject MeteoritePrefab;
    public Vector2 SpawnRange = new(-10, 10);
    public float SpawnHeight = 40f;
    public float SpawnAngleRange = 20f;
    public float rotationSpeedMin = 30f;
    public float rotationSpeedMax = 180f;
    public float speedRandomizationMult = 0.1f;

    private static readonly Logger log = new(nameof(MeteoriteWaveManager));
    private TimeManager timeManager;
    private TechManager techManager;
    private MeteoriteParticleSystem meteoriteParticleSystem;
    private List<MeteoriteWaveData> attackWaves = new();
    private Dictionary<Building, bool> interceptors = new();
    private int interceptorCount;
    [SerializeField] private float upgradeReductionMult = 1f;
    [SerializeField] private float totalReductionMult = 1f;

    private async void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        techManager = FindFirstObjectByType<TechManager>();
        meteoriteParticleSystem = FindFirstObjectByType<MeteoriteParticleSystem>();

        attackWaves = await Utility.LoadAllByLabel<MeteoriteWaveData>("AttackWaveSO");
        foreach (MeteoriteWaveData wave in attackWaves)
        {
            timeManager.ScheduleEvent(() => HandleWaveSpawn(wave), wave.Day, wave.Hour);
        }

        TechManager.OnResearchCompleted += HandleResearchCompleted;
    }

    public void SetInterceptorState(Building interceptor, bool isOperational)
    {
        interceptors[interceptor] = isOperational;
        UpdateMeteoriteAmountMult();
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
            newReduction -= 0.05f; // Flat 5% reduction per interceptor
        }

        newReduction = Mathf.Clamp(newReduction, 0.5f, 1f); // Cap the reduction at 50%

        totalReductionMult = newReduction / upgradeReductionMult;
        log.Info($"Raw meteorite reduction: {newReduction}, upgrade multiplier: {upgradeReductionMult}, final reduction: {totalReductionMult}, intercepter count: {interceptorCount}");
    }

    private void HandleWaveSpawn(MeteoriteWaveData wave)
    {
        int spawnAmount = Mathf.FloorToInt(wave.Amount * totalReductionMult);
        log.Info($"Spawning wave with base amount {wave.Amount}, total multiplier {totalReductionMult}, final spawn amount {spawnAmount}");
        StartCoroutine(SpawnWave(spawnAmount, wave.Duration));
    }

    private void HandleResearchCompleted()
    {
        upgradeReductionMult = techManager.GetModifiedValue(1f, ModifierType.InterceptorEffectiveness, "interceptor_cannon");
        UpdateMeteoriteAmountMult();
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
        GameObject meteor = Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);

        // Randomize rotation
        float rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
        rotationSpeed = Random.value > 0.5f ? -rotationSpeed : rotationSpeed;
        meteor.GetComponent<Rigidbody2D>().angularVelocity = rotationSpeed;

        // Randomize speed
        Meteorite meteoriteComponent = meteor.GetComponent<Meteorite>();
        float speedChange = Random.Range(-1f, 1f) * speedRandomizationMult;
        meteoriteComponent.Speed += meteoriteComponent.Speed * speedChange;
    }
}
