using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ShieldController : Building
{
    public Sprite shieldOnTexture;
    public Sprite shieldOffTexture;
    public float RecoverSpeed = 0.5f;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D shieldCollider;
    private bool shieldIsActive => shieldCollider.enabled;
    private float shieldHealth = 100f;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        shieldCollider = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Meteorite meteorite))
        {
            if (meteorite == null)
            {
                Debug.LogError("Shield collided with an object that doesnt have the meteorite script");
                return;
            }

            if (!meteorite.HasCollided)
            {
                Damage(meteorite.Damage);
                meteorite.HasCollided = true;
            }
        }
    }

    public void ActivateShield()
    {
        shieldCollider.enabled = true;
        spriteRenderer.sprite = shieldOnTexture;
    }

    public void DeactivateShield()
    {
        shieldCollider.enabled = false;
        spriteRenderer.sprite = shieldOffTexture;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        switch (currentState)
        {
            case BuildingState.Inactive:
                // Do nothing until activated
                break;

            case BuildingState.PendingResources:
                break;

            case BuildingState.Operational:
                Repair(RecoverSpeed * Time.deltaTime);

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

    private void Damage(float damage)
    {
        shieldHealth = Mathf.Max(shieldHealth - damage, 0);

        if (shieldHealth <= 0)
        {
            DeactivateShield();
        }
    }

    private void Repair(float amount)
    {
        shieldHealth = Mathf.Min(shieldHealth + amount, 100f);
    }
}
