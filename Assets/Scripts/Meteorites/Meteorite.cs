using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 50f;
    public float Speed = 30f;
    public bool HasCollided = false;
    public float Lifetime = 10f;

    private static readonly Logger log = new(true, LogLevel.Warning);
    private Rigidbody2D rb;
    private float lifeTimer = 0f;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveDirection = rb.transform.up;
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
        rb.MovePosition(rb.position - Speed * Time.fixedDeltaTime * moveDirection);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
