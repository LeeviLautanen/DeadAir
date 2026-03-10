using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    private static readonly Logger log = new(true, LogLevel.Info);
    private ResourceManager resourceManager;
    private readonly Dictionary<string, TMP_Text> resourceTexts = new();

    private void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogWarning("Resource UI manager was null on enable");
            return;
        }

        resourceTexts["humans"] = GameObject.Find("HumansCounter").GetComponent<TMP_Text>();
        resourceTexts["materials"] = GameObject.Find("MaterialsCounter").GetComponent<TMP_Text>();
        resourceTexts["energy"] = GameObject.Find("EnergyCounter").GetComponent<TMP_Text>();

        //InvokeRepeating(nameof(UpdateResourceTexts), 0f, 1f);
    }

    private void Update()
    {
        UpdateResourceTexts();
    }

    private void UpdateResourceTexts()
    {
        foreach (var kvp in resourceTexts)
        {
            if (resourceManager.ContainsResource(kvp.Key))
            {
                float amount = resourceManager.GetResourceAmount(kvp.Key);
                float maxAmount = resourceManager.GetResourceMax(kvp.Key);
                float reservedAmount = resourceManager.GetResourceReserved(kvp.Key);
                float rate = resourceManager.GetResourceRate(kvp.Key);
                kvp.Value.SetText(Format(kvp.Key, amount, maxAmount, reservedAmount, rate));
            }
            else
            {
                kvp.Value.SetText(kvp.Key + ":");
            }
        }
    }

    private void OnResourceChanged(string id, float amount, float maxAmount, float reservedAmount)
    {
        if (!resourceTexts.ContainsKey(id)) return;
        //Debug.Log($"Resource '{id}' changed to {amount}");
        resourceTexts[id].text = Format(id, amount, maxAmount, reservedAmount);
    }

    private string Format(string name, float amount, float maxAmount = 0, float reservedAmount = 0, float rate = 0)
    {
        if (maxAmount > 0 && reservedAmount > 0)
        {
            return $"{name}: {amount:F0} / {maxAmount:F0} (Available: {amount - reservedAmount:F0}, Δ: {rate:F1}/s)";
        }
        else if (maxAmount > 0)
        {
            return $"{name}: {amount:F0} / {maxAmount:F0} (Δ: {rate:F1}/s)";
        }
        else
        {
            return $"{name}: {amount:F0}";
        }
    }
}
