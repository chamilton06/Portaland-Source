using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public Weapon equippedWeapon;
    public Transform weaponPivot; // Live scene pivot containing the visible weapon model
    public bool weaponEquipped = true;
    public bool inventoryOpen = false;

    [Header("Guard Pose (block and parry)")]
    public Vector3 guardPositionOffset = new Vector3(-0.25f, 0.25f, 0.15f);
    public Vector3 guardRotationOffset = new Vector3(-20f, 0f, 65f);
    [Min(0.01f)] public float guardTransitionTime = 0.12f;

    [Header("Hand-held weapons (optional)")]
    [Tooltip("Turned on by WeaponHandSocket. The hand bone moves the weapon, so the code swing and guard pose are skipped.")]
    public bool weaponFollowsHand = false;
    [Tooltip("Models to hold instead of a weapon's Weapon Prefab. Leave empty to use the normal prefabs.")]
    public HeldWeaponModel[] heldWeaponModels;
    [Tooltip("How long an attack lasts when the weapon is in the hand. Roughly match the attack animation.")]
    [Min(0.05f)] public float handAttackDuration = 0.6f;
    [Tooltip("Seconds into the attack animation before the blade connects and damage lands.")]
    [Min(0f)] public float handHitDelay = 0.18f;

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
        if (weaponPivot != null && !weaponFollowsHand)
        {
            weaponPivot.localRotation = restingRotation;
            weaponPivot.localPosition = restingPosition;
        }
    }

    private void OnDisable()
    {
        CancelAttack();
    }

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

        HeldWeaponModel held = FindHeldModel(weapon);
        GameObject prefab = held != null ? held.model : weapon.weaponPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("Equipped weapon has no Weapon Prefab assigned.", this);
            return;
        }

        weaponInstance = Instantiate(prefab, weaponPivot, false);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
        if (held != null) held.FitToGrip(weaponInstance.transform, weaponPivot);
        weaponInstance.SetActive(true);
    }

    // Returns the hand-held model set up for this weapon, or null to use its normal prefab.
    private HeldWeaponModel FindHeldModel(Weapon weapon)
    {
        if (heldWeaponModels == null) return null;
        foreach (HeldWeaponModel held in heldWeaponModels)
            if (held != null && held.weapon == weapon && held.model != null) return held;
        return null;
    }

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

            // With the hand animation, damage waits for the blade (see SwingWeapon).
            if (!weaponFollowsHand) DealDamage();
        }
    }

    private void DealDamage()
    {
        if (equippedWeapon == null || equippedWeapon.stats == null) return;

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

    // ------------------------------------------------------------------------
    // Coroutines
    // ------------------------------------------------------------------------

    // Combat owns the pivot pose, so guard transitions never compete with swings.
    private void LateUpdate()
    {
        if (weaponPivot == null || swinging || weaponFollowsHand) return;

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

        // In the hand, the swing keeps its timing but leaves the pose to the animation.
        // Damage lands when the blade connects; a roll or cancel before then means no hit.
        if (weaponFollowsHand)
        {
            float hitDelay = Mathf.Min(handHitDelay, handAttackDuration);
            yield return new WaitForSeconds(hitDelay);
            DealDamage();
            yield return new WaitForSeconds(handAttackDuration - hitDelay);
            swinging = false;
            swingRoutine = null;
            yield break;
        }

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

