
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public float health = 100f;
    public float damage = 10f;

    [Header("Chasing Settings")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 0.6f;

    [Header("Obstacle Debug")]
    public bool showObstacleLogs = true;

    [Header("Death Settings")]
    public float deathDestroyDelay = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D skeletonCollider;

    private ContactFilter2D obstacleFilter;

    private readonly RaycastHit2D[] castResults =
        new RaycastHit2D[8];

    private bool isAttacking;
    private bool isDead;

    private Transform targetPlayer;

    private LanceMovement player;
    private PlayerMovement2 player1;
    private PlayerMovementArcher player3;

    private Vector2 lastMoveDirection;

    public AudioClip deathSFX;

    //healthBehaviour

    //public float Hitpoints;
    //public float MaxHitpoints;
    //public HealthbarBehaviour Healthbar;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        skeletonCollider = GetComponent<Collider2D>();

        //Hitpoints = MaxHitpoints;
        //Healthbar.SetHealth(Hitpoints, MaxHitpoints);

        obstacleFilter = new ContactFilter2D();
        obstacleFilter.SetLayerMask(obstacleLayer);
        obstacleFilter.useTriggers = false;

        if (rb == null)
            Debug.LogError("Skeleton: Rigidbody2D is missing!", this);

        if (anim == null)
            Debug.LogError("Skeleton: Animator is missing!", this);

        if (skeletonCollider == null)
        {
            Debug.LogError(
                "Skeleton: Add a Collider2D to the same GameObject!",
                this);
        }

        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            targetPlayer = playerObject.transform;
    }

    void FixedUpdate()
    {
        if (isDead || rb == null)
            return;

        if (targetPlayer == null)
            FindPlayer();

        if (isAttacking || targetPlayer == null)
        {
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
                anim.SetBool("IsRunning", false);

            return;
        }

        Vector2 difference =
            (Vector2)targetPlayer.position - rb.position;

        if (difference.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
                anim.SetBool("IsRunning", false);

            return;
        }

        Vector2 direction = FindChaseDirection(difference);

        if (direction == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;

            if (anim != null)
                anim.SetBool("IsRunning", false);

            return;
        }

        rb.linearVelocity = direction * moveSpeed;

        if (anim != null)
            anim.SetBool("IsRunning", true);

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            transform.localScale = scale;
        }
    }

    Vector2 FindChaseDirection(Vector2 difference)
    {
        if (skeletonCollider == null)
            return difference.normalized;

        Vector2 targetDirection = difference.normalized;

        float x = Mathf.Sign(difference.x);
        float y = Mathf.Sign(difference.y);

        Vector2[] directions =
        {
            targetDirection,
            new Vector2(x, 0f),
            new Vector2(0f, y),
            new Vector2(x, -y).normalized,
            new Vector2(-x, y).normalized,
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        Vector2 bestDirection = Vector2.zero;
        float bestScore = float.NegativeInfinity;

        foreach (Vector2 candidateDirection in directions)
        {
            if (candidateDirection.sqrMagnitude < 0.01f)
                continue;

            Vector2 candidate = candidateDirection.normalized;

            int hitCount = skeletonCollider.Cast(
                candidate,
                obstacleFilter,
                castResults,
                obstacleCheckDistance
            );

            if (hitCount > 0)
            {
                if (showObstacleLogs &&
                    castResults[0].collider != null)
                {
                    Debug.Log(
                        "Skeleton detected obstacle: " +
                        castResults[0].collider.name +
                        " | Direction: " + candidate,
                        this
                    );
                }

                continue;
            }

            float score = Vector2.Dot(
                candidate,
                targetDirection
            );

            if (lastMoveDirection != Vector2.zero &&
                Vector2.Dot(candidate, lastMoveDirection) > 0.8f)
            {
                score += 0.15f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = candidate;
            }
        }

        if (bestDirection != Vector2.zero)
            lastMoveDirection = bestDirection;

        return bestDirection;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead ||
            !collision.gameObject.CompareTag("Player"))
            return;

        targetPlayer = collision.transform;

        player =
            collision.gameObject.GetComponent<LanceMovement>();

        player1 =
            collision.gameObject.GetComponent<PlayerMovement2>();

        player3 =
            collision.gameObject.GetComponent<PlayerMovementArcher>();

        if (player != null || player1 != null || player3 != null)
        {
            Attack();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        StopAttack();

        player = null;
        player1 = null;
        player3 = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Skeleton trigger detected: " + collision.name);

        if (isDead)
            return;

        if (collision.CompareTag("Barrier"))
        {
            Debug.Log("Barrier detected! Destroying: " + collision.name);

            Die();
        }
    }

    void Attack()
    {
        if (isDead || anim == null || rb == null)
            return;

        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", true);
    }

    public void DealDamage()
    {
        if (isDead)
            return;

        if (player != null)
            player.TakeDamage(damage);
        else if (player1 != null)
            player1.TakeDamage(damage);
        else if (player3 != null)
            player3.TakeDamage(damage);
    }

    void StopAttack()
    {
        if (isDead)
            return;

        isAttacking = false;

        if (anim != null)
            anim.SetBool("IsAttacking", false);
    }

    public void TakeDamage(float damageAmount)
    {
        

        if (isDead)
            return;

        health -= damageAmount;
        //Healthbar.SetHealth(Hitpoints,MaxHitpoints)

        Debug.Log("Skeleton Health: " + health);

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        isAttacking = false;

        targetPlayer = null;
        player = null;
        player1 = null;
        player3 = null;

        // Stop the skeleton's movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Play the death animation
        if (anim != null)
        {
            anim.SetBool("IsRunning", false);
            anim.SetBool("IsAttacking", false);
            anim.SetBool("IsDying", true);

            anim.Update(0f);
        }

        Debug.Log("Skeleton death animation started.", this);

        //sound
        gameManager.instance.playSFX(deathSFX, 1, transform);

        Destroy(gameObject, deathDestroyDelay);
    }
    public void FinishDeath()
    {
        if (!isDead)
            return;

        Destroy(gameObject);
    }
}
