// ResourceInstance.cs
using System;
using UnityEngine;

[Serializable]
public class ResourceStack
{
    public event Action<ResourceStack> OnChanged;

    private ResourceData data;
    private int amount;
    private int maxAmount;

    public ResourceData Data => data;
    public int Amount => amount;
    public int MaxAmount => maxAmount;

    public ResourceStack(ResourceData data, int initialAmount = 0, int? customMaxAmount = null)
    {
        this.data = data;
        this.maxAmount = customMaxAmount ?? data.DefaultMaxAmount;
        SetAmount(initialAmount);
    }

    public bool CanAdd(int value) => amount + value <= maxAmount && value >= 0;
    public bool CanRemove(int value) => amount - value >= 0 && value >= 0;

    public bool TryAdd(int value)
    {
        if (!CanAdd(value)) return false;

        amount += value;
        OnChanged?.Invoke(this);
        return true;
    }

    public bool TryRemove(int value)
    {
        if (!CanRemove(value)) return false;

        amount -= value;
        OnChanged?.Invoke(this);
        return true;
    }

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Clamp(newAmount, 0, maxAmount);
        OnChanged?.Invoke(this);
    }

    public void SetMaxCap(int newMaxCap)
    {
        maxAmount = Mathf.Max(1, newMaxCap);
        if (amount > maxAmount) amount = maxAmount;
        OnChanged?.Invoke(this);
    }

    public float GetPercentage() => (float)amount / maxAmount;

    public override string ToString() => $"{data.DisplayName}: {amount}/{maxAmount}";
}