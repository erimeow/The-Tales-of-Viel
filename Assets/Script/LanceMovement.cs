using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;

public class LanceMovement : MonoBehaviour
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
    private readonly System.Collections.Generic.HashSet<Component> hitTargets = new();

   
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

        health = gameManager.instance?.TakeCarriedHealth(gameObject.scene.buildIndex, maxHealth) ?? maxHealth;
        PlayerHealthHud.Attach(healthBar);

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

    }

    public bool IsDead => isDead;

    internal bool TryHeal(float amount)
    {
        if (isDead || health <= 0f || health >= maxHealth || amount <= 0f)
            return false;

        health = Mathf.Min(maxHealth, health + amount);
        if (healthBar != null)
            healthBar.value = health;
        return true;
    }

    void Update()
    {
        if (isDead)
            return;

        if (PlayerWeaponLoadout.BlocksInput(rb))
        {
            horizontal = vertical = 0;
            anim?.SetBool("IsRuinning", false);
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (horizontal < 0)
            transform.localScale = new Vector2(-1, 1);
        else if (horizontal > 0)
            transform.localScale = new Vector2(1, 1);

        if (horizontal != 0 || vertical != 0)
            anim.SetBool("IsRuinning", true);
        else
            anim.SetBool("IsRuinning", false);

        if (Input.GetMouseButtonDown(1))
        {
            Attack1();
        }

        //if (Input.GetMouseButtonDown(1))
        //{
        //    Attack2();
        //}

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    anim.SetBool("IsGuarding", true);
        //}
        //else
        //{
        //    anim.SetBool("IsGuarding", false);
        //}
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (PlayerWeaponLoadout.BlocksInput(rb))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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

        hitTargets.Clear();
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                CombatTarget.TryDamage(enemy, attackDamage + (gameManager.instance?.WeaponBonus(CharacterRole.Spear) ?? 0), hitTargets);
            }

            //attackDamage health when attack villager
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
        if (collision.CompareTag("Barrier"))
            Die();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = health;

        Debug.Log("Player Health: " + health);

        if (health <= 0f)
            Die();
    }
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        health = 0;
        if (healthBar != null)
            healthBar.value = 0;
        DeathPresentation.Player(this, rb, anim, "Player_Die", gameOverPanel, deathSFX);
    }
}
