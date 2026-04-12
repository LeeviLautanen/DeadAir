using UnityEngine;

public class BuildingCollider : MonoBehaviour
{
    public BuildingColliderType Type;

    private static readonly Logger log = new(nameof(BuildingCollider));
    private Building owner;

    private void Awake()
    {
        owner = GetComponentInParent<Building>();

        switch (Type)
        {
            case BuildingColliderType.Placement:
                gameObject.layer = LayerMask.NameToLayer("BuildingPlacement");
                break;
            case BuildingColliderType.Damage:
                gameObject.layer = LayerMask.NameToLayer("BuildingDamage");
                break;
            case BuildingColliderType.Shield:
                gameObject.layer = LayerMask.NameToLayer("Shield");
                break;
            default:
                log.Warning($"Unknown BuildingColliderType: {Type}");
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == null)
        {
            return;
        }

        owner.ColliderEnter(Type, other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (owner == null)
        {
            return;
        }

        owner.ColliderExit(Type, other);
    }
}
