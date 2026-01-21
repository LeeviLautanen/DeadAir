using System.Collections.Generic;
using UnityEngine;
using System;
using Mono.Cecil;

public class ResourceManager : MonoBehaviour
{
    public event Action<string, int, int> OnResourceChanged;
    [SerializeField] private List<ResourceStack> startingResources = new();
    private readonly Dictionary<string, ResourceStack> resourceLookup = new();
    private readonly Dictionary<string, int> resourceMaxLookup = new();

    public bool TryConsumeResources(List<ResourceStack> costs)
    {
        if (costs == null || costs.Count == 0) return false;

        int costsProcessed = 0;
        for (int i = 0; i < costs.Count; i++)
        {
            ResourceStack cost = costs[i];
            if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceStack entry))
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
                ResourceStack cost = costs[i];
                if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceStack entry))
                {
                    entry.Amount += cost.Amount;
                }
            }
            return false;
        }

        foreach (ResourceStack cost in costs)
        {
            TriggerResourceChanged(cost.Data.Id, resourceLookup[cost.Data.Id].Amount);
        }

        return true;
    }

    public bool TryConsumeResource(string resourceId, int amount)
    {
        if (amount <= 0) return false;

        if (resourceLookup.TryGetValue(resourceId, out ResourceStack entry))
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

    public bool AddResources(List<ResourceStack> newResources)
    {
        if (newResources == null || newResources.Count == 0) return false;

        foreach (ResourceStack stack in newResources)
        {
            if (resourceLookup.TryGetValue(stack.Data.Id, out ResourceStack entry))
            {
                AddToStack(entry, stack.Amount);
                TriggerResourceChanged(entry.Data.Id, entry.Amount);
            }
        }

        return true;
    }

    public bool AddResource(string resourceId, int amount)
    {
        if (amount <= 0) return false;

        if (resourceLookup.TryGetValue(resourceId, out ResourceStack entry))
        {
            AddToStack(entry, amount);
            TriggerResourceChanged(entry.Data.Id, entry.Amount);
            return true;
        }
        return false;
    }

    public bool HasResources(List<ResourceStack> costs)
    {
        if (costs == null || costs.Count == 0) return true;

        for (int i = 0; i < costs.Count; i++)
        {
            ResourceStack cost = costs[i];
            if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceStack entry))
            {
                if (entry.Amount < cost.Amount)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public int GetResourceAmount(string resourceId)
    {
        if (resourceLookup.TryGetValue(resourceId, out ResourceStack entry))
        {
            return entry.Amount;
        }
        return -1;
    }

    public bool ContainsResource(string resourceId)
    {
        return resourceLookup.ContainsKey(resourceId);
    }

    public int GetResourceMaxAmount(string resourceId)
    {
        if (resourceMaxLookup.TryGetValue(resourceId, out int maxAmount))
        {
            return maxAmount;
        }
        return -1;
    }

    public Dictionary<string, int> GetResourceStates()
    {
        Dictionary<string, int> states = new();
        foreach (var pair in resourceLookup)
        {
            states[pair.Key] = pair.Value.Amount;
        }
        return states;
    }

    public void LoadResourceStates(Dictionary<string, int> states)
    {
        foreach (var pair in states)
        {
            if (resourceLookup.TryGetValue(pair.Key, out ResourceStack stack))
            {
                TriggerResourceChanged(stack.Data.Id, stack.Amount);
            }
        }
    }

    private void Awake()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        resourceLookup.Clear();

        foreach (ResourceStack stack in startingResources)
        {
            if (stack.Data != null)
            {
                resourceLookup[stack.Data.Id] = stack;
                resourceMaxLookup[stack.Data.Id] = stack.Data.DefaultMaxAmount;
                TriggerResourceChanged(stack.Data.Id, stack.Amount);
            }
        }
    }

    private bool AddToStack(ResourceStack stack, int amount)
    {
        if (amount <= 0) return false;

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

    private void TriggerResourceChanged(string id, int amount)
    {
        OnResourceChanged?.Invoke(id, amount, resourceMaxLookup[id]);
    }
}
