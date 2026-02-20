public interface IResourceUser
{
    public int Priority { get; }
    public BuildingState CurrentState { get; }
    public void Activate();
    public void Deactivate();
    public void OnOutOfResources();
    public void OnResourcesRecovered();
}
