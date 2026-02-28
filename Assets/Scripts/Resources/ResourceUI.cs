using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    private ResourceManager resourceManager;
    private readonly Dictionary<string, TMP_Text> resourceTexts = new();

    private void Awake()
    {
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
                kvp.Value.SetText(Format(kvp.Key, amount, maxAmount, reservedAmount));
            }
            else
            {
                kvp.Value.SetText(kvp.Key + ":");
            }
        }
    }

    private void OnEnable()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogWarning("Resource UI manager was null on enable");
            return;
        }

        foreach (var kvp in resourceTexts)
        {
            if (resourceManager.ContainsResource(kvp.Key))
            {
                float amount = resourceManager.GetResourceAmount(kvp.Key);
                float maxAmount = resourceManager.GetResourceMax(kvp.Key);
                kvp.Value.text = Format(kvp.Key, amount, maxAmount);
            }
            else
            {
                kvp.Value.text = $"{kvp.Key}:";
            }
        }
    }

    private void OnDisable()
    {

    }

    private void OnResourceChanged(string id, float amount, float maxAmount, float reservedAmount)
    {
        if (!resourceTexts.ContainsKey(id)) return;
        //Debug.Log($"Resource '{id}' changed to {amount}");
        resourceTexts[id].text = Format(id, amount, maxAmount, reservedAmount);
    }

    private string Format(string name, float amount, float maxAmount = 0, float reservedAmount = 0)
    {
        if (maxAmount > 0 && reservedAmount > 0)
        {
            return $"{name}: {amount:F0} / {maxAmount:F0} (Available: {amount - reservedAmount:F0})";
        }
        else if (maxAmount > 0)
        {
            return $"{name}: {amount:F0} / {maxAmount:F0}";
        }
        else
        {
            return $"{name}: {amount:F0}";
        }
    }
}
