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
        int extraHeight = warrior ? 188 : 0;
        int top = extraHeight / 2;
        RectTransform frame = Place(shade, "Gold frame", 0, 0, 710, 386 + extraHeight);
        Paint(frame, Gold);
        RectTransform panel = Place(frame, "Shop", 0, 0, 702, 378 + extraHeight);
        HudArt.ShopPanel(Paint(panel, Panel));

        heading = Label(panel, "Title", "WEAPON SHOP", -78, 150 + top, 490, 38, 33, Gold);
        heading.fontStyle = FontStyles.Bold;
        MakeButton(panel, "Leave", 270, 150 + top, 112, 36, dismiss, out _);
        balance = Label(panel, "Wallet", "", 0, 109 + top, 646, 30, 19, Gold);
        equipped = Label(panel, "Equipment", "", 0, 77 + top, 646, 26, 18, Cream);
        Paint(Place(panel, "Divider", 0, 55 + top, 650, 2), Gold);

        if (warrior)
            Label(panel, "Weapon section", "SWORDS", 0, 42 + top, 650, 20, 16, Gold);
        for (int tier = 0; tier < weapons.Length; tier++)
        {
            int index = tier;
            weapons[tier] = MakeRow(panel, WeaponShopCatalog.Name(role, tier), tier,
                (warrior ? 12 : 25) + top - tier * 49, $"+{WeaponShopCatalog.Damage(tier)} DMG",
                WeaponShopCatalog.Coins(tier), WeaponShopCatalog.Meat(tier), () => select(index, false));
        }

        if (warrior)
        {
            Label(panel, "Shield section", "SHIELDS", 0, -115 + top, 650, 22, 16, Gold);
            for (int tier = 0; tier < shields.Length; tier++)
            {
                int index = tier;
                shields[tier] = MakeRow(panel, ShieldShopCatalog.Name(tier), tier,
                    -146 + top - tier * 49, $"+{ShieldShopCatalog.Defense(tier)} DEF",
                    ShieldShopCatalog.Coins(tier), ShieldShopCatalog.Meat(tier), () => select(index, true));
                shields[tier].Icon.sprite = Resources.Load<Sprite>(ShieldShopCatalog.ArtPath(tier, "Icon"));
            }
        }

        message = Label(panel, "Message", "", 0, -143 - top, 646, 36, 18, Gold);
    }

    public void Open(CharacterRole characterRole, Sprite icon)
    {
        role = characterRole;
        heading.text = $"WEAPON SHOP  /  {WeaponShopCatalog.Category(role).ToUpperInvariant()}";
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
        if (current == lastEquipped && shield == lastShield && wallet.coinCollected == lastCoins && wallet.meatCollected == lastMeat)
            return;

        lastEquipped = current;
        lastShield = shield;
        lastCoins = wallet.coinCollected;
        lastMeat = wallet.meatCollected;
        balance.text = $"COINS  {lastCoins}      MEAT  {lastMeat}";
        equipped.text = $"EQUIPPED  {WeaponShopCatalog.Name(role, current)}  (+{wallet.WeaponBonus(role)} DMG)";

        for (int tier = 0; tier < weapons.Length; tier++)
            RefreshRow(weapons[tier], tier == current, wallet.OwnsWeapon(role, tier),
                WeaponShopCatalog.Coins(tier), WeaponShopCatalog.Meat(tier));

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
        int y, string bonus, int coins, int meat, Action select)
    {
        RectTransform row = Place(panel, name, 0, y, 650, 44);
        Paint(row, tier % 2 == 0 ? new Color32(73, 47, 40, 255) : new Color32(61, 39, 35, 255));
        Image icon = Paint(Place(row, "Icon", -298, 0, 34, 34), Color.white);
        icon.preserveAspect = true;
        Label(row, "Name", name, -169, 0, 202, 40, 19, Cream);
        Label(row, "Bonus", bonus, -15, 0, 98, 40, 17, Gold);
        Label(row, "Price", tier == 0 ? "Starter gear" : $"{coins} coins   {meat} meat",
            118, 0, 166, 40, 16, Cream);
        Button button = MakeButton(row, "Buy", 249, 0, 124, 34, select, out TMP_Text label);
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
        HudArt.Typography(label, name == "Title" || name.EndsWith("section", StringComparison.Ordinal));
        label.fontSize = size;
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
        HudArt.ShopButton(button, background, label, value == "Leave");
        return button;
    }
}
