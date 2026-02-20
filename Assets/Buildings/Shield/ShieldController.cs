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
    private float shieldHealth = 0f;

    private void Start()
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

    private void Update()
    {
        if (CurrentState == BuildingState.Operational) Repair(RecoverSpeed * Time.deltaTime);

        if (CurrentState == BuildingState.Operational && !shieldIsActive && shieldHealth > 50f)
        {
            ActivateShield();
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

    protected override void EnterState(BuildingState state)
    {
        base.EnterState(state);

        switch (state)
        {
            case BuildingState.PendingReservation:
                break;

            case BuildingState.Operational:
                break;

            case BuildingState.OutOfResources:
                DeactivateShield();
                break;

            case BuildingState.Inactive:
                DeactivateShield();
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
