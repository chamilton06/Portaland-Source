using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
public class Recipe : JournalEntry
{
    [System.Serializable]
    public class Ingredient
    {
        public Item item;
        public int amount = 1;
    }

    public Item result;
    public int resultAmount = 1;
    public List<Ingredient> ingredients = new List<Ingredient>();

    // recipes always live in the crafting tab
    void OnEnable()
    {
        tab = JournalTab.Crafting;
        if (picture == null && result != null) picture = result.icon;   // recipe page shows what it makes
    }

    // list shows the result name if no title was typed
    public override string GetDisplayTitle()
    {
        return string.IsNullOrEmpty(title) && result != null ? result.itemName : title;
    }
}