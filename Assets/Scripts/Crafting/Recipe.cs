using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe")]
public class Recipe : ScriptableObject
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
    public bool startsKnown = true;   // off = has to be learned first
}