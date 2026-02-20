using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public event Action<string, float, float, float> OnResourceChanged;

    private static readonly Logger log = new();
    private static System.Random rng = new();
    [SerializeField] private List<ResourceAmount> startingResources = new();
    private readonly Dictionary<string, ResourceAmount> resourceLookup = new();
    private readonly Dictionary<string, float> resourceMaxLookup = new();
    private readonly Dictionary<string, float> reservationLookup = new();
    private readonly List<Building>[] resourceUserLists = new List<Building>[10];
    private readonly List<Building>[] resourceReserverLists = new List<Building>[10];
    private readonly List<Building> unregisterBuffer = new();
    private readonly List<Building> registerBuffer = new();

    private void Awake()
    {
        InitializeResources();
    }

    private void Update()
    {
        // Handle pending registers
        foreach (Building building in registerBuffer)
        {
            var list = resourceUserLists[building.Data.ResourcePriority];
            log.Log($"Registering building {building.Data.DisplayName} to resource manager ({resourceUserLists[building.Data.ResourcePriority].Count}).");
            if (!list.Contains(building))
                list.Add(building);
        }
        registerBuffer.Clear();

        // Handle pending unregisters
        foreach (Building building in unregisterBuffer)
        {
            var list = resourceUserLists[building.Data.ResourcePriority];
            if (list.Contains(building))
                list.Remove(building);
        }
        unregisterBuffer.Clear();

        // Add resources
        foreach (var userList in resourceUserLists)
        {
            foreach (var user in userList)
            {
                if (user.CurrentState != BuildingState.Operational
                    || user.Data.ProducedResources.Count == 0) continue;

                AddResourceRates(user.Data.ProducedResources);
            }
        }

        // Consume resources
        foreach (var userList in resourceUserLists)
        {
            // Randomize to prevent bias inside the same priority
            for (int i = userList.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (userList[i], userList[j]) = (userList[j], userList[i]);
            }

            foreach (var user in userList)
            {
                if (user.CurrentState == BuildingState.Inactive
                    || user.Data.ConsumedResources.Count == 0) continue;

                if (TryConsumeResourceRates(user.Data.ConsumedResources))
                {
                    if (user.CurrentState == BuildingState.OutOfResources) user.OnResourcesRecovered();
                }
                else
                {
                    if (user.CurrentState != BuildingState.OutOfResources) user.OnOutOfResources();
                }
            }
        }

        SmoothResources();
    }

    public void RegisterResourceUser(Building building)
    {
        if (registerBuffer.Contains(building)) return;
        registerBuffer.Add(building);
    }

    public void UnregisterResourceUser(Building building)
    {
        if (unregisterBuffer.Contains(building)) return;
        unregisterBuffer.Add(building);
    }

    public bool TryConsumeResources(List<ResourceAmount> costs)
    {
        int costsProcessed = 0;
        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount cost = costs[i];
            if (resourceLookup.TryGetValue(cost.Data.Id, out ResourceAmount entry))
            {
                if (cost.Amount > (entry.Amount + reservationLookup[entry.Data.Id]))
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

        return true;
    }

    public bool TryConsumeResourceRates(List<ResourceAmount> rates)
    {
        int costsProcessed = 0;
        for (int i = 0; i < rates.Count; i++)
        {
            ResourceAmount rate = rates[i];
            resourceLookup.TryGetValue(rate.Data.Id, out ResourceAmount entry);
            float amount = rate.Amount * Time.deltaTime;
            if (amount > (entry.Amount + reservationLookup[entry.Data.Id]))
            {
                break;
            }
            //log.Log($"Successfully consumed {amount} of {rate.Data.DisplayName} (current: {entry.Amount}, reserved: {reservationLookup[entry.Data.Id]})");
            entry.Amount -= amount;
            costsProcessed++;
        }

        if (costsProcessed != rates.Count)
        {
            // rollback
            for (int i = 0; i < costsProcessed; i++)
            {
                ResourceAmount rate = rates[i];
                if (resourceLookup.TryGetValue(rate.Data.Id, out ResourceAmount entry))
                {
                    entry.Amount += rate.Amount * Time.deltaTime;
                }
            }
            return false;
        }

        return true;
    }

    public bool TryConsumeResource(string resourceId, float amount)
    {
        if (amount <= 0f) return false;

        resourceLookup.TryGetValue(resourceId, out ResourceAmount entry);
        if (amount > (entry.Amount + reservationLookup[entry.Data.Id]))
        {
            return false;
        }
        entry.Amount -= amount;
        return true;
    }

    public bool TryReserveResource(ResourceAmount reservation)
    {
        resourceLookup.TryGetValue(reservation.Data.Id, out ResourceAmount entry);
        float amount = reservation.Amount;
        if (amount > (entry.Amount + reservationLookup[entry.Data.Id])
            || reservationLookup[entry.Data.Id] + amount > resourceMaxLookup[entry.Data.Id])
        {
            return false;
        }
        reservationLookup[entry.Data.Id] += amount;
        log.Log($"Reserved {amount} of {reservation.Data.DisplayName} (current: {entry.Amount}, reserved: {reservationLookup[entry.Data.Id]}, max: {resourceMaxLookup[entry.Data.Id]})");
        return true;
    }

    public bool TryReserveResources(List<ResourceAmount> reservations)
    {
        int processed = 0;
        for (int i = 0; i < reservations.Count; i++)
        {
            if (!TryReserveResource(reservations[i]))
            {
                break;
            }
            processed++;
        }

        if (processed != reservations.Count)
        {
            // rollback
            for (int i = 0; i < processed; i++)
            {
                ResourceAmount reservation = reservations[i];
                reservationLookup[reservation.Data.Id] -= reservation.Amount;
            }
            return false;
        }

        return true;
    }

    public bool ReleaseReservation(ResourceAmount reservation)
    {
        float amount = reservation.Amount;
        if (amount > reservationLookup[reservation.Data.Id])
        {
            return false;
        }
        log.Log($"Released reservation of {amount} of {reservation.Data.DisplayName} (reserved: {reservationLookup[reservation.Data.Id]}, max: {resourceMaxLookup[reservation.Data.Id]})");
        reservationLookup[reservation.Data.Id] = Mathf.Max(0f, reservationLookup[reservation.Data.Id] - amount);
        return true;
    }

    public bool ReleaseReservations(List<ResourceAmount> reservations)
    {
        foreach (ResourceAmount reservation in reservations)
        {
            if (!ReleaseReservation(reservation))
            {
                return false;
            }
        }
        return true;
    }

    public bool AddResourceRates(List<ResourceAmount> rates)
    {
        foreach (ResourceAmount rate in rates)
        {
            if (resourceLookup.TryGetValue(rate.Data.Id, out ResourceAmount entry))
            {
                float amount = rate.Amount * Time.deltaTime;
                if (entry.Amount + amount > resourceMaxLookup[entry.Data.Id])
                {
                    entry.Amount = resourceMaxLookup[entry.Data.Id];
                }
                else
                {
                    entry.Amount += amount;
                }
            }
        }

        return true;
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

    public float GetResourceReserved(string resourceId)
    {
        if (reservationLookup.TryGetValue(resourceId, out float reservedAmount))
        {
            return reservedAmount;
        }
        return -1f;
    }

    public bool ApplyCapacityEffects(List<ResourceAmount> capacities)
    {
        foreach (ResourceAmount capacity in capacities)
        {
            ChangeResourceMax(capacity.Data.Id, capacity.Amount);
        }
        return true;
    }

    public bool RemoveCapacityEffects(List<ResourceAmount> capacities)
    {
        foreach (ResourceAmount capacity in capacities)
        {
            ChangeResourceMax(capacity.Data.Id, -capacity.Amount);
        }
        return true;
    }

    public void ChangeResourceMax(string resourceId, float delta)
    {
        resourceMaxLookup.ContainsKey(resourceId);
        resourceMaxLookup[resourceId] = Mathf.Max(0f, resourceMaxLookup[resourceId] + delta);
    }

    private void SmoothResources()
    {
        resourceLookup.TryGetValue("humans", out ResourceAmount humans);
        if (humans != null)
        {
            float maxHumans = resourceMaxLookup["humans"];
            ResourceAmount newAmount = new() { Data = humans.Data, Amount = maxHumans * 0.1f };
            List<ResourceAmount> rates = new() { newAmount };
            if (humans.Amount < maxHumans)
            {
                AddResourceRates(rates);
            }
            else if (humans.Amount > maxHumans)
            {
                TryConsumeResourceRates(rates);
            }
        }
    }

    private void InitializeResources()
    {
        for (int i = 0; i < resourceUserLists.Length; i++)
            resourceUserLists[i] = new List<Building>();

        for (int i = 0; i < resourceReserverLists.Length; i++)
            resourceReserverLists[i] = new List<Building>();

        foreach (ResourceAmount stack in startingResources)
        {
            if (stack.Data != null)
            {
                resourceLookup[stack.Data.Id] = stack;
                resourceMaxLookup[stack.Data.Id] = stack.Data.DefaultMaxAmount;
                reservationLookup[stack.Data.Id] = 0f;
            }
        }
    }
}
