using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceTextUI : MonoBehaviour
{
    private ResourceManager manager;
    private readonly Dictionary<string, TMP_Text> resourceTexts;

    private void Awake()
    {
        resourceTexts["human"] = GameObject.Find("HumanText").GetComponent<TMP_Text>();
        resourceTexts["material"] = GameObject.Find("MaterialText").GetComponent<TMP_Text>();
        resourceTexts["energy"] = GameObject.Find("EnergyText").GetComponent<TMP_Text>();
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
            if (manager.TryGetResource(kvp.Key, out var stack))
            {
                kvp.Value.text = Format(kvp.Key, stack.Amount, stack.MaxAmount);
            }
            else
            {
                kvp.Value.text = "--";
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

    private void OnResourceChanged(string id, int amount, int maxAmount)
    {
        if (!resourceTexts.ContainsKey(id)) return;
        resourceTexts[id].text = Format(id, amount, maxAmount);
    }

    private string Format(string name, int amount, int maxAmount)
    {
        return $"{name}: {amount} / {maxAmount}";
    }
}
