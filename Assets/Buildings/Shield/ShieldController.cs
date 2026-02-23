using System.Collections.Generic;
using UnityEngine;

public class ShieldController : Building
{
    public Sprite shieldOnTexture;
    public Sprite shieldOffTexture;
    public float RecoverSpeed = 10f;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D shieldCollider;
    private bool shieldIsActive => shieldCollider.enabled;
    private float shieldHealth = 100f;

    private void Awake()
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
            Debug.LogError("No shield collider found on shield building");
        }
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
        if (shieldIsActive || shieldCollider == null) return;

        log.Info("Activating shield");
        shieldCollider.enabled = true;
        spriteRenderer.sprite = shieldOnTexture;
    }

    public void DeactivateShield()
    {
        if (!shieldIsActive || shieldCollider == null) return;

        log.Info("Deactivating shield");
        shieldCollider.enabled = false;
        spriteRenderer.sprite = shieldOffTexture;
    }

    public override void UpdateState(float deltaTime)
    {
        base.UpdateState(deltaTime);
        switch (currentState)
        {
            case BuildingState.Inactive:
                // Do nothing until activated
                break;

            case BuildingState.PendingResources:
                break;

            case BuildingState.Operational:
                RepairShield(RecoverSpeed * Time.deltaTime);

                if (!shieldIsActive && shieldHealth > 50f)
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

            case BuildingState.Operational:
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
