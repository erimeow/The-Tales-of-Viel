using UnityEngine;

public enum CharacterRole { Warrior, Archer, Spear }

[CreateAssetMenu(menuName = "Shop/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public CharacterRole role;
    public int damage;
    public int coinCost;
    public int meatCost;
}
internal static class WeaponShopCatalog
{
    private static readonly (int Damage, int Coins, int Meat)[] Tiers =
    {
        (0, 0, 0),
        (15, 4, 2),
        (30, 9, 4)
    };

    public static int Count => Tiers.Length;

    public static string Category(CharacterRole role) => role switch
    {
        CharacterRole.Warrior => "Sword",
        CharacterRole.Archer => "Bow",
        _ => "Spear"
    };

    public static string Name(CharacterRole role, int tier)
    {
        string grade = tier switch
        {
            0 => "Basic",
            1 => role == CharacterRole.Archer ? "Hunter" : "Iron",
            _ => "Vielguard"
        };
        return $"{grade} {Category(role)}";
    }

    public static string ArtPath(CharacterRole role, int tier, string asset) =>
        $"Weapons/{role}/{tier}/{asset}";

    public static int Damage(int tier) => Tiers[tier].Damage;
    public static int Coins(int tier) => Tiers[tier].Coins;
    public static int Meat(int tier) => Tiers[tier].Meat;
}

internal static class ShieldShopCatalog
{
    private static readonly (string Name, int Defense, int Coins, int Meat)[] Tiers =
    {
        ("Basic Shield", 0, 0, 0),
        ("Iron Shield", 2, 2, 1),
        ("Vielguard Shield", 4, 4, 2)
    };

    public static int Count => Tiers.Length;
    public static string Name(int tier) => Tiers[tier].Name;
    public static int Defense(int tier) => Tiers[tier].Defense;
    public static int Coins(int tier) => Tiers[tier].Coins;
    public static int Meat(int tier) => Tiers[tier].Meat;
    public static string ArtPath(int tier, string asset) => $"Shields/{tier}/{asset}";
}
