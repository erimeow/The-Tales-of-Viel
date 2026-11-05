
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletSpeed = 10f;
    public float damage = 20f;
    public float lifetime = 3f;

    private Rigidbody2D rb;
    private float direction = 1f;
    private bool hasHit = false;

    public void SetDirection(float newDirection)
    {
        direction = newDirection >= 0f ? 1f : -1f;

        // Flip the arrow when shooting left
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Bullet needs a Rigidbody2D!", this);
            Destroy(gameObject);
            return;
        }

        rb.linearVelocity = new Vector2(
            direction * bulletSpeed,
            0f
        );

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        if (other.TryGetComponent<Animal>(out _))
            HitEnemy(other);
        else if (other.TryGetComponent<TownPatrol>(out _))
            HitVillager();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit)
            return;

        if (collision.gameObject.CompareTag("Enemy"))
            HitEnemy(collision.collider);
        else if (collision.gameObject.CompareTag("Villager"))
            HitVillager();
        else if (collision.gameObject.CompareTag("Barrier"))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }

    private void HitEnemy(Collider2D target)
    {
        hasHit = true;
        if (CombatTarget.TryDamage(target, damage))
            Debug.Log("Bullet hit enemy! Damage: " + damage);
        Destroy(gameObject);
    }

    private void HitVillager()
    {
        hasHit = true;
        var archer = PlayerWeaponLoadout.Active?.GetComponent<PlayerMovementArcher>() ??
            FindFirstObjectByType<PlayerMovementArcher>();
        if (archer != null)
            archer.TakeDamage(archer.villagerDamageToPlayer);
        Destroy(gameObject);
    }
}
