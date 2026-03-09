using UnityEngine;

public class ShieldController : Building
{
    public Sprite shieldOnTexture;
    public Sprite shieldOffTexture;
    public float RecoverSpeed = 10f;

    private static readonly new Logger log = new(true, LogLevel.Warning);
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D shieldCollider;
    private bool isShieldActive;
    private float shieldHealth = 100f;

    protected override void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        foreach (var collider in GetComponentsInChildren<BuildingCollider>())
        {
            if (collider.Type == BuildingColliderType.Shield)
            {
                collider.gameObject.TryGetComponent(out shieldCollider);
                break;
            }
        }
        if (shieldCollider == null)
        {
            log.Error("No shield collider found on shield building");
        }

        base.Start();
    }

    public override void ColliderEnter(BuildingColliderType colliderType, Collider2D other)
    {
        base.ColliderEnter(colliderType, other);
        switch (colliderType)
        {
            case BuildingColliderType.Shield:
                if (other.gameObject.TryGetComponent(out Meteorite meteorite))
                {
                    if (meteorite == null)
                    {
                        Debug.LogError("Shield collided with an object that doesnt have the meteorite script");
                        return;
                    }
                    if (meteorite.HasCollided) return; // Prevent multiple collisions from the same meteorite
                    meteorite.HasCollided = true;
                    DamageShield(meteorite.Damage);
                }
                break;
        }
    }

    public void ActivateShield()
    {
        if (isShieldActive || shieldCollider == null) return;

        log.Info("Activating shield");
        isShieldActive = true;
        shieldCollider.enabled = true;
        spriteRenderer.sprite = shieldOnTexture;
    }

    public void DeactivateShield()
    {
        if (!isShieldActive || shieldCollider == null) return;

        log.Info("Deactivating shield");
        isShieldActive = false;
        shieldCollider.enabled = false;
        spriteRenderer.sprite = shieldOffTexture;
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);
        switch (currentState)
        {
            case BuildingState.Operational:
                RepairShield(RecoverSpeed * Time.deltaTime);

                if (!isShieldActive && shieldHealth > 50f)
                {
                    ActivateShield();
                }
                break;
        }
    }

    protected override void EnterState(BuildingState state)
    {
        base.EnterState(state);

        switch (state)
        {
            case BuildingState.Inactive:
                DeactivateShield();
                break;

            case BuildingState.PendingResources:
                DeactivateShield();
                break;
        }
    }

    private void DamageShield(float damage)
    {
        shieldHealth = Mathf.Max(shieldHealth - damage, 0);
        if (shieldHealth <= 0)
        {
            DeactivateShield();
        }
    }

    private void RepairShield(float amount)
    {
        shieldHealth = Mathf.Min(shieldHealth + amount, 100f);
    }
}
