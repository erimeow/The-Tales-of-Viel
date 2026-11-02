using UnityEngine;



public class Villager : MonoBehaviour
{
    public float moveSpeed = 3f;
    int direction = -1;

    Rigidbody2D rb;
    Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = moveSpeed * direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Invert"))
        {
            direction *= -1;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(originalScale.x) * direction;
            transform.localScale = scale;
        }
    }
}




