using UnityEngine;

// What each starting class and keepsake gives you. The items are loaded from Resources/Items,
// so they don't need setting up in a scene.
public static class Origins
{
    public struct Stats
    {
        public float health, stamina;
        public string description;
    }

    public static Stats For(StartingClass origin)
    {
        switch (origin)
        {
            case StartingClass.Knight:
                return new Stats { health = 120f, stamina = 90f, description = "Tough, but tires quickly. Starts with a sword." };
            case StartingClass.Ranger:
                return new Stats { health = 95f, stamina = 120f, description = "Quick and light on their feet. Starts with an axe." };
            default:
                return new Stats { health = 105f, stamina = 105f, description = "A bit of everything. Starts with two healing potions." };
        }
    }

    // Items the class starts with, as (path in Resources, amount).
    public static (string item, int amount)[] ClassItems(StartingClass origin)
    {
        switch (origin)
        {
            case StartingClass.Knight: return new[] { ("Items/Sword", 1) };
            case StartingClass.Ranger: return new[] { ("Items/Axe", 1) };
            default: return new[] { ("Items/Crafting/HealingPotion", 2) };
        }
    }

    public static (string item, int amount)[] KeepsakeItems(Keepsake keepsake)
    {
        switch (keepsake)
        {
            case Keepsake.HealingPotions: return new[] { ("Items/Crafting/HealingPotion", 2) };
            case Keepsake.Apples: return new[] { ("Items/Apple", 5) };
            case Keepsake.Herbs: return new[] { ("Items/Crafting/Duskroot", 1), ("Items/Crafting/Moonpetal", 1), ("Items/Crafting/Wardleaf", 1) };
            default: return new (string, int)[0];
        }
    }

    public static string Describe(Keepsake keepsake)
    {
        switch (keepsake)
        {
            case Keepsake.HealingPotions: return "Healing potions (2)";
            case Keepsake.Apples: return "Apples (5)";
            case Keepsake.Herbs: return "Crafting herbs (3)";
            default: return "None";
        }
    }
}
