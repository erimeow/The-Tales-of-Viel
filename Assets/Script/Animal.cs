using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Animal : MonoBehaviour
{
    [SerializeField] private float health = 20f;
    [SerializeField] private float speed = 0.65f;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private meat meatDrop;
    [SerializeField, Min(1)] private int meatAmount = 1;

    private Rigidbody2D body;
    private SpriteRenderer artwork;
    private Vector2 home;
    private Vector2 destination;
    private float nextMove;
    private bool dead;
    private float maxHealth;
    private TargetHealthHud healthHud;
    private AmbientGround ground;

    private void Awake()
    {
        maxHealth = health;
        body = GetComponent<Rigidbody2D>();
        artwork = GetComponent<SpriteRenderer>();
        home = destination = body.position;
        nextMove = Time.time + Random.Range(0.8f, 2.5f);
    }

    private void Start()
    {
        var shape = GetComponent<Collider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        shape.isTrigger = true;
        ground = new AmbientGround(shape);
        home = destination = ground.FindHome(body.position);
        body.position = home;
    }

    private void FixedUpdate()
    {
        if (dead)
            return;

        if (Time.time >= nextMove)
        {
            destination = home + Random.insideUnitCircle * 0.55f;
            nextMove = Time.time + Random.Range(1.3f, 3.2f);
        }

        Vector2 position = body.position;
        Vector2 next = Vector2.MoveTowards(position, destination, speed * Time.fixedDeltaTime);
        bool moving = (destination - position).sqrMagnitude > .015f && ground.CanStep(position, next);
        if (moving)
        {
            body.MovePosition(next);
            if (Mathf.Abs(next.x - position.x) > .001f)
                artwork.flipX = next.x < position.x;
        }
        else
        {
            destination = position;
        }

        if (frames != null && frames.Length > 0)
        {
            float rate = moving ? 8f : 2f;
            artwork.sprite = frames[Mathf.FloorToInt(Time.time * rate) % frames.Length];
        }
    }

    public void TakeDamage(float damage)
    {
        if (dead || damage <= 0f)
            return;

        health = Mathf.Max(0, health - damage);
        if (health > 0 && healthHud == null)
            healthHud = TargetHealthHud.Create(transform, .62f, null);
        if (healthHud != null)
            healthHud.SetHealth(health, maxHealth);
        if (health > 0f)
            return;

        dead = true;
        if (meatDrop != null)
            Instantiate(meatDrop, transform.position, Quaternion.identity).SetDropAmount(meatAmount);
        Destroy(gameObject);
    }
}
