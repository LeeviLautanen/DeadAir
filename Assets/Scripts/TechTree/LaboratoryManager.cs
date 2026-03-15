using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaboratoryManager : MonoBehaviour
{
    public float MinLabResearchRateIncrease = 0.1f;

    private static readonly Logger log = new(nameof(LaboratoryManager));
    private TechManager techManager;
    private TimeManager timeManager;
    private readonly Dictionary<Building, bool> laboratories = new();
    [SerializeField] private float researchRateMultiplier = 1f;
    private float totalResearchRate = 0f;
    private int laboratoryCount;

    private void Start()
    {
        timeManager = TimeManager.Instance;
        techManager = FindFirstObjectByType<TechManager>();
        TechManager.OnResearchCompleted += HandleResearchCompleted;
    }

    private void Update()
    {
        float deltaTime = timeManager.DeltaTime;

        if (deltaTime > 0 && laboratoryCount > 0)
        {
            techManager.Research(totalResearchRate * deltaTime);
        }
    }

    public void SetLaboratoryState(Building lab, bool isOperational)
    {
        laboratories[lab] = isOperational;
        UpdateResearchRate();
    }

    private void UpdateResearchRate()
    {
        // Prevent two calls from the same lab counting as two
        laboratoryCount = laboratories.Values.Count(v => v);

        if (laboratoryCount <= 0)
        {
            totalResearchRate = 0f;
            return;
        }

        float changeAmount = 0f;
        for (int i = 1; i < laboratoryCount + 1; i++)
        {
            changeAmount += Mathf.Max(MinLabResearchRateIncrease, 1f / i);
        }

        totalResearchRate = changeAmount * researchRateMultiplier;
        log.Info($"Raw research: {changeAmount}, upgrade multiplier: {researchRateMultiplier}, final research rate: {totalResearchRate}, lab count: {laboratoryCount}");
    }

    private void HandleResearchCompleted()
    {
        researchRateMultiplier = techManager.GetModifiedValue(1f, ModifierType.ResearchRate, "laboratory");
        UpdateResearchRate();
    }
}
