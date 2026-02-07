using System;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData Data => buildingData;
    public static event Action<GameObject> OnBuildingDestroyed;

    private BuildingData buildingData;
    private bool isInitialized = false;
    private float currentHealth;

    internal void Initialize(BuildingData data)
    {
        if (isInitialized)
        {
            Debug.LogWarning("Building already initialized!");
            return;
        }

        buildingData = data;
        currentHealth = buildingData.MaxHealth;
        isInitialized = true;
    }

    private void Start()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Building was not initialized with data!");
        }
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

    private void Damage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    private void Repair(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, buildingData.MaxHealth);
    }

    public virtual void DestroyBuilding()
    {
        OnBuildingDestroyed?.Invoke(gameObject);
    }
}
