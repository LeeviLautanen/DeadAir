using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 5f;
    public float Speed = 10f;
    public bool HasCollided = false;
    public float Lifetime = 5f;

    private float lifeTimer = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Meteorite")) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        transform.position += -1f * Speed * Time.deltaTime * transform.up;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
