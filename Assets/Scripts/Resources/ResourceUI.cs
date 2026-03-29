using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ResourceUI : MonoBehaviour
{
    private static readonly Logger log = new(nameof(ResourceUI));
    private ResourceManager resourceManager;
    private readonly Dictionary<string, TMP_Text> resourceTexts = new();
    private readonly StringBuilder sb = new(128);

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
                FormatPooled(kvp.Key, amount, maxAmount, reservedAmount, rate);
                kvp.Value.SetText(sb);
            }
        }
    }

    private void FormatPooled(string name, float amount, float maxAmount = 0, float reservedAmount = 0, float rate = 0)
    {
        sb.Clear();
        sb.Append(name).Append(": ").AppendFormat("{0:F0}", amount);

        if (maxAmount > 0)
        {
            sb.Append(" / ").AppendFormat("{0:F0}", maxAmount);

            if (reservedAmount > 0)
            {
                sb.Append(" (Available: ")
                   .AppendFormat("{0:F0}", amount - reservedAmount)
                   .Append(", Δ: ").AppendFormat("{0:F1}", rate)
                   .Append("/hr)");
            }
            else
            {
                sb.Append(" (Δ: ").AppendFormat("{0:F0}", rate).Append("/hr)");
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
            return $"{name}: {amount:F0} / {maxAmount:F0} (Available: {amount - reservedAmount:F0}, Δ: {Mathf.FloorToInt(rate)}/hr)";
        }
        else if (maxAmount > 0)
        {
            return $"{name}: {amount:F0} / {maxAmount:F0} (Δ: {Mathf.FloorToInt(rate)}/hr)";
        }
        else
        {
            return $"{name}: {amount:F0}";
        }
    }
}
