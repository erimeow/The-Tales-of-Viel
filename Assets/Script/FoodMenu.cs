using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal sealed class FoodMenu : MonoBehaviour
{
    private static readonly Vector2 Center = new(.5f, .5f);
    private readonly Button[] actions = new Button[FoodCatalog.Items.Length];
    private readonly TMP_Text[] actionText = new TMP_Text[FoodCatalog.Items.Length];
    private readonly TMP_Text[] details = new TMP_Text[FoodCatalog.Items.Length];
    private PlayerWeaponLoadout player;
    private RectTransform panel;
    private TMP_Text title;
    private TMP_Text balance;
    private TMP_Text message;
    private GameObject bagButton;
    private Canvas canvas;
    private bool market;
    private int lastCoins;
    private int lastMeat;

    public bool IsOpen => panel != null && panel.gameObject.activeSelf;
    public bool MarketOpen => IsOpen && market;

    public void Initialize(PlayerWeaponLoadout owner)
    {
        player = owner;
        canvas = HudArt.Overlay(null, "Food inventory HUD", 90, true);
        bagButton = MakeButton(canvas.transform, "Food [I]", Vector2.one, new Vector2(-76, -72),
            new Vector2(124, 34), ToggleBag, out _).gameObject;
        panel = HudArt.Rect(canvas.transform, "Food panel", Center, Vector2.zero, new Vector2(704, 452));
        HudArt.Image(panel, HudArt.Gold).raycastTarget = true;
        var content = HudArt.Rect(panel, "Contents", Center, Vector2.zero, new Vector2(696, 444));
        HudArt.ShopPanel(HudArt.Image(content, HudArt.Ink));
        title = HudArt.Text(content, "Title", "", Center, new Vector2(-65, 176), new Vector2(490, 39), 30, HudArt.Gold);
        title.fontStyle = FontStyles.Bold;
        MakeButton(content, "Close", Center, new Vector2(272, 176), new Vector2(98, 34), Close, out _);
        balance = HudArt.Text(content, "Wallet", "", Center, new Vector2(0, 129), new Vector2(640, 30), 18, HudArt.Gold);
        for (int i = 0; i < FoodCatalog.Items.Length; i++)
        {
            int index = i;
            var food = FoodCatalog.Items[i];
            var row = HudArt.Rect(content, food.Name, Center, new Vector2(0, 85 - i * 54), new Vector2(648, 49));
            HudArt.Image(row, new Color32(65, 45, 45, 255));
            var icon = HudArt.Image(HudArt.Rect(row, "Icon", Center, new Vector2(-292, 0), new Vector2(36, 36)), Color.white);
            icon.sprite = food.Icon;
            icon.preserveAspect = true;
            var name = HudArt.Text(row, "Name", food.Name, Center, new Vector2(-142, 11), new Vector2(236, 23), 18, HudArt.Cream);
            name.alignment = TextAlignmentOptions.Left;
            var description = HudArt.Text(row, "Description", food.Description, Center, new Vector2(-142, -11),
                new Vector2(236, 20), 14, HudArt.Cream);
            description.alignment = TextAlignmentOptions.Left;
            details[i] = HudArt.Text(row, "Details", "", Center, new Vector2(85, 0), new Vector2(206, 42), 16, HudArt.Gold);
            actions[i] = MakeButton(row, "Action", Center, new Vector2(255, 0), new Vector2(108, 33),
                () => Select(index), out actionText[i]);
        }

        message = HudArt.Text(content, "Status", "", Center, new Vector2(0, -177), new Vector2(642, 34), 17, HudArt.Cream);
        panel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
    }

    private void Update()
    {
        if (panel == null)
            return;

        bool weaponShop = ShopStall.BlocksInput(player.Body);
        bool available = !player.IsDead && player.Body.simulated && !WelcomeDialogue.IsDialogueActive && !weaponShop;
        bagButton.SetActive(available && !IsOpen);
        if (!available)
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(KeyCode.I) && !MarketOpen)
            ToggleBag();
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            Close();

        var wallet = gameManager.instance;
        if (IsOpen && wallet != null && (wallet.coinCollected != lastCoins || wallet.meatCollected != lastMeat))
            Refresh(wallet);
    }

    public bool OpenMarket()
    {
        if (IsOpen)
            return false;
        return Open(true);
    }

    public void CloseMarket()
    {
        if (MarketOpen)
            Close();
    }

    private void ToggleBag()
    {
        if (MarketOpen)
            return;
        if (IsOpen)
            Close();
        else
            Open(false);
    }

    private bool Open(bool shop)
    {
        var wallet = gameManager.instance;
        if (wallet == null || player.IsDead || !player.Body.simulated ||
            WelcomeDialogue.IsDialogueActive || ShopStall.BlocksInput(player.Body))
            return false;

        market = shop;
        title.text = shop ? "FOOD MARKET" : "INVENTORY  /  FOOD";
        message.text = shop ? "Purchases go into your food inventory. Eat later with I." : "Choose a food to restore health.";
        player.Body.linearVelocity = Vector2.zero;
        panel.gameObject.SetActive(true);
        Refresh(wallet);
        return true;
    }

    private void Close()
    {
        if (IsOpen)
            panel.gameObject.SetActive(false);
    }

    private void Select(int index)
    {
        var wallet = gameManager.instance;
        if (!IsOpen || wallet == null || player.IsDead)
            return;

        var food = FoodCatalog.Items[index];
        if (market)
            message.text = wallet.TryBuyFood(food.Type) ? $"Packed {food.Name}." : "You need more coins or meat.";
        else
            message.text = wallet.Inventory.ConsumeFood(food.Type, () => player.TryHeal(food.Healing))
                ? $"Ate {food.Name} and recovered health." : "Your health is full or this food is unavailable.";
        Refresh(wallet);
    }

    private void Refresh(gameManager wallet)
    {
        lastCoins = wallet.coinCollected;
        lastMeat = wallet.meatCollected;
        balance.text = $"COINS  {lastCoins}       MEAT  {lastMeat}";
        for (int i = 0; i < FoodCatalog.Items.Length; i++)
        {
            var food = FoodCatalog.Items[i];
            int quantity = wallet.Inventory.Count(food.Type);
            details[i].text = market
                ? $"+{food.Healing} HP   •   {food.Coins} coins\n{food.Meat} meat   |   Owned {quantity}"
                : $"+{food.Healing} HP   •   In bag {quantity}";
            actionText[i].text = market ? "Buy" : "Eat";
            actions[i].interactable = market ? lastCoins >= food.Coins && lastMeat >= food.Meat : quantity > 0;
        }
    }

    private static Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 at,
        Vector2 size, System.Action click, out TMP_Text text)
    {
        var rect = HudArt.Rect(parent, label, anchor, at, size);
        var image = HudArt.Image(rect, new Color32(118, 65, 50, 255));
        image.raycastTarget = true;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => click());
        text = HudArt.Text(rect, "Label", label, Center, new Vector2(0, 2), size, 16, HudArt.Cream);
        HudArt.ShopButton(button, image, text, label == "Close");
        return button;
    }
}
