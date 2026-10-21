using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// eats it straight away
public class FoodPickup : ItemPickup
{
    public float hungerAmount = 25f;

    protected override bool OnPickup(PlayerStats player)
    {
        player.Eat(hungerAmount);
        return true;
    }
}
