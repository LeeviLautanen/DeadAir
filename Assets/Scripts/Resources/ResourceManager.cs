using System.Collections.Generic;
using UnityEngine;
using System;
using Mono.Cecil;

public class ResourceManager : MonoBehaviour
{
    public event Action<string, float, float> OnResourceChanged;
    [SerializeField] private List<ResourceAmount> startingResources = new();
    private readonly Dictionary<string, ResourceAmount> resourceLookup = new();
    private readonly Dictionary<string, float> resourceMaxLookup = new();

    private void Awake()
    {
        InitializeResources();
    }

    public bool TryConsumeResources(List<ResourceAmount> costs)
    {
        if (costs == null || costs.Count == 0) return false;

        int costsProcessed = 0;
        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];
            if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceAmount entry))
            {
                if (entry.Amount < cost.Amount)
                {
                    break;
                }
                entry.Amount -= cost.Amount;
                costsProcessed++;
            }
            else
            {
                break;
            }
        }

        if (costsProcessed != costs.Count)
        {
            // rollback
            for (int i = 0; i < costsProcessed; i++)
            {
                ResourceAmount cost = costs[i];
                if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceAmount entry))
                {
                    entry.Amount += cost.Amount;
                }
            }
            return false;
        }

        foreach (ResourceAmount cost in costs)
        {
            TriggerResourceChanged(cost.Data.Id, resourceLookup[cost.Data.Id].Amount);
        }

        return true;
    }

    public bool TryConsumeResource(string resourceId, float amount)
    {
        if (amount <= 0f) return false;

        if (resourceLookup.TryGetValue(resourceId, out ResourceAmount entry))
        {
            if (entry.Amount < amount)
            {
                return false;
            }
            entry.Amount -= amount;
            TriggerResourceChanged(entry.Data.Id, entry.Amount);
            return true;
        }
        return false;
    }

    public bool AddResources(List<ResourceAmount> newResources)
    {
        if (newResources == null || newResources.Count == 0) return false;

        foreach (ResourceAmount stack in newResources)
        {
            if (resourceLookup.TryGetValue(stack.Data.Id, out ResourceAmount entry))
            {
                AddToStack(entry, stack.Amount);
                TriggerResourceChanged(entry.Data.Id, entry.Amount);
            }
        }

        return true;
    }

    public bool AddResource(string resourceId, float amount)
    {
        if (amount <= 0f) return false;

        if (resourceLookup.TryGetValue(resourceId, out ResourceAmount entry))
        {
            AddToStack(entry, amount);
            TriggerResourceChanged(entry.Data.Id, entry.Amount);
            return true;
        }
        return false;
    }

    public float GetResourceAmount(string resourceId)
    {
        if (resourceLookup.TryGetValue(resourceId, out ResourceAmount entry))
        {
            return entry.Amount;
        }
        return -1f;
    }

    public bool ContainsResource(string resourceId)
    {
        return resourceLookup.ContainsKey(resourceId);
    }

    public float GetResourceMax(string resourceId)
    {
        if (resourceMaxLookup.TryGetValue(resourceId, out float maxAmount))
        {
            return maxAmount;
        }
        return -1f;
    }

    public void ChangeResourceMax(string resourceId, float delta)
    {
        if (resourceMaxLookup.ContainsKey(resourceId))
        {
            resourceMaxLookup[resourceId] = Mathf.Max(0f, resourceMaxLookup[resourceId] + delta);
            if (resourceLookup.TryGetValue(resourceId, out ResourceAmount entry))
            {
                TriggerResourceChanged(entry.Data.Id, entry.Amount);
            }
        }
    }

    public void SmoothResources()
    {
        resourceLookup.TryGetValue("humans", out ResourceAmount humans);
        if (humans != null)
        {
            float maxHumans = resourceMaxLookup["humans"];
            if (humans.Amount < maxHumans)
            {
                AddResource("humans", Mathf.Ceil(humans.Amount * 0.1f));
                TriggerResourceChanged(humans.Data.Id, humans.Amount);
            }
            else if (humans.Amount > maxHumans)
            {
                TryConsumeResource("humans", Mathf.Ceil(humans.Amount * 0.1f));
                TriggerResourceChanged(humans.Data.Id, humans.Amount);
            }
        }
    }

    public Dictionary<string, float> GetResourceStates()
    {
        Dictionary<string, float> states = new();
        foreach (var pair in resourceLookup)
        {
            states[pair.Key] = pair.Value.Amount;
        }
        return states;
    }

    public void LoadResourceStates(Dictionary<string, float> states)
    {
        foreach (var pair in states)
        {
            if (resourceLookup.TryGetValue(pair.Key, out ResourceAmount stack))
            {
                TriggerResourceChanged(stack.Data.Id, stack.Amount);
            }
        }
    }

    private void InitializeResources()
    {
        resourceLookup.Clear();

        foreach (ResourceAmount stack in startingResources)
        {
            if (stack.Data != null)
            {
                resourceLookup[stack.Data.Id] = stack;
                resourceMaxLookup[stack.Data.Id] = stack.Data.DefaultMaxAmount;
                TriggerResourceChanged(stack.Data.Id, stack.Amount);
            }
        }

        Building.OnBuildingDestroyed += HandleBuildingDestroyed;
    }

    private void HandleBuildingDestroyed(GameObject buildingGO)
    {
        if (buildingGO.TryGetComponent(out Building building))
        {
            var data = building.Data;
            if (data == null)
            {
                Debug.LogError("No building data found for resource processing.");
                return;
            }

            foreach (var effect in data.CapacityEffects)
            {
                ChangeResourceMax(effect.Data.Id, -effect.Amount);
            }
        }
    }

    private bool AddToStack(ResourceAmount stack, float amount)
    {
        if (amount <= 0f) return false;

        if (stack.Amount + amount > resourceMaxLookup[stack.Data.Id])
        {
            stack.Amount = resourceMaxLookup[stack.Data.Id];
        }
        else
        {
            stack.Amount += amount;
        }

        return true;
    }

    private void TriggerResourceChanged(string id, float amount)
    {
        OnResourceChanged?.Invoke(id, amount, resourceMaxLookup[id]);
    }
}
