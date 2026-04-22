using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ResourceUI : MonoBehaviour
{
    private static readonly Logger log = new(nameof(ResourceUI));
    private ResourceManager resourceManager;
    private readonly Dictionary<string, List<TMP_Text>> resourceTexts = new();
    private readonly Dictionary<string, float> oldRates = new();
    private readonly StringBuilder sb = new(128);

    private void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogWarning("Resource UI manager was null on enable");
            return;
        }

        resourceTexts["humans"] = new()
        {
            GameObject.Find("HumansCounter").GetComponent<TMP_Text>(),
            GameObject.Find("HumansRateText").GetComponent<TMP_Text>()
        };
        resourceTexts["materials"] = new()
        {
            GameObject.Find("MaterialsCounter").GetComponent<TMP_Text>(),
            GameObject.Find("MaterialsRateText").GetComponent<TMP_Text>()
        };
        resourceTexts["energy"] = new()
        {
            GameObject.Find("EnergyCounter").GetComponent<TMP_Text>(),
            GameObject.Find("EnergyRateText").GetComponent<TMP_Text>(),
        };

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
                TMP_Text counterText = kvp.Value[0];

                float amount = resourceManager.GetResourceAmount(kvp.Key);
                float maxAmount = resourceManager.GetResourceMax(kvp.Key);
                float reservedAmount = resourceManager.GetResourceReserved(kvp.Key);
                float rate = resourceManager.GetResourceRate(kvp.Key);

                if (!oldRates.ContainsKey(kvp.Key))
                {
                    oldRates[kvp.Key] = rate;
                }

                if (Mathf.Abs(rate - oldRates[kvp.Key]) > 0.01f)
                {
                    oldRates[kvp.Key] = rate;
                }

                if (float.IsNaN(rate) || float.IsInfinity(rate))
                {
                    rate = 0;
                }

                //FormatPooled(kvp.Key, amount, maxAmount, reservedAmount, oldRates[kvp.Key]);
                //sb.Clear();
                //sb.AppendFormat("{0:F0}/{1:F0}", amount - reservedAmount, maxAmount);
                counterText.SetText("{0:0}/{1:0}", amount - reservedAmount, maxAmount);

                if (kvp.Value.Count > 1)
                {
                    TMP_Text rateText = kvp.Value[1];
                    sb.Clear();
                    sb.AppendFormat("{0:F0}/hr", rate);
                    rateText.SetText(sb);
                }
            }
        }
    }
}
