using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class FoodShop : MonoBehaviour
{
    private BoxCollider2D approach;
    private FoodMenu visitor;
    private bool entered;

    private void Awake() => approach = GetComponent<BoxCollider2D>();

    private void Update()
    {
        var player = PlayerWeaponLoadout.Active;
        if (player == null || player.Food == null || !player.Overlaps(approach, entered ? .12f : 0f))
        {
            Close();
            return;
        }

        if (!entered && player.Food.OpenMarket())
        {
            visitor = player.Food;
            entered = true;
        }
    }

    private void OnDisable() => Close();

    private void Close()
    {
        if (visitor != null)
            visitor.CloseMarket();
        visitor = null;
        entered = false;
    }
}
