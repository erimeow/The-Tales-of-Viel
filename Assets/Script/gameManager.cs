
using TMPro;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    public TextMeshProUGUI meatText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI attkText;

    public int meatCollected = 0;
    public int coinCollected = 0;
    public int attkCollected = 0;

    private int pendingHealthScene = -1;
    private float pendingHealth;

    internal void CarryHealth(int destination, float health)
    {
        pendingHealthScene = destination;
        pendingHealth = health;
    }

    internal float TakeCarriedHealth(int scene, float maxHealth)
    {
        if (pendingHealthScene != scene)
            return maxHealth;

        pendingHealthScene = -1;
        return Mathf.Clamp(pendingHealth, 0f, maxHealth);
    }


    private readonly int[] ownedWeapons = new int[3];
    private readonly int[] equippedWeapons = new int[3];

    public int EquippedWeapon(CharacterRole role) => equippedWeapons[(int)role];
    public int WeaponBonus(CharacterRole role) => WeaponShopCatalog.Damage(EquippedWeapon(role));

    public bool OwnsWeapon(CharacterRole role, int tier)
    {
        return tier >= 0 && tier < WeaponShopCatalog.Count &&
            (tier == 0 || (ownedWeapons[(int)role] & (1 << tier)) != 0);
    }

    public bool TryBuyWeapon(CharacterRole role, int tier)
    {
        if (tier <= 0 || tier >= WeaponShopCatalog.Count || OwnsWeapon(role, tier))
            return false;

        if (!TrySpend(WeaponShopCatalog.Coins(tier), WeaponShopCatalog.Meat(tier)))
            return false;

        ownedWeapons[(int)role] |= 1 << tier;
        EquipWeapon(role, tier);
        UpdateUI();
        return true;
    }

    public bool EquipWeapon(CharacterRole role, int tier)
    {
        if (!OwnsWeapon(role, tier))
            return false;

        equippedWeapons[(int)role] = tier;
        return true;
    }

    public Inventory Inventory { get; } = new Inventory(includeStarterItems: false);

    public bool TryBuyFood(Item.ItemType type)
    {
        if (!FoodCatalog.TryGet(type, out var food) || !TrySpend(food.Coins, food.Meat))
            return false;

        Inventory.AddFood(type);
        UpdateUI();
        return true;
    }

    private int ownedShields;
    public int EquippedShield { get; private set; }
    public int ShieldDefense => ShieldShopCatalog.Defense(EquippedShield);

    public bool OwnsShield(int tier) => tier >= 0 && tier < ShieldShopCatalog.Count &&
        (tier == 0 || (ownedShields & (1 << tier)) != 0);

    public bool TryBuyShield(int tier)
    {
        if (tier <= 0 || tier >= ShieldShopCatalog.Count || OwnsShield(tier) ||
            !TrySpend(ShieldShopCatalog.Coins(tier), ShieldShopCatalog.Meat(tier)))
            return false;

        ownedShields |= 1 << tier;
        EquipShield(tier);
        UpdateUI();
        return true;
    }

    public bool EquipShield(int tier)
    {
        if (!OwnsShield(tier))
            return false;

        EquippedShield = tier;
        return true;
    }

    private bool TrySpend(int coins, int meat)
    {
        if (coinCollected < coins || meatCollected < meat)
            return false;

        coinCollected -= coins;
        meatCollected -= meat;
        return true;
    }

    // Sound
    public GameObject SFXinstance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            instance.meatText = meatText;
            instance.coinText = coinText;
            instance.attkText = attkText;

            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        meatCollected = 0;
        coinCollected = 0;
        attkCollected = 0;
        pendingHealthScene = -1;
        Inventory.Clear();
        ownedShields = 0;
        EquippedShield = 0;
        System.Array.Clear(ownedWeapons, 0, ownedWeapons.Length);
        System.Array.Clear(equippedWeapons, 0, equippedWeapons.Length);

        UpdateUI();

        Debug.Log("Game Manager reset! All collected items are now 0.");
    }

    public void playSFX(AudioClip clip, int volume, Transform t)
    {
        AudioSource aud = Instantiate(
            SFXinstance, t.position, Quaternion.identity
        ).GetComponent<AudioSource>();

        aud.clip = clip;
        aud.volume = volume;
        aud.Play();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (meatText != null)
            meatText.text = meatCollected.ToString();

        if (coinText != null)
            coinText.text = coinCollected.ToString();

        if (attkText != null)
            attkText.text = attkCollected.ToString();
    }
}
