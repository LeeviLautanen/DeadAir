using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 5f;
    public float Speed = 10f;
    public bool HasCollided = false;
    public float Lifetime = 5f;

    private static readonly Logger log = new(true, LogLevel.Warning);
    private Rigidbody2D rb;
    private float lifeTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Meteorite") &&
            (collision.gameObject.layer == LayerMask.NameToLayer("Damage") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position - Speed * Time.fixedDeltaTime * (Vector2)rb.transform.up);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
