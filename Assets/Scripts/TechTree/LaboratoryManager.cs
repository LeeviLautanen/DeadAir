using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaboratoryManager : MonoBehaviour
{
    public float MinLabResearchRateIncrease = 0.1f;

    private static readonly Logger log = new(true, LogLevel.Info);
    private TechManager techManager;
    private readonly Dictionary<Building, bool> laboratories = new();
    [SerializeField] private float researchRateMultiplier = 1f;
    private float researchRate = 0f;
    private int laboratoryCount;

    private void Start()
    {
        techManager = FindFirstObjectByType<TechManager>();
    }

    private void Update()
    {
        if (laboratoryCount > 0)
            techManager.Research(researchRate * researchRateMultiplier * Time.deltaTime);
    }

    public void SetLaboratoryState(Building lab, bool isOperational)
    {
        laboratories[lab] = isOperational;

        // Prevent two calls from the same lab counting as two
        laboratoryCount = laboratories.Values.Count(v => v);

        researchRate = GetResearchRate(laboratoryCount);
    }

    private float GetResearchRate(int labCount)
    {
        if (labCount <= 0)
            return 0f;

        float changeAmount = 0f;
        for (int i = 1; i < labCount + 1; i++)
        {
            changeAmount += Mathf.Max(MinLabResearchRateIncrease, 1f / i);
        }

        log.Info($"Calculated research rate: {changeAmount} for {labCount} labs");

        return changeAmount;
    }
}
