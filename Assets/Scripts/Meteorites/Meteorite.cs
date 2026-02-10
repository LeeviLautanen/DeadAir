using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 5f;
    public float Speed = 10f;
    public bool HasCollided = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        transform.position += -1f * Speed * Time.deltaTime * transform.up;
    }
}
