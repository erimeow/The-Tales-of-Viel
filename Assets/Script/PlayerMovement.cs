using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    float horizontal;
    float vertical;

    float lastHorizontal;
    float lastVertical;

    Rigidbody2D rb;
    Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0)
            vertical = 0;

        if (vertical != 0)
            horizontal = 0;

        // Remember last direction
        if (horizontal != 0)
        {
            lastHorizontal = horizontal;
            lastVertical = 0;
        }

        if (vertical != 0)
        {
            lastHorizontal = 0;
            lastVertical = vertical;
        }

        anim.SetFloat("Horizontal", horizontal);
        anim.SetFloat("Vertical", vertical);

        // Left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        anim.SetFloat("AttackHorizontal", lastHorizontal);
        anim.SetFloat("AttackVertical", lastVertical);

        anim.SetTrigger("Attack");
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = horizontal * moveSpeed;
        rb.linearVelocityY = vertical * moveSpeed;
    }
}