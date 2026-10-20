using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemPickup : MonoBehaviour
{
    // Start is called before the first frame update
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

    protected abstract bool OnPickup(PlayerStats player);
}
