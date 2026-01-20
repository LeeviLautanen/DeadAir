using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData Data => buildingData;

    private BuildingData buildingData;
    private bool isInitialized = false;

    internal void Initialize(BuildingData data)
    {
        if (isInitialized)
        {
            Debug.LogWarning("Building already initialized!");
            return;
        }

        buildingData = data;
        isInitialized = true;
    }

    void Start()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Building was not initialized with data!");
        }
    }

    void OnDestroy()
    {
        // Building is being destroyed
    }

    public virtual void DestroyBuilding()
    {
        Destroy(gameObject);
    }
}
