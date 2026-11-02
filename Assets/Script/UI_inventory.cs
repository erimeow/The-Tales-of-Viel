using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_inventory : MonoBehaviour
{
    private Inventory inventory;

    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private Transform itemSlotTemplate;

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnItemListChanged;

        RefreshInventoryItems();
    }

    private void Inventory_OnItemListChanged(object sender, EventArgs e)
    {
        RefreshInventoryItems();
    }

    private void RefreshInventoryItems()
    {
        if (itemSlotContainer == null)
        {
            Debug.LogError("itemSlotContainer is not assigned!");
            return;
        }

        if (itemSlotTemplate == null)
        {
            Debug.LogError("itemSlotTemplate is not assigned!");
            return;
        }

        foreach (Transform child in itemSlotContainer)
        {
            if (child == itemSlotTemplate)
                continue;

            Destroy(child.gameObject);
        }

        int x = 0;
        int y = 0;
        float itemSlotCellSize = 30f;

        foreach (Item item in inventory.GetItems())
        {
            RectTransform itemSlotRectTransform =
                Instantiate(itemSlotTemplate, itemSlotContainer)
                .GetComponent<RectTransform>();

            itemSlotRectTransform.gameObject.SetActive(true);

            itemSlotRectTransform.anchoredPosition =
                new Vector2(
                    x * itemSlotCellSize,
                    y * itemSlotCellSize
                );

            Transform imageTransform =
                itemSlotRectTransform.Find("image");

            if (imageTransform == null)
            {
                Debug.LogError("image was not found!");
                continue;
            }

            Image image = imageTransform.GetComponent<Image>();

            if (image == null)
            {
                Debug.LogError("Image component was not found!");
                continue;
            }

            image.sprite = item.GetSprite();

            x++;

            if (x > 4)
            {
                x = 0;
                y++;
            }
        }
    }
}