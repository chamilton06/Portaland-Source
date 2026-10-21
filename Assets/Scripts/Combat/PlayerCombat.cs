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

    [Header("Guard Pose (block and parry)")]
    public Vector3 guardPositionOffset = new Vector3(-0.25f, 0.25f, 0.15f);
    public Vector3 guardRotationOffset = new Vector3(-20f, 0f, 65f);
    [Min(0.01f)] public float guardTransitionTime = 0.12f;

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------
    private float nextAttackTime = 0f;
    private bool swinging = false;
    private GameObject weaponInstance;
    private Quaternion restingRotation;
    private Vector3 restingPosition;
    private float guardPoseWeight;
    private Coroutine swingRoutine;
    private PlayerStats playerStats;
    private PlayerDefence defence;
    private PlayerMovement movement;

    public bool IsAttacking => swinging;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
        defence = GetComponent<PlayerDefence>();
        if (weaponPivot != null)
        {
            restingRotation = weaponPivot.localRotation;
            restingPosition = weaponPivot.localPosition;
        }
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
        guardPoseWeight = 0f;
        if (weaponPivot != null)
        {
            weaponPivot.localRotation = restingRotation;
            weaponPivot.localPosition = restingPosition;
        }
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
        if (defence != null) defence.CancelGuard();
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
        if ((movement != null && movement.IsRolling) || inventoryOpen || (playerStats != null && playerStats.IsDead()))
        {
            CancelAttack();
            return;
        }

        if (!weaponEquipped || equippedWeapon == null || equippedWeapon.stats == null
            || weaponPivot == null || weaponInstance == null || swinging)
        {
            return;
        }

        if (defence != null && defence.IsBlocking) return;

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

    // Combat owns the pivot pose, so guard transitions never compete with swings.
    private void LateUpdate()
    {
        if (weaponPivot == null || swinging) return;

        bool guarding = defence != null && defence.IsBlocking;
        guardPoseWeight = Mathf.MoveTowards(guardPoseWeight, guarding ? 1f : 0f,
            Time.deltaTime / Mathf.Max(0.01f, guardTransitionTime));

        weaponPivot.localPosition = Vector3.Lerp(restingPosition,
            restingPosition + guardPositionOffset, guardPoseWeight);
        weaponPivot.localRotation = Quaternion.Slerp(restingRotation,
            restingRotation * Quaternion.Euler(guardRotationOffset), guardPoseWeight);
    }

    // Rotates the weapon through a quick swing and back, blocking attacks until it finishes.
    IEnumerator SwingWeapon()
    {
        if (weaponPivot == null)
        {
            Debug.LogWarning("Assign Weapon Pivot on PlayerCombat.", this);
            yield break;
        }

        swinging = true;
        guardPoseWeight = 0f;
        weaponPivot.localPosition = restingPosition;

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
