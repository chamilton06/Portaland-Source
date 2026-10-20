// ============================================================================
// FoodPickup.cs
// Pickup item that restores hunger to the player when collected.
// Inherits pickup/collection behavior from ItemPickup; this class only
// defines what happens on collection (feeding the player).
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPickup : ItemPickup
{
    // How much hunger gets restored by this item
    public float hungerAmount = 25f;

    // Feeds the player when this item is picked up.
    protected override bool OnPickup(PlayerStats player)
    {
        player.Eat(hungerAmount);
        return true;
    }
}
