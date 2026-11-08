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
    private readonly GameObject[] cards = new GameObject[FoodCatalog.Items.Length];
    private int page;
    private TMP_Text pageLabel;
    private Button previousPage;
    private Button nextPage;

    public bool IsOpen => panel != null && panel.gameObject.activeSelf;
    public bool MarketOpen => IsOpen && market;

    public void Initialize(PlayerWeaponLoadout owner)
    {
        player = owner;
        canvas = HudArt.Overlay(null, "Food inventory HUD", 90, true);
        bagButton = MakeButton(canvas.transform, "Food [I]", Vector2.one, new Vector2(-76, -72),
            new Vector2(124, 34), ToggleBag, out _).gameObject;
        panel = HudArt.Rect(canvas.transform, "Food panel", Center, Vector2.zero, new Vector2(724, 570));
        MarketArt.Surface(panel, MarketArt.Paper);
        panel.GetComponent<Image>().raycastTarget = true;
        var content = panel;
        MarketArt.Surface(HudArt.Rect(content, "Title ribbon", Center, new Vector2(0, 249), new Vector2(500, 42)), MarketArt.Edge);
        title = HudArt.Text(content, "Title", "", Center, new Vector2(0, 249), new Vector2(490, 38), 29, MarketArt.TextColor);
        MakeButton(content, "Exit", Center, new Vector2(0, -252), new Vector2(160, 32), Close, out _);
        balance = HudArt.Text(content, "Wallet", "", Center, new Vector2(0, 208), new Vector2(670, 30), 19, MarketArt.TextColor);
        previousPage = MakeButton(content, "<", Center, new Vector2(-115, 155), new Vector2(70, 28), () => ChangePage(-1), out _);
        nextPage = MakeButton(content, ">", Center, new Vector2(115, 155), new Vector2(70, 28), () => ChangePage(1), out _);
        pageLabel = HudArt.Text(content, "Page", "", Center, new Vector2(0, 155), new Vector2(150, 28), 18, MarketArt.TextColor);
        for (int i = 0; i < FoodCatalog.Items.Length; i++)
        {
            int index = i;
            var food = FoodCatalog.Items[i];
            var row = HudArt.Rect(content, food.Name, Center, new Vector2((i % 3 - 1) * 226, -25), new Vector2(210, 318));
            cards[i] = row.gameObject;
            MarketArt.Surface(row, MarketArt.Card);
            MarketArt.Surface(HudArt.Rect(row, "Icon well", Center, new Vector2(0, 98), new Vector2(196, 108)), MarketArt.Paper);
            var icon = HudArt.Image(HudArt.Rect(row, "Icon", Center, new Vector2(0, 98), new Vector2(82, 82)), Color.white);
            icon.sprite = food.Icon;
            icon.preserveAspect = true;
            MarketArt.Surface(HudArt.Rect(row, "Name ribbon", Center, new Vector2(0, 25), new Vector2(198, 34)), MarketArt.Edge);
            HudArt.Text(row, "Name", food.Name.ToUpperInvariant(), Center, new Vector2(0, 25), new Vector2(190, 30), 18, MarketArt.TextColor);
            HudArt.Text(row, "Description", food.Description, Center, new Vector2(0, -23),
                new Vector2(184, 56), 17, MarketArt.TextColor);
            details[i] = HudArt.Text(row, "Details", "", Center, new Vector2(0, -84), new Vector2(194, 52), 16, MarketArt.TextColor);
            actions[i] = MakeButton(row, "Action", Center, new Vector2(0, -135), new Vector2(190, 30),
                () => Select(index), out actionText[i]);
        }

        message = HudArt.Text(content, "Status", "", Center, new Vector2(0, -211), new Vector2(680, 40), 16, MarketArt.TextColor);
        foreach (var label in panel.GetComponentsInChildren<TMP_Text>(true))
            MarketArt.Typography(label);
        ChangePage(0);
        panel.gameObject.SetActive(false);
    }

    private void ChangePage(int direction)
    {
        int pages = (cards.Length + 2) / 3;
        page = Mathf.Clamp(page + direction, 0, pages - 1);
        for (int i = 0; i < cards.Length; i++) cards[i].SetActive(i / 3 == page);
        pageLabel.text = $"{page + 1} / {pages}";
        previousPage.interactable = page > 0;
        nextPage.interactable = page < pages - 1;
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
        page = 0;
        ChangePage(0);
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
        MarketArt.Button(button, image, text);
        return button;
    }
}
