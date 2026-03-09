using UnityEngine;

public class BuildingCollider : MonoBehaviour
{
    public BuildingColliderType Type;

    private static readonly Logger log = new(true, LogLevel.Warning);
    private Building owner;

    private void Awake()
    {
        owner = GetComponentInParent<Building>();

        switch (Type)
        {
            case BuildingColliderType.Placement:
                gameObject.layer = LayerMask.NameToLayer("Placement");
                break;
            case BuildingColliderType.Damage:
                gameObject.layer = LayerMask.NameToLayer("Damage");
                break;
            case BuildingColliderType.Shield:
                gameObject.layer = LayerMask.NameToLayer("Damage");
                break;
            default:
                log.Warning($"Unknown BuildingColliderType: {Type}");
                break;
        }
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
