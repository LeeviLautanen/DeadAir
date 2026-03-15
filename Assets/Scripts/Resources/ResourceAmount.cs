using UnityEngine;

public interface IResourceAmount
{
    ResourceData Data { get; }
    float Amount { get; }
}

[System.Serializable]
public class ReadonlyResourceAmount : IResourceAmount
{
    [SerializeField] private ResourceData data;
    [SerializeField] private float amount;
    public ReadonlyResourceAmount(ResourceData data, float amount)
    {
        this.data = data;
        this.amount = amount;
    }

    // Getters
    public ResourceData Data => data;
    public float Amount => amount;
}

[System.Serializable]
public class ResourceAmount : IResourceAmount
{
    [SerializeField] private ResourceData data;
    [SerializeField] private float amount;

    public ResourceAmount(ResourceData data, float amount)
    {
        this.data = data;
        this.amount = amount;
    }

    public ResourceData Data { get => data; set => data = value; }
    public float Amount { get => amount; set => amount = value; }
}
