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
    private float shieldHealth = 100f;

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
            Damage(meteorite.Damage);
        }
    }

    private void Update()
    {
        Repair(RecoverSpeed * Time.deltaTime);

        if (shieldHealth > 50f && spriteRenderer.sprite == shieldOffTexture)
        {
            Activate();
        }
    }

    public void Activate()
    {
        shieldCollider.enabled = true;
        spriteRenderer.sprite = shieldOnTexture;
    }

    public void Deactivate()
    {
        shieldCollider.enabled = false;
        spriteRenderer.sprite = shieldOffTexture;
    }

    private void Damage(float damage)
    {
        shieldHealth = Mathf.Max(shieldHealth - damage, 0);

        if (shieldHealth <= 0)
        {
            Deactivate();
        }
    }

    private void Repair(float amount)
    {
        shieldHealth = Mathf.Min(shieldHealth + amount, 100f);
    }
}
