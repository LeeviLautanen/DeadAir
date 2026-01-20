using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private ResourceStack[] startingResources;

    private readonly Dictionary<string, ResourceStack> resources = new();

    public event System.Action<string, int, int> OnResourceChanged;

    public void AddResource(ResourceData data, int amount)
    {
        if (!resources.ContainsKey(data.Id))
        {
            resources[data.Id] = new(data, amount);
        }

        ResourceStack stack = resources[data.Id];
        stack.TryAdd(amount);
        OnResourceChanged?.Invoke(stack.Data.Id, stack.Amount, stack.MaxAmount);
    }

    public ResourceStack GetResource(string Id)
    {
        if (resources.TryGetValue(Id, out var resource))
            return resource;

        Debug.LogError($"Resource {Id} not found!");
        return null;
    }

    public bool TryGetResource(string Id, out ResourceStack resource)
    {
        return resources.TryGetValue(Id, out resource);
    }

    public IEnumerable<ResourceStack> GetAllResources()
    {
        foreach (var resource in resources.Values)
            yield return resource;
    }

    public bool HasEnough(string Id, int amount)
    {
        var resource = GetResource(Id);
        return resource != null && resource.Amount >= amount;
    }

    public bool TryConsume(Dictionary<string, int> cost)
    {
        // First, check if all costs can be paid
        foreach (var kvp in cost)
        {
            if (!HasEnough(kvp.Key, kvp.Value))
                return false;
        }

        // Then deduct all resources
        foreach (var kvp in cost)
        {
            GetResource(kvp.Key).TryRemove(kvp.Value);
        }

        return true;
    }

    private void Awake()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        resources.Clear();

        foreach (ResourceStack stack in startingResources)
        {
            if (stack.Data != null)
            {
                resources[stack.Data.Id] = stack;
                OnResourceChanged?.Invoke(stack.Data.Id, stack.Amount, stack.MaxAmount);
            }
        }
    }
}
