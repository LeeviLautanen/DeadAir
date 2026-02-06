using System.Collections.Generic;
using UnityEngine;

public class ShieldController : Building
{
    public Sprite shieldOnTexture;
    public Sprite shieldOffTexture;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        // Building is being destroyed
    }

    public void ActivateShield()
    {
        spriteRenderer.sprite = shieldOnTexture;
    }

    public void DeactivateShield()
    {
        spriteRenderer.sprite = shieldOffTexture;
    }

}
