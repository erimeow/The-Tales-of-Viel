using UnityEngine;

using UnityEngine.Rendering.Universal;

public class ItemWorld : MonoBehaviour
{
    public static ItemWorld SpawnItemWorld(Vector3 position, Item item)
    {
        Transform transform = Instantiate(
            ItemAsset.Instance.pfItemWorld,
            position,
            Quaternion.identity
        );

        ItemWorld itemWorld = transform.GetComponent<ItemWorld>();
        itemWorld.SetItem(item);

        return itemWorld;
    }

    private Item item;
    private SpriteRenderer spriteRenderer;
    private Light2D light2d;

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        light2d = GetComponent<Light2D>();
    }

    public void SetItem(Item item)
    {
        this.item = item;
        spriteRenderer.sprite = item.GetSprite();
        light2d.color = item.GetColor();
        
    }
    public Item GetItem()
    {
        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}