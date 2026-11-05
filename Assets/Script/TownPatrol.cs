using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public sealed class TownPatrol : MonoBehaviour
{
    [SerializeField] private Vector2 routeOffset = new Vector2(3, 0);
    [SerializeField] private float speed = .85f;
    [SerializeField] private int activity;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] workFrames;
    [SerializeField] private Sprite[] gatherFrames;
    [SerializeField] private Sprite[] carryFrames;

    private Rigidbody2D body;
    private SpriteRenderer artwork;
    private AmbientGround ground;
    private Vector2 home;
    private bool outbound = true;
    private float waitUntil;
    private float activityStart;
    private float walkTime;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        var shape = GetComponent<Collider2D>();
        artwork = GetComponent<SpriteRenderer>();
        body.bodyType = RigidbodyType2D.Kinematic;
        shape.isTrigger = true;
        ground = new AmbientGround(shape);
        home = ground.FindHome(body.position);
        body.position = home;
    }

    private void FixedUpdate()
    {
        if (Time.time < waitUntil)
        {
            Sprite[] frames = activity == 1 ? workFrames : activity == 2 ? gatherFrames : idleFrames;
            Animate(frames, Time.time - activityStart, activity == 1 || activity == 2 ? 8 : 4);
            return;
        }

        Vector2 target = home + (outbound ? routeOffset : Vector2.zero);
        Vector2 position = body.position;
        Vector2 next = Vector2.MoveTowards(position, target, speed * Time.fixedDeltaTime);
        if ((target - position).sqrMagnitude < .035f || !ground.CanStep(position, next))
        {
            outbound = !outbound;
            activityStart = Time.time;
            waitUntil = Time.time + (activity == 1 || activity == 2 ? 2.2f : .5f);
            return;
        }

        body.MovePosition(next);
        if (Mathf.Abs(next.x - position.x) > .001f)
            artwork.flipX = next.x < position.x;
        walkTime += Time.fixedDeltaTime;
        Animate(activity == 3 ? carryFrames : walkFrames, walkTime, 8);
    }

    private void Animate(Sprite[] frames, float time, int rate)
    {
        if (frames != null && frames.Length > 0)
            artwork.sprite = frames[Mathf.FloorToInt(time * rate) % frames.Length];
    }
}
