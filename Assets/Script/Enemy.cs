using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float health = 100f;
    int direction = -1;
    public float damage = 10f;

    Rigidbody2D rb;
    Animator anim;

    bool isAttacking = false;

    //oop script ni player and variable name
    LanceMovement player;
    PlayerMovement2 player1;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocityX = 0;
        }
        else
        {
            rb.linearVelocityX = moveSpeed * direction;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //kinuha ang variable and component ni player
            player = collision.gameObject.GetComponent<LanceMovement>();
            player1 = collision.gameObject.GetComponent<PlayerMovement2>();
            Attack();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StopAttack();
            player = null;
            player1 = null;
        }
    }

    void Attack()
    {
        isAttacking = true;

        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", true);

    }
    public void DealDamage()
    {
        if (player != null)
        {
            player.TakeDamage(damage);

        }
        if (player1 != null)
        {
            player1.TakeDamage(damage);
        }
    }
    void StopAttack()
    {
        isAttacking = false;

        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsRunning", true);

    }
    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log("Skeleton Health: " + health);

        if (health <= 0)
        {
            isAttacking = true;
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsDying", true);

            Destroy(gameObject, 1f);

        }
    }

}