using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public float Damage = 50f;
    public float Speed = 30f;
    public float Lifetime = 10f;
    public bool HasCollided = false;

    private static readonly Logger log = new(nameof(Meteorite));
    private MeteoriteParticleSystem meteoriteParticleSystem;
    private TimeManager timeManager;
    private Rigidbody2D rb;
    private float lifeTimer = 0f;
    private Vector2 moveDirection;

    private void Start()
    {
        meteoriteParticleSystem = MeteoriteParticleSystem.Instance;
        timeManager = TimeManager.Instance;
        rb = GetComponent<Rigidbody2D>();
        moveDirection = -rb.transform.up;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Ignore collisions between meteorites
        if (collider.CompareTag("Meteorite"))
        {
            return;
        }

        // Trigger particles only once
        if (HasCollided)
        {
            return;
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                meteoriteParticleSystem.Play(new(transform.position.x, -1f), particleAngle);
            }
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Damage"))
        {
            if (meteoriteParticleSystem != null)
            {
                Vector2 particleAngle = moveDirection;
                particleAngle.y = Mathf.Abs(particleAngle.y);
                var collisionPoint = collider.ClosestPoint(transform.position);
                var collisionNormal = ((Vector2)transform.position - collisionPoint).normalized;

                collisionPoint -= collisionNormal * 0.5f; // Move slightly towards the building

                meteoriteParticleSystem.Play(collisionPoint, collisionNormal);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Speed * Time.fixedDeltaTime * timeManager.GameTimeMultiplier * moveDirection);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
