// ============================================================================
// Health.cs
// Tracks hit points for any damageable object (player, enemies). Handles
// damage, death, and spawning an optional drop on death, and fires onDeath
// so other scripts can react.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Health : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    public int maxHealth = 100;

    public GameObject dropPrefab; //Item spawned when this object dies (e.g. enemy hunger drop).

    // ------------------------------------------------------------------------
    // State & Events
    // ------------------------------------------------------------------------

    private int currentHealth;
    public UnityEvent onDeath = new UnityEvent();
    private DamageFlash damageFlash;
// new method to flash the object when it takes damage
    private void Awake()
{
    damageFlash = GetComponent<DamageFlash>();
}

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Initializes health to maximum.
    void Start()
    {
        currentHealth = maxHealth;
    }

    // ------------------------------------------------------------------------
    // Damage Handling
    // ------------------------------------------------------------------------

    // Applies damage, and on reaching zero spawns the drop and fires onDeath.
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || damage <= 0)  // already dead or no damage, stop
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (damageFlash != null)
        {
                                
            damageFlash.Flash();
        }

        Debug.Log(gameObject.name + " health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " died");
            Debug.Log("Drop prefab: " + (dropPrefab != null ? dropPrefab.name : "NONE"));
            if (dropPrefab != null)
                Instantiate(dropPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            onDeath.Invoke();
        }
    }
}
