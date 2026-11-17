using UnityEngine;

// counting and using up inventory items for recipes
public static class Crafting
{
    public static int Count(Inventory inventory, Item item)
    {
        int total = 0;
        foreach (Slot slot in inventory.InventorySlots)
        {
            if (slot.GetItem() == item) total += slot.GetAmount();
        }
        return total;
    }

    public static bool CanCraft(Inventory inventory, Recipe recipe)
    {
        foreach (Recipe.Ingredient ingredient in recipe.ingredients)
        {
            if (Count(inventory, ingredient.item) < ingredient.amount) return false;
        }
        return true;
    }

    public static bool Craft(Inventory inventory, Recipe recipe)
    {
        if (!CanCraft(inventory, recipe)) return false;

        foreach (Recipe.Ingredient ingredient in recipe.ingredients)
        {
            Remove(inventory, ingredient.item, ingredient.amount);
        }
        inventory.AddItem(recipe.result, recipe.resultAmount);
        return true;
    }

    // takes from the last stacks first so the front of the bag stays tidy
    static void Remove(Inventory inventory, Item item, int amount)
    {
        for (int i = inventory.InventorySlots.Count - 1; i >= 0 && amount > 0; i--)
        {
            Slot slot = inventory.InventorySlots[i];
            if (slot.GetItem() != item) continue;

            int take = Mathf.Min(amount, slot.GetAmount());
            inventory.RemoveFromSlot(slot, take);
            amount -= take;
        }
    }
}