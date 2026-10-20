// ============================================================================
// PlayerCombat.cs
// Handles the player's melee attack: reads mouse input, enforces the weapon's
// cooldown, raycasts forward to damage whatever it hits, and plays a swing
// animation on the equipped weapon.
// ============================================================================

using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------
    public Weapon equippedWeapon;
    public Transform weaponPivot; // Live scene pivot containing the visible weapon model
    public bool weaponEquipped = true;
    public bool inventoryOpen = false;  // Set by the inventory UI; blocks attacks while open

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------
    private float nextAttackTime = 0f;
    private bool swinging = false;

    // ------------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------------

    // Sets the active weapon and updates weaponEquipped to match.
    public void SetEquippedWeapon(Weapon weapon)
    {
        equippedWeapon = weapon;
        weaponEquipped = equippedWeapon != null;
    }

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Checks for attack input; on click, starts the swing and damages the target in range.
    void Update()
    {
        if (!weaponEquipped || inventoryOpen|| equippedWeapon == null || swinging)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + equippedWeapon.stats.attackCooldown;
            
            StartCoroutine(SwingWeapon());
            
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, equippedWeapon.stats.attackRange))
            {
                Health targetHealth = hit.collider.GetComponentInParent<Health>();

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(equippedWeapon.stats.damage);
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // Coroutines
    // ------------------------------------------------------------------------

    // Rotates the weapon through a quick swing and back, blocking attacks until it finishes.
    IEnumerator SwingWeapon()
    {
        if (weaponPivot == null)
        {
            Debug.LogWarning("Assign Weapon Pivot on PlayerCombat.", this);
            yield break;
        }

        swinging = true;

        Transform weapon = weaponPivot;
        Quaternion startRotation = weapon.localRotation;
        Quaternion swingRotation = startRotation * Quaternion.Euler(60f, 0f, -40f);

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            weapon.localRotation = Quaternion.Slerp(
                startRotation, swingRotation, Mathf.Clamp01(elapsed / duration));

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            weapon.localRotation = Quaternion.Slerp(
                swingRotation, startRotation, Mathf.Clamp01(elapsed / duration));

            yield return null;
        }

        weapon.localRotation = startRotation;
        swinging = false;
    }
}
