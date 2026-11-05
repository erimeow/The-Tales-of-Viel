
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovementArcher : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;

    private float horizontal;
    private float vertical;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Health Settings")]
    public float health = 100f;
    public float maxHealth = 100f;
    public Slider healthBar;
    public GameObject gameOverPanel;

    [Header("Shooting Settings")]
    public GameObject bullet;
    public float fireRate = 0.5f;
    public float bulletSpawnOffset = 0.6f;

    private bool isReadyToShoot = true;
    private bool isDead = false;
    private float facingDirection = 1f;


    //hit villager
    [Header("Villager Damage")]
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

        // Initialize health bar
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
        else
        {
            Debug.LogWarning("Archer health bar is not assigned!", this);
        }

        // Hide game-over panel at the start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (rb == null)
        {
            Debug.LogError("Archer needs a Rigidbody2D!", this);
        }

        if (anim == null)
        {
            Debug.LogError("Archer needs an Animator!", this);
        }

        if (bullet == null)
        {
            Debug.LogError(
                "Bullet prefab is not assigned in the Inspector!", this);
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
        if (isDead) return;

        // Player movement input
        if (PlayerWeaponLoadout.BlocksInput(rb))
        {
            horizontal = vertical = 0;
            anim?.SetBool("IsRunning", false);
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Update facing direction
        if (horizontal > 0)
        {
            facingDirection = 1f;
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (horizontal < 0)
        {
            facingDirection = -1f;
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Running animation
        if (anim != null)
        {
            bool isRunning = horizontal != 0 || vertical != 0;
            anim.SetBool("IsRunning", isRunning);
        }

        // Shoot using the right mouse button
        if (Input.GetMouseButtonDown(1) && isReadyToShoot)
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (rb == null || isDead) return;

        if (PlayerWeaponLoadout.BlocksInput(rb))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Unity 6 Rigidbody2D movement
        rb.linearVelocity = new Vector2(
            horizontal * moveSpeed,
            vertical * moveSpeed
        );
    }

    void Shoot()
    {
        // Prevent shooting if the prefab is missing
        if (bullet == null)
        {
            Debug.LogError(
                "Cannot shoot: assign the Bullet prefab in the Inspector!",
                this
            );
            return;
        }

        // Calculate bullet spawn position
        Vector3 spawnPosition = transform.position +
            new Vector3(
                facingDirection * bulletSpawnOffset,
                0f,
                0f
            );

        // Create bullet
        GameObject newBullet = Instantiate(
            bullet,
            spawnPosition,
            Quaternion.identity
        );

        // Set bullet direction
        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(facingDirection);
            gameManager wallet = gameManager.instance;
            if (wallet != null)
            {
                int tier = wallet.EquippedWeapon(CharacterRole.Archer);
                bulletScript.damage += wallet.WeaponBonus(CharacterRole.Archer);
                if (tier > 0 && newBullet.TryGetComponent<SpriteRenderer>(out var arrowRenderer))
                {
                    Sprite arrow = Resources.Load<Sprite>(WeaponShopCatalog.ArtPath(CharacterRole.Archer, tier, "Arrow"));
                    if (arrow != null)
                        arrowRenderer.sprite = arrow;
                }
            }
        }
        else
        {
            Debug.LogWarning(
                "Bullet prefab is missing the Bullet script!", newBullet
            );
        }



        // Play attack animation
        Attack1();

        // Start shooting cooldown
        isReadyToShoot = false;
        Invoke(nameof(ReloadGun), Mathf.Max(0.01f, fireRate));
    }

    void ReloadGun()
    {
        if (!isDead)
        {
            isReadyToShoot = true;
        }
    }

    void Attack1()
    {
        if (anim != null)
        {
            //sound
            gameManager.instance.playSFX(attackSFX, 1, transform);

            anim.SetTrigger("Attack1");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = health;
        }

        Debug.Log("Archer Health: " + health);

        if (health <= 0f)
            Die();
    }

   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier"))
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
        CancelInvoke(nameof(ReloadGun));
        DeathPresentation.Player(this, rb, anim, "Archer_die", gameOverPanel, deathSFX);
    }
}
