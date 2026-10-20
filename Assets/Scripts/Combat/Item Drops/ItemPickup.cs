using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemPickup : MonoBehaviour
{
    public Item item;   // optional, goes in the journal

    void OnTriggerEnter(Collider other)
    {
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        if (OnPickup(stats))
        {
            Debug.Log("Picked up " + gameObject.name);

            if (item != null)
            {
                JournalUI journal = FindFirstObjectByType<JournalUI>();
                if (journal != null)
                {
                    journal.Discover(item);
                }
            }

            Destroy(gameObject);
        }
    }

    // return true to use up the pickup
    protected abstract bool OnPickup(PlayerStats player);
}
