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
    private GameObject weaponInstance;
    private Quaternion restingRotation;
    private Coroutine swingRoutine;
    private PlayerStats playerStats;

    public bool IsAttacking => swinging;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (weaponPivot != null)
            restingRotation = weaponPivot.localRotation;
    }

    private void Start()
    {
        SetEquippedWeapon(equippedWeapon);
    }

    public void CancelAttack()
    {
        if (swingRoutine != null)
            StopCoroutine(swingRoutine);

        swingRoutine = null;
        swinging = false;
        if (weaponPivot != null)
            weaponPivot.localRotation = restingRotation;
    }

    private void OnDisable()
    {
        CancelAttack();
    }

    // ------------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------------

    // Sets the active weapon and updates weaponEquipped to match.
    public void SetEquippedWeapon(Weapon weapon)
    {
        CancelAttack();
        if (weaponInstance != null)
        {
            weaponInstance.SetActive(false);
            Destroy(weaponInstance);
            weaponInstance = null;
        }

        equippedWeapon = weapon;
        weaponEquipped = equippedWeapon != null;

        if (!weaponEquipped || weaponPivot == null) return;
        if (weapon.weaponPrefab == null)
        {
            Debug.LogWarning("Equipped weapon has no Weapon Prefab assigned.", this);
            return;
        }

        weaponInstance = Instantiate(weapon.weaponPrefab, weaponPivot, false);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
        weaponInstance.SetActive(true);
    }

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Checks for attack input; on click, starts the swing and damages the target in range.
    void Update()
    {
        if (inventoryOpen || (playerStats != null && playerStats.IsDead()))
        {
            CancelAttack();
            return;
        }

        if (!weaponEquipped || equippedWeapon == null || equippedWeapon.stats == null
            || weaponPivot == null || weaponInstance == null || swinging)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + equippedWeapon.stats.attackCooldown;
            
            swingRoutine = StartCoroutine(SwingWeapon());
            
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
        Quaternion startRotation = restingRotation;
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
        swingRoutine = null;
    }
}
