using UnityEngine;

public enum StartingItem { Sword, Axe, HealingPotion, Apple, Duskroot, Moonpetal, Wardleaf }

// What each starting class and keepsake gives you.
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

    // Items the class starts with. PlayerCharacterLook has the actual item for each of these.
    public static (StartingItem item, int amount)[] ClassItems(StartingClass origin)
    {
        switch (origin)
        {
            case StartingClass.Knight: return new[] { (StartingItem.Sword, 1) };
            case StartingClass.Ranger: return new[] { (StartingItem.Axe, 1) };
            default: return new[] { (StartingItem.HealingPotion, 2) };
        }
    }

    public static (StartingItem item, int amount)[] KeepsakeItems(Keepsake keepsake)
    {
        switch (keepsake)
        {
            case Keepsake.HealingPotions: return new[] { (StartingItem.HealingPotion, 2) };
            case Keepsake.Apples: return new[] { (StartingItem.Apple, 5) };
            case Keepsake.Herbs: return new[] { (StartingItem.Duskroot, 1), (StartingItem.Moonpetal, 1), (StartingItem.Wardleaf, 1) };
            default: return new (StartingItem, int)[0];
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
