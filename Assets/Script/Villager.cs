using UnityEngine;

public class Villager : MonoBehaviour
{
    public float moveSpeed = 3f;
    private int direction = -1;
    private Rigidbody2D body;
    private AmbientGround ground;
    private Vector2 lastSafePosition;
    private Vector3 originalScale;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        if (body != null && TryGetComponent<Collider2D>(out var shape))
        {
            ground = new AmbientGround(shape);
            lastSafePosition = ground.FindHome(body.position);
            body.position = lastSafePosition;
        }
    }

    private void FixedUpdate()
    {
        if (body == null)
            return;

        float velocity = moveSpeed * direction;
        if (ground != null)
        {
            Vector2 position = body.position;
            if (!ground.CanStep(position, position))
            {
                body.position = lastSafePosition;
                body.linearVelocity = Vector2.zero;
                Turn();
                return;
            }

            lastSafePosition = position;
            Vector2 next = position + Vector2.right * (velocity * Time.fixedDeltaTime);
            if (!ground.CanStep(position, next))
            {
                body.linearVelocityX = 0f;
                Turn();
                return;
            }
        }

        body.linearVelocityX = velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Invert"))
            Turn();
    }

    private void Turn()
    {
        direction *= -1;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(originalScale.x) * direction;
        transform.localScale = scale;
    }
}

