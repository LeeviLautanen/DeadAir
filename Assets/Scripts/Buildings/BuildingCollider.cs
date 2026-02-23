using UnityEngine;

public class BuildingCollider : MonoBehaviour
{
    public BuildingColliderType Type;

    private Building owner;

    private void Awake()
    {
        owner = GetComponentInParent<Building>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        owner.ColliderEnter(Type, other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        owner.ColliderExit(Type, other);
    }
}
