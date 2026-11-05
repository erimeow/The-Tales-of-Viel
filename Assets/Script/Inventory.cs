using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{
    public event EventHandler OnItemListChanged;
    private List<Item> itemList;

    public Inventory(bool includeStarterItems = true)
    {
        itemList = new List<Item>();
        if (!includeStarterItems)
            return;

        
        AddItem(new Item { itemType = Item.ItemType.HealthPotion, amount = 1 });
        AddItem(new Item { itemType = Item.ItemType.Coin, amount = 1 });
        AddItem(new Item { itemType = Item.ItemType.Medkit, amount = 1 });
        
    }
    public void AddItem(Item item)
    {
        itemList.Add(item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public int Count(Item.ItemType type) => itemList.Find(item => item.itemType == type)?.amount ?? 0;

    public void AddFood(Item.ItemType type)
    {
        Item stack = itemList.Find(item => item.itemType == type);
        if (stack == null)
        {
            AddItem(new Item { itemType = type, amount = 1 });
            return;
        }

        stack.amount++;
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool ConsumeFood(Item.ItemType type, Func<bool> apply)
    {
        Item stack = itemList.Find(item => item.itemType == type);
        if (stack == null || stack.amount <= 0 || !apply())
            return false;

        if (--stack.amount == 0)
            itemList.Remove(stack);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void Clear()
    {
        itemList.Clear();
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<Item> GetItems()
    {
        return itemList;
    }

 
}
