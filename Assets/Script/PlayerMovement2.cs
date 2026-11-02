using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;

public class PlayerMovement2 : MonoBehaviour
{
    public float moveSpeed;
    public float health = 100f;
    public float maxHealth = 100f;
    public Slider healthBar;

    Rigidbody2D rb;
    Animator anim;

    float horizontal, vertical;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackDamage = 20f;

    public float gameOverDelay;
    bool isDead = false;

    public GameObject gameOverPanel;

    //attackDamage health when attack villager
    public float villagerDamageToPlayer = 10f;

    //sound
    public AudioClip attackSFX;
    public AudioClip deathSFX;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        health = maxHealth;

        healthBar.maxValue = maxHealth;
        healthBar.value = health;

    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (horizontal < 0)
            transform.localScale = new Vector2(-1, 1);
        else if (horizontal > 0)
            transform.localScale = new Vector2(1, 1);

        if (horizontal != 0 || vertical != 0)
            anim.SetBool("IsRunning", true);
        else
            anim.SetBool("IsRunning", false);

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Attack1();
        //}

        if (Input.GetMouseButtonDown(1))
        {
            Attack1();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            anim.SetBool("IsGuarding", true);
        }
        else
        {
            anim.SetBool("IsGuarding", false);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocityX = horizontal * moveSpeed;
        rb.linearVelocityY = vertical * moveSpeed;
    }

    void Attack1()
    {
        //sound
        gameManager.instance.playSFX(attackSFX, 1, transform);

        anim.SetTrigger("Attack1");
        Attack();
    }

    void Attack2()
    {
        anim.SetTrigger("Attack2");
        Attack();
    }

    void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange
        );

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Skeleton skeleton = enemy.GetComponent<Skeleton>();

                if (skeleton != null)
                {
                    //this is from global variable which is naka public
                    skeleton.TakeDamage(attackDamage);
                    
                }
            }

            if (enemy.CompareTag("Villager"))
            {
                // Player loses health when attacking a Villager
                TakeDamage(villagerDamageToPlayer);

                Debug.Log("Lance attacked a Villager! Player lost health.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Barrier"))
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("IsDying", true);


            gameManager.instance.playSFX(deathSFX, 1, transform);
            Destroy(gameObject, 2f);
            gameOverPanel.SetActive(true);



            if (gameManager.instance != null)
            {
                gameManager.instance.ResetGame();
            }


        }
    }



    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        healthBar.value = health;

        Debug.Log("Player Health: " + health);

        if (health <= 0)
        {
            isDead = true;

            rb.linearVelocity = Vector2.zero;


            anim.SetBool("IsDying", true);
           




            Invoke(nameof(ShowGameOver), gameOverDelay);

            Destroy(gameObject, gameOverDelay);
            //sound
            gameManager.instance.playSFX(deathSFX, 1, transform);

            if (gameManager.instance != null)
            {
                gameManager.instance.ResetGame();
            }
        }
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

        }
        else
        {
            Debug.LogWarning("Game Over Panel is not assigned!");
        }
    }




}
