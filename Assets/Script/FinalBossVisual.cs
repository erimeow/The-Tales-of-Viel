using UnityEngine;

public class FinalBossVisual : MonoBehaviour
{
    [SerializeField] private Skeleton boss;
    [SerializeField] private SpriteRenderer artwork;
    private Animator animator;
    private SpriteRenderer attackRing;
    private Sprite ringSprite;
    private Texture2D ringTexture;
    private float lastHealth;
    private float flashUntil;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private float attackPhase;

    private void OnEnable()
    {
        if (boss != null)
            lastHealth = boss.health;
        animator = boss != null ? boss.GetComponent<Animator>() : null;
        basePosition = transform.localPosition;
        baseScale = transform.localScale;
        BuildAttackRing();
    }

    private void LateUpdate()
    {
        if (boss == null || artwork == null)
            return;
        if (boss.health < lastHealth)
            flashUntil = Time.time + 0.12f;
        lastHealth = boss.health;
        bool attacking = animator != null && animator.GetBool("IsAttacking") && boss.health > 0;
        if (attacking)
            attackPhase += Time.deltaTime * 1.8f;
        else
            attackPhase = 0f;
        float pulse = attacking ? Mathf.Sin(Mathf.Repeat(attackPhase, 1f) * Mathf.PI) : 0f;
        artwork.color = Time.time < flashUntil ? new Color(1f, .5f, .58f) :
            attacking ? Color.Lerp(Color.white, new Color(1f, .42f, 1f), pulse * .65f) : Color.white;
        transform.localScale = baseScale * (1f + pulse * .12f);
        transform.localPosition = basePosition + Vector3.up * (Mathf.Sin(Time.time * 2f) * .06f);
        if (attackRing != null)
        {
            attackRing.enabled = attacking;
            if (attacking)
            {
                attackRing.transform.localScale = Vector3.one * (.7f + Mathf.Repeat(attackPhase, 1f) * .5f);
                attackRing.color = new Color(1f, .26f, 1f, (1f - Mathf.Repeat(attackPhase, 1f)) * .85f);
            }
        }
    }

    private void BuildAttackRing()
    {
        if (attackRing != null || artwork == null)
            return;
        ringTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        ringTexture.filterMode = FilterMode.Point;
        ringTexture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color32[64 * 64];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                pixels[y * 64 + x] = distance >= 26f && distance <= 29f &&
                    ((x / 5 + y / 7) % 8 != 0) ? new Color32(220, 83, 255, 255) : new Color32(0, 0, 0, 0);
            }
        ringTexture.SetPixels32(pixels);
        ringTexture.Apply();
        ringSprite = Sprite.Create(ringTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 18f);
        var visual = new GameObject("Attack pulse");
        visual.transform.SetParent(transform, false);
        attackRing = visual.AddComponent<SpriteRenderer>();
        attackRing.sprite = ringSprite;
        attackRing.sharedMaterial = artwork.sharedMaterial;
        attackRing.sortingLayerID = artwork.sortingLayerID;
        attackRing.sortingOrder = artwork.sortingOrder + 1;
        attackRing.enabled = false;
    }

    private void OnDestroy()
    {
        if (ringSprite != null)
            Destroy(ringSprite);
        if (ringTexture != null)
            Destroy(ringTexture);
    }
}
