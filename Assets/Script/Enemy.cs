using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float health = 100f;
    int direction = -1;
    public float damage = 10f;
    [SerializeField] private Coin coinDrop;
    private bool isDead;
    private float maxHealth;
    private TargetHealthHud healthHud;

    Rigidbody2D rb;
    Animator anim;

    bool isAttacking = false;

    //oop script ni player and variable name
    LanceMovement player;
    PlayerMovement2 player1;


    void Start()
    {
        maxHealth = health;
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
        if (isDead)
            return;

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
        if (isDead)
            return;

        isAttacking = false;

        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsRunning", true);

    }
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (maxHealth <= 0f)
            maxHealth = health;
        health = Mathf.Max(0, health - damage);
        if (health > 0 && health < maxHealth && healthHud == null)
            healthHud = TargetHealthHud.Create(transform, 1.05f, null);
        if (healthHud != null)
            healthHud.SetHealth(health, maxHealth);

        Debug.Log("Skeleton Health: " + health);

        if (health <= 0)
        {
            isDead = true;
            gameManager.instance?.RegisterEnemyDefeat();
            if (coinDrop != null)
                Instantiate(coinDrop, transform.position, Quaternion.identity).PlayDropEffect();

            isAttacking = true;
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsAttacking", false);
            DeathPresentation.Play(anim, "Warrior die", "Skeleton_Dying");

            Destroy(gameObject, 1f);

        }
    }

}