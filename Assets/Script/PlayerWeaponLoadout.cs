using System;
using System.Collections.Generic;
using UnityEngine;

internal sealed class PlayerWeaponLoadout : MonoBehaviour
{
    public static PlayerWeaponLoadout Active { get; private set; }

    public CharacterRole Role { get; private set; }
    public Rigidbody2D Body { get; private set; }
    public bool IsDead => isDead();

    private readonly List<Collider2D> colliders = new();
    private readonly Dictionary<(Sprite, int, int), Sprite> frames = new();
    private readonly Dictionary<(Texture2D, int, int), Texture2D> sheets = new();
    private Func<bool> isDead;
    private Func<float, bool> heal;
    private Func<float> currentHealth;
    public float CurrentHealth => currentHealth();
    internal FoodMenu Food { get; private set; }

    internal bool TryHeal(float amount) => heal(amount);

    internal static bool BlocksInput(Rigidbody2D body) =>
        WelcomeDialogue.BlocksInput(body) || ShopStall.BlocksInput(body) ||
        (Active != null && Active.Body == body &&
            Active.Food != null && Active.Food.IsOpen);

    private void Start()
    {
        Food = gameObject.AddComponent<FoodMenu>();
        Food.Initialize(this);
    }
    private SpriteRenderer spriteRenderer;
    private Sprite originalFrame;
    private Sprite displayedFrame;

    public static void Register(GameObject player)
    {
        if (player.TryGetComponent<PlayerMovement2>(out var warrior))
            player.AddComponent<PlayerWeaponLoadout>().Initialize(CharacterRole.Warrior, () => warrior.IsDead, warrior.TryHeal, () => warrior.health);
        else if (player.TryGetComponent<PlayerMovementArcher>(out var archer))
            player.AddComponent<PlayerWeaponLoadout>().Initialize(CharacterRole.Archer, () => archer.IsDead, archer.TryHeal, () => archer.health);
        else if (player.TryGetComponent<LanceMovement>(out var spear))
            player.AddComponent<PlayerWeaponLoadout>().Initialize(CharacterRole.Spear, () => spear.IsDead, spear.TryHeal, () => spear.health);
    }

    private void Initialize(CharacterRole role, Func<bool> dead, Func<float, bool> restore, Func<float> readHealth)
    {
        Role = role;
        Body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isDead = dead;
        heal = restore;
        currentHealth = readHealth;
        Active = this;
    }

    public bool Overlaps(Collider2D volume, float margin)
    {
        if (Body == null || !Body.simulated || !isActiveAndEnabled || !volume.enabled)
            return false;

        // Query the spawned body's actual shapes, including colliders on child objects.
        Body.GetAttachedColliders(colliders);
        foreach (Collider2D collider in colliders)
        {
            if (!collider.isActiveAndEnabled)
                continue;

            ColliderDistance2D distance = volume.Distance(collider);
            if (distance.isValid && distance.distance <= margin)
                return true;
        }
        return false;
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        // The animator writes the source frame. Retain it when the current pose holds a frame.
        if (spriteRenderer.sprite != displayedFrame)
            originalFrame = spriteRenderer.sprite;

        var wallet = gameManager.instance;
        int weapon = wallet?.EquippedWeapon(Role) ?? 0;
        int shield = Role == CharacterRole.Warrior ? wallet?.EquippedShield ?? 0 : 0;
        displayedFrame = (weapon == 0 && shield == 0) || originalFrame == null
            ? originalFrame
            : UpgradeFrame(originalFrame, weapon, shield);
        if (spriteRenderer.sprite != displayedFrame)
            spriteRenderer.sprite = displayedFrame;
    }

    private Sprite UpgradeFrame(Sprite source, int weapon, int shield)
    {
        if (frames.TryGetValue((source, weapon, shield), out var frame))
            return frame == null ? source : frame;

        Texture2D original = source.texture;
        if (!sheets.TryGetValue((original, weapon, shield), out var sheet))
        {
            string path = shield == 0
                ? WeaponShopCatalog.ArtPath(Role, weapon, original.name)
                : ShieldShopCatalog.ArtPath(shield, $"{weapon}/{original.name}");
            sheet = Resources.Load<Texture2D>(path);
            sheets.Add((original, weapon, shield), sheet);
        }

        if (sheet != null)
        {
            Rect rect = source.rect;
            Vector2 pivot = source.pivot / rect.size;
            frame = Sprite.Create(sheet, rect, pivot, source.pixelsPerUnit, 0, SpriteMeshType.FullRect, source.border);
        }
        frames.Add((source, weapon, shield), frame);
        return frame == null ? source : frame;
    }

    private void OnDestroy()
    {
        if (Active == this)
            Active = null;

        foreach (Sprite frame in frames.Values)
        {
            if (frame != null)
                Destroy(frame);
        }
    }
}
