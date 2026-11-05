using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ShopStall : MonoBehaviour
{
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite bowIcon;
    [SerializeField] private Sprite spearIcon;

    private static ShopStall openStall;
    private BoxCollider2D approach;
    private PlayerWeaponLoadout visitor;
    private WeaponShopView view;
    private bool dismissed;

    public static bool BlocksInput(Rigidbody2D body) =>
        openStall != null && openStall.visitor != null && openStall.visitor.Body == body;

    private void Awake()
    {
        approach = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        PlayerWeaponLoadout player = PlayerWeaponLoadout.Active;
        if (player != visitor)
        {
            Close();
            visitor = player;
            dismissed = false;
        }

        if (visitor == null || !visitor.Overlaps(approach, openStall == this || dismissed ? 0.12f : 0f))
        {
            Close();
            dismissed = false;
            return;
        }

        if (visitor.IsDead || WelcomeDialogue.IsDialogueActive || gameManager.instance == null)
        {
            Close();
            return;
        }

        if (dismissed)
            return;

        if (view == null)
            view = WeaponShopView.Create(transform, visitor.Role, Dismiss, SelectGear);

        if (!view.gameObject.activeSelf)
        {
            if (openStall != null)
                return;

            openStall = this;
            visitor.Body.linearVelocity = Vector2.zero;
            CharacterRole role = visitor.Role;
            view.Open(role, role switch
            {
                CharacterRole.Warrior => swordIcon,
                CharacterRole.Archer => bowIcon,
                _ => spearIcon
            });
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Dismiss();
        else
            view.Refresh(gameManager.instance);
    }

    private void SelectGear(int tier, bool shield)
    {
        if (view == null || !view.gameObject.activeSelf || visitor == null || visitor.IsDead || gameManager.instance == null)
            return;

        CharacterRole role = visitor.Role;
        gameManager wallet = gameManager.instance;
        if (shield && role != CharacterRole.Warrior)
            return;

        string name = shield ? ShieldShopCatalog.Name(tier) : WeaponShopCatalog.Name(role, tier);
        bool owned = shield ? wallet.OwnsShield(tier) : wallet.OwnsWeapon(role, tier);
        bool success = owned
            ? (shield ? wallet.EquipShield(tier) : wallet.EquipWeapon(role, tier))
            : (shield ? wallet.TryBuyShield(tier) : wallet.TryBuyWeapon(role, tier));
        view.ShowMessage(success
            ? (owned ? $"Equipped {name}." : $"Purchased and equipped {name}.")
            : "You need both coins and meat.");
        view.Refresh(wallet);
    }

    private void Dismiss()
    {
        dismissed = true;
        Close();
    }

    private void Close()
    {
        if (openStall == this)
            openStall = null;

        if (view != null && view.gameObject.activeSelf)
            view.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Close();
        visitor = null;
        dismissed = false;
    }
}
