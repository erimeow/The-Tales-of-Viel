using UnityEngine;

public class PlayerMovement4 : MonoBehaviour
{
    public float moveSpeed;
    float horizontal,vertical;

    Rigidbody2D rb;
    Animator anim;
    void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        anim= GetComponent<Animator>();
    }

    
    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal < 0) transform.localScale = new Vector2(-1, 1);
        else if (horizontal > 0) transform.localScale = new Vector2(1, 1);

        if (horizontal != 0 || vertical != 0)
            anim.SetBool("IsRunning", true);
        else
            anim.SetBool("IsRunning", false);

        if (Input.GetMouseButtonDown(0))
        {
            Attack1();
        }
    }
    private void FixedUpdate()
    {
        rb.linearVelocityX = horizontal * moveSpeed;
        rb.linearVelocityY = vertical * moveSpeed;
    }
    void Attack1()
    {
        anim.SetTrigger("Attack");
    }
}
