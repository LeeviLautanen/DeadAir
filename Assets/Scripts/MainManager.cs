using UnityEngine;
using System.Collections.Generic;

public class MainManager : MonoBehaviour
{

    private BuildingManager buildingManager;

    private void Start()
    {
        buildingManager = GetComponent<BuildingManager>();

        InvokeRepeating(nameof(Tick), 0f, 1f);
    }

    private void Tick()
    {
        buildingManager.ProcessBuildingResources();
    }
}
