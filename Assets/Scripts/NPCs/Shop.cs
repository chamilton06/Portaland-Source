// ============================================================================
// Shop.cs
// Reusable Shop script
//
// To use, right-click any NPC and select Create -> New Shop.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
 
[Serializable]
public class ItemAmount
{
    public Item item;
    [Min(1)] public int amount = 1;
}

// The offer given to the player
[Serializable]
public class TradeOffer
{
    [Tooltip("What the player gives. Leave empty for a free item.")]
    public List<ItemAmount> cost = new List<ItemAmount>();
 
    [Tooltip("What the player gets.")]
    public List<ItemAmount> reward = new List<ItemAmount>();
 
    [Tooltip("How many times this trade can be made. -1 = unlimited.")]
    public int stock = -1;
}
 
// The compiled Shop
[CreateAssetMenu(fileName = "Shop", menuName = "New Shop")]
public class Shop : ScriptableObject
{
    public string shopName = "Blacksmith";
    [TextArea] public string greeting = "Take a look. Fair trades only.";
    public List<TradeOffer> offers = new List<TradeOffer>();
 
    void OnValidate()
    {
        // New list entries can come in at 0; a 0-amount cost would make trades free.
        foreach (var offer in offers)
        {
            foreach (var c in offer.cost) if (c.amount < 1) c.amount = 1;
            foreach (var r in offer.reward) if (r.amount < 1) r.amount = 1;
        }
    }
}