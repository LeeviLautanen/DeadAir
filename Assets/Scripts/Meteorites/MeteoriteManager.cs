using UnityEngine;
using System.Collections;

public class MeteoriteManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(0.5f);
    public GameObject MeteoritePrefab;

    private void Start()
    {
        StartCoroutine(SpawnMeteorites());
    }

    IEnumerator SpawnMeteorites()
    {
        while (true)
        {
            SpawnMeteorite();
            yield return _waitForSeconds1;
        }
    }

    private void SpawnMeteorite()
    {
        Vector3 spawnPosition = new(0, 20, -1);
        float angle = Random.Range(-30f, 30f);

        Quaternion spawnRotation = Quaternion.AngleAxis(angle, Vector3.forward) * MeteoritePrefab.transform.rotation;

        Instantiate(MeteoritePrefab, spawnPosition, spawnRotation);
    }
}
