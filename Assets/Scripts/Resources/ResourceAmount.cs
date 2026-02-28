[System.Serializable]
public class ResourceAmount
{
    public ResourceData Data;
    public float Amount;
    public ResourceAmount(ResourceData data, float amount)
    {
        Data = data;
        Amount = amount;
    }
}
