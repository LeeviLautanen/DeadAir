using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MeteoriteManager : MonoBehaviour
{
    public GameObject MeteoritePrefab;
    private void Start()
    {

    }

    void FixedUpdate()
    {
        spawnMeteorite();
    }

    private void spawnMeteorite()
    {
        Vector3 spawnPosition = new(0, 20, -1);
        float angle = Random.Range(-30f, 30f);

        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;

        Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);
    }
}
