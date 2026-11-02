
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement3 : MonoBehaviour
{
    public float moveSpeed;

    float horizontal, vertical;

    Rigidbody2D rb;
    Animator anim;

    float lastHorizontal = 1;
    float lastVertical = 0;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (horizontal < 0) transform.localScale = new Vector2(-1, 1);
        else if (horizontal > 0) transform.localScale = new Vector2(1, 1);

        if (horizontal != 0)
        {
            lastHorizontal = horizontal;
            
        }

        if (vertical != 0)
        {
            lastVertical = vertical;
            
        }

        if (horizontal != 0 || vertical != 0)
            anim.SetBool("IsRunning", true);
        else
            anim.SetBool("IsRunning", false);

        

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

   
    private void FixedUpdate()
    {
        rb.linearVelocityX = horizontal * moveSpeed;
        rb.linearVelocityY = vertical * moveSpeed;
    }
    void Attack()
    {
        anim.SetBool("AttackUp", false);
        anim.SetBool("AttackDown", false);
        anim.SetBool("Attack", false);
        anim.SetBool("UpRightAttack", false);
        anim.SetBool("DownRightAttack", false);

        if (lastVertical > 0)
        {
            anim.SetBool("AttackUp", true);
            
        }
        else if (lastVertical < 0)
        {
            
            anim.SetBool("AttackDown", true);
           
        }
        else if (lastHorizontal != 0)
        {
           
            anim.SetBool("Attack", true);
        }
        // UP-RIGHT / UP-LEFT
        else if (lastVertical > 0 && lastHorizontal != 0)
        {
            anim.SetBool("UpRightAttack", true);

            if (lastHorizontal < 0)
                transform.localScale = new Vector2(-1, 1);
            else
                transform.localScale = new Vector2(1, 1);
        }

        // DOWN-RIGHT / DOWN-LEFT
        else if (lastVertical < 0 && lastHorizontal != 0)
        {
            anim.SetBool("DownRightAttack", true);

            if (lastHorizontal < 0)
                transform.localScale = new Vector2(-1, 1);
            else
                transform.localScale = new Vector2(1, 1);
        }


    }
}
