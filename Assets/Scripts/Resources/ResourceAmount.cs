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
    public ResourceData Data { get; set; }
    public float Amount { get; set; }
    public ResourceAmount(ResourceData data, float amount)
    {
        Data = data;
        Amount = amount;
    }
}
