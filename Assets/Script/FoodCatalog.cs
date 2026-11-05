using UnityEngine;

internal static class FoodCatalog
{
    internal sealed class Food
    {
        public readonly Item.ItemType Type;
        public readonly string Name;
        public readonly string Description;
        public readonly int Coins;
        public readonly int Meat;
        public readonly int Healing;
        private Sprite icon;
        public Sprite Icon => icon != null ? icon : icon = Resources.Load<Sprite>("Food/" + Type);

        public Food(Item.ItemType type, string name, string description, int coins, int meat, int healing)
        {
            Type = type;
            Name = name;
            Description = description;
            Coins = coins;
            Meat = meat;
            Healing = healing;
        }
    }

    public static readonly Food[] Items =
    {
        new(Item.ItemType.Apple, "Apple", "A crisp orchard apple", 2, 0, 15),
        new(Item.ItemType.Bread, "Bread", "Fresh country loaf", 4, 0, 25),
        new(Item.ItemType.CookedMeat, "Cooked Meat", "Roasted over the coals", 4, 1, 35),
        new(Item.ItemType.Stew, "Meat Stew", "A slow-cooked bowl", 6, 2, 50),
        new(Item.ItemType.SpecialMeal, "Special Meal", "The market's hearty platter", 9, 3, 75)
    };

    public static bool TryGet(Item.ItemType type, out Food food)
    {
        foreach (Food entry in Items)
        {
            if (entry.Type != type)
                continue;
            food = entry;
            return true;
        }
        food = null;
        return false;
    }
}
