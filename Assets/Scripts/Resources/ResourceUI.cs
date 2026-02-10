using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    private ResourceManager manager;
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
            if (manager.ContainsResource(kvp.Key))
            {
                float amount = manager.GetResourceAmount(kvp.Key);
                float maxAmount = manager.GetResourceMax(kvp.Key);
                kvp.Value.SetText(Format(kvp.Key, amount, maxAmount));
            }
            else
            {
                kvp.Value.SetText(kvp.Key + ":");
            }
        }
    }

    private void OnEnable()
    {
        manager = FindFirstObjectByType<ResourceManager>();
        if (manager == null)
        {
            Debug.LogWarning("Resource UI manager was null on enable");
            return;
        }
        manager.OnResourceChanged += OnResourceChanged;

        foreach (var kvp in resourceTexts)
        {
            if (manager.ContainsResource(kvp.Key))
            {
                float amount = manager.GetResourceAmount(kvp.Key);
                float maxAmount = manager.GetResourceMax(kvp.Key);
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
        if (manager == null)
        {
            Debug.LogWarning("Resource UI manager was null on disable");
            return;
        }
        manager.OnResourceChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(string id, float amount, float maxAmount)
    {
        if (!resourceTexts.ContainsKey(id)) return;
        Debug.Log($"Resource '{id}' changed to {amount}");
        resourceTexts[id].text = Format(id, amount, maxAmount);
    }

    private string Format(string name, float amount, float maxAmount = 0)
    {
        if (maxAmount > 0)
        {

            return $"{name}: {amount:F0} / {maxAmount:F0}";
        }
        else
        {
            return $"{name}: {amount:F0}";
        }
    }
}
