using UnityEngine;

public class Item
{
    public enum ItemType
    {
        Sword,
        HealthPotion,
        ManaPotion,
        Coin,
        Medkit
    }

    public ItemType itemType;
    public int amount;

    public Sprite GetSprite()
    {
        if (ItemAsset.Instance == null)
        {
            Debug.LogError("ItemAsset.Instance is NULL!");
            return null;
        }

        switch (itemType)
        {
            case ItemType.HealthPotion:

                if (ItemAsset.Instance.healthPotionSprite == null)
                {
                    Debug.LogError("Health Potion Sprite is NULL!");
                }

                return ItemAsset.Instance.healthPotionSprite;

            case ItemType.Coin:

                if (ItemAsset.Instance.coinSprite == null)
                {
                    Debug.LogError("Coin Sprite is NULL!");
                }

                return ItemAsset.Instance.coinSprite;

            case ItemType.Medkit:

                if (ItemAsset.Instance.medkitSprite == null)
                {
                    Debug.LogError("Medkit Sprite is NULL!");
                }

                return ItemAsset.Instance.medkitSprite;

            default:
                Debug.LogError("No sprite assigned for: " + itemType);
                return null;
        }
    }

    public Color GetColor()
    {
        switch (itemType)
        {
            case ItemType.HealthPotion:
                return Color.red;

            case ItemType.Coin:
                return Color.yellow;

            case ItemType.Medkit:
                return Color.blue;

            default:
                return Color.white;
        }
    }
}