// ============================================================================
// ItemPickup.cs
// Abstract base class for all collectible items. Detects the player entering
// the trigger, calls the subclass's OnPickup, and destroys the item if the
// pickup succeeded.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemPickup : MonoBehaviour
{
    // Detects the player entering the trigger and attempts the pickup.
    void OnTriggerEnter(Collider other)
    {
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        if (OnPickup(stats))
        {
            Debug.Log("Picked up " + gameObject.name);
            Destroy(gameObject);
        }
            
    }

    // Applies this item's effect to the player. Return true to consume the pickup.
    protected abstract bool OnPickup(PlayerStats player);
}
