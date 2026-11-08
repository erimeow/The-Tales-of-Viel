using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class WeaponShopView : MonoBehaviour
{
    private static readonly Color32 Gold = new(246, 208, 122, 255);
    private static readonly Color32 Cream = new(252, 233, 194, 255);
    private static readonly Color32 Panel = new(48, 31, 30, 255);
    private static readonly Color32 ButtonColor = new(124, 67, 42, 255);

    private TMP_Text heading;
    private TMP_Text balance;
    private TMP_Text equipped;
    private TMP_Text message;
    private readonly (Button Button, TMP_Text Label, Image Icon)[] weapons = new (Button, TMP_Text, Image)[WeaponShopCatalog.Count];
    private readonly (Button Button, TMP_Text Label, Image Icon)[] shields = new (Button, TMP_Text, Image)[ShieldShopCatalog.Count];

    private CharacterRole role;
    private int lastCoins;
    private int lastMeat;
    private int lastEquipped;
    private int lastShield;
    private int lastSpeed = -1;
    private readonly RectTransform[] sections = new RectTransform[3];
    private readonly UnityEngine.UI.Button[] tabs = new UnityEngine.UI.Button[3];
    private readonly (UnityEngine.UI.Button Button, TMP_Text Label, UnityEngine.UI.Image Icon)[] speedRows =
        new (UnityEngine.UI.Button, TMP_Text, UnityEngine.UI.Image)[ArcherSpeedCatalog.Count - 1];

    public static WeaponShopView Create(Transform stall, CharacterRole role, Action dismiss, Action<int, bool> select)
    {
        var root = new GameObject("Weapon Shop UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(WeaponShopView));
        root.SetActive(false);
        root.transform.SetParent(stall, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var view = root.GetComponent<WeaponShopView>();
        view.role = role;
        view.Build(root.GetComponent<RectTransform>(), dismiss, select);
        return view;
    }

    private void Build(RectTransform canvas, Action dismiss, Action<int, bool> select)
    {
        RectTransform shade = new GameObject("Backdrop", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        shade.SetParent(canvas, false);
        shade.anchorMin = Vector2.zero;
        shade.anchorMax = Vector2.one;
        shade.offsetMin = shade.offsetMax = Vector2.zero;
        shade.GetComponent<Image>().color = new Color32(16, 11, 15, 160);

        bool warrior = role == CharacterRole.Warrior;
        int top = 0;
        RectTransform panel = Place(shade, "Shop", 0, 0, 724, 570);
        MarketArt.Surface(panel, MarketArt.Paper);
        MarketArt.Surface(Place(panel, "Title ribbon", 0, 249, 500, 42), MarketArt.Edge);

        heading = Label(panel, "Title", "BUY SOMETHING!", 0, 249, 490, 38, 29, Cream);
        MakeButton(panel, "Exit", 0, -252, 160, 32, dismiss, out _);
        balance = Label(panel, "Wallet", "", 0, 214, 670, 26, 19, Cream);
        equipped = Label(panel, "Equipment", "", 0, 185, 680, 26, 17, Cream);
        for (int i = 0; i < sections.Length; i++)
            sections[i] = Place(panel, "Item section " + i, 0, 0, 700, 400);
        tabs[0] = MakeButton(panel, "Weapons", -110, 151, 180, 28, () => ShowSection(0), out _);
        if (warrior)
            tabs[1] = MakeButton(panel, "Shields", 110, 151, 180, 28, () => ShowSection(1), out _);
        if (role == CharacterRole.Archer)
            tabs[2] = MakeButton(panel, "Quick Draw", 110, 151, 180, 28, () => ShowSection(2), out _);
        for (int tier = 0; tier < weapons.Length; tier++)
        {
            int index = tier;
            weapons[tier] = MakeRow(sections[0], WeaponShopCatalog.Name(role, tier), tier,
                (warrior ? 12 : 25) + top - tier * 49, $"+{WeaponShopCatalog.Damage(tier)} DMG",
                WeaponShopCatalog.Coins(tier), WeaponShopCatalog.Meat(tier), () => select(index, false));
        }

        if (warrior)
        {
            for (int tier = 0; tier < shields.Length; tier++)
            {
                int index = tier;
                shields[tier] = MakeRow(sections[1], ShieldShopCatalog.Name(tier), tier,
                    -146 + top - tier * 49, $"+{ShieldShopCatalog.Defense(tier)} DEF",
                    ShieldShopCatalog.Coins(tier), ShieldShopCatalog.Meat(tier), () => select(index, true));
                shields[tier].Icon.sprite = Resources.Load<Sprite>(ShieldShopCatalog.ArtPath(tier, "Icon"));
            }
        }

        if (role == CharacterRole.Archer)
        {
            for (int tier = 1; tier < ArcherSpeedCatalog.Count; tier++)
            {
                int index = tier;
                speedRows[tier - 1] = MakeRow(sections[2], "Quick Draw " + tier, tier,
                    -140 + top - (tier - 1) * 49, $"+{tier * 50}% SPD",
                    ArcherSpeedCatalog.Coins(tier), ArcherSpeedCatalog.Meat(tier), () =>
                    {
                        var wallet = gameManager.instance;
                        if (wallet == null) return;
                        ShowMessage(wallet.TryBuyArcherSpeed(index)
                            ? $"Quick Draw {index} learned. Your arrows fire faster."
                            : "Buy the previous tier first and check your coins and meat.");
                        Refresh(wallet);
                    }, tier - 1);
                speedRows[tier - 1].Icon.sprite = Resources.Load<Sprite>(WeaponShopCatalog.ArtPath(role, tier, "Icon"));
            }
        }

        message = Label(panel, "Message", "", 0, -211, 680, 40, 16, Cream);
        ShowSection(0);
    }

    private void ShowSection(int selected)
    {
        for (int i = 0; i < sections.Length; i++)
        {
            sections[i].gameObject.SetActive(i == selected);
            if (tabs[i] != null) tabs[i].interactable = i != selected;
        }
    }

    public void Open(CharacterRole characterRole, Sprite icon)
    {
        role = characterRole;
        heading.text = "BUY SOMETHING!";
        ShowSection(0);
        for (int tier = 0; tier < WeaponShopCatalog.Count; tier++)
        {
            weapons[tier].Icon.sprite = tier == 0
                ? icon
                : Resources.Load<Sprite>(WeaponShopCatalog.ArtPath(role, tier, "Icon"));
        }

        lastEquipped = -1;
        ShowMessage("Use coins and meat to buy upgrades.  Leave or press Esc to walk away.");
        gameObject.SetActive(true);
    }

    public void Refresh(gameManager wallet)
    {
        int current = wallet.EquippedWeapon(role);
        int shield = role == CharacterRole.Warrior ? wallet.EquippedShield : 0;
        if (current == lastEquipped && shield == lastShield && wallet.ArcherSpeedTier == lastSpeed && wallet.coinCollected == lastCoins && wallet.meatCollected == lastMeat)
            return;

        lastEquipped = current;
        lastShield = shield;
        lastSpeed = wallet.ArcherSpeedTier;
        lastCoins = wallet.coinCollected;
        lastMeat = wallet.meatCollected;
        balance.text = $"COINS  {lastCoins}      MEAT  {lastMeat}";
        equipped.text = $"EQUIPPED  {WeaponShopCatalog.Name(role, current)}  (+{wallet.WeaponBonus(role)} DMG)";

        for (int tier = 0; tier < weapons.Length; tier++)
            RefreshRow(weapons[tier], tier == current, wallet.OwnsWeapon(role, tier),
                WeaponShopCatalog.Coins(tier), WeaponShopCatalog.Meat(tier));

        if (role == CharacterRole.Archer)
        {
            equipped.text += $"   |   ATTACK SPEED x{wallet.ArcherAttackSpeed:0.0}";
            for (int tier = 1; tier < ArcherSpeedCatalog.Count; tier++)
            {
                var row = speedRows[tier - 1];
                bool owned = tier <= lastSpeed;
                bool next = tier == lastSpeed + 1;
                row.Label.text = owned ? "Learned" : next ? "Buy" : "Locked";
                row.Button.interactable = next && lastCoins >= ArcherSpeedCatalog.Coins(tier)
                    && lastMeat >= ArcherSpeedCatalog.Meat(tier);
            }
        }

        if (role == CharacterRole.Warrior)
        {
            equipped.text += $"   |   {ShieldShopCatalog.Name(shield)} (+{wallet.ShieldDefense} DEF)";
            for (int tier = 0; tier < shields.Length; tier++)
                RefreshRow(shields[tier], tier == shield, wallet.OwnsShield(tier),
                    ShieldShopCatalog.Coins(tier), ShieldShopCatalog.Meat(tier));
        }
    }

    private void RefreshRow((Button Button, TMP_Text Label, Image Icon) row, bool equipped, bool owned, int coins, int meat)
    {
        row.Button.interactable = !equipped && (owned || (lastCoins >= coins && lastMeat >= meat));
        row.Label.text = equipped ? "Equipped" : owned ? "Equip" : "Buy";
    }

    private static (Button, TMP_Text, Image) MakeRow(RectTransform panel, string name, int tier,
        int y, string bonus, int coins, int meat, Action select, int slot = -1)
    {
        RectTransform row = Place(panel, name, ((slot < 0 ? tier : slot) - 1) * 226, -25, 210, 318);
        MarketArt.Surface(row, MarketArt.Card);
        MarketArt.Surface(Place(row, "Icon well", 0, 98, 196, 108), MarketArt.Paper);
        Image icon = Paint(Place(row, "Icon", 0, 98, 82, 82), Color.white);
        icon.preserveAspect = true;
        MarketArt.Surface(Place(row, "Name ribbon", 0, 25, 198, 34), MarketArt.Edge);
        Label(row, "Name", name.ToUpperInvariant(), 0, 25, 190, 30, 18, Cream);
        Label(row, "Bonus", bonus + (name.StartsWith("Quick Draw") ? "\nFIRE ARROWS FASTER" : name.Contains("Shield") ? "\nADDED DEFENSE" : "\nATTACK BONUS"), 0, -27, 184, 64, 18, Cream);
        Label(row, "Price", tier == 0 ? "Starter gear" : $"{coins} coins   {meat} meat",
            0, -87, 190, 36, 17, Cream);
        Button button = MakeButton(row, "Buy", 0, -135, 190, 30, select, out TMP_Text label);
        return (button, label, icon);
    }

    public void ShowMessage(string value) => message.text = value;

    private static RectTransform Place(RectTransform parent, string name, float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static Image Paint(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text Label(RectTransform parent, string name, string value,
        float x, float y, float width, float height, float size, Color color)
    {
        var label = Place(parent, name, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        MarketArt.Typography(label);
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static Button MakeButton(RectTransform parent, string value, float x, float y,
        float width, float height, Action action, out TMP_Text label)
    {
        RectTransform rect = Place(parent, value, x, y, width, height);
        Image background = Paint(rect, ButtonColor);
        background.raycastTarget = true;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => action());
        label = Label(rect, "Label", value, 0, 2, width, height, 16, Cream);
        MarketArt.Button(button, background, label);
        return button;
    }
}
