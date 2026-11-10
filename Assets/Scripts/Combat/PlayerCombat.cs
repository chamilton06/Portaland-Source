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
    [Tooltip("Tap left click: quick attacks that chain in order if you keep tapping. Leave empty to use the single attack above.")]
    public HandAttack[] lightAttacks;
    [Tooltip("Hold left click: a slow, strong attack. Leave its Name empty to have no heavy attack.")]
    public HandAttack heavyAttack;
    [Tooltip("Left click in the air. Leave its Name empty to use the normal attacks in the air too.")]
    public HandAttack jumpAttack;
    [Tooltip("Left click while sprinting: a lunging strike. Leave its Name empty to use the normal attacks.")]
    public HandAttack sprintAttack;
    [Tooltip("Left click while crouched. Leave its Name empty to use the normal attacks.")]
    public HandAttack crouchAttack;
    [Tooltip("Seconds left click must be held for a heavy attack instead of a light one.")]
    [Min(0.05f)] public float heavyHoldTime = 0.3f;
    [Tooltip("Seconds after a light attack ends to tap again and carry on the chain.")]
    [Min(0f)] public float comboWindow = 0.6f;

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
    private int comboStep;
    private float comboEndTime = -100f;
    private bool holdingAttack;
    private float holdStartTime;

    public bool IsAttacking => swinging;
    // The hand attack being played, or null for the single default attack.
    public HandAttack CurrentAttack { get; private set; }

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
        comboStep = 0;
        holdingAttack = false;
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

        if (defence != null && defence.IsBlocking)
        {
            holdingAttack = false;
            return;
        }

        // In the air, sprinting or crouched, left click goes straight into that attack.
        HandAttack special = weaponFollowsHand ? SituationAttack() : null;
        if (special != null)
        {
            holdingAttack = false;
            if (Input.GetMouseButtonDown(0)) StartAttack(special, true);
            return;
        }

        // In the hand with a heavy attack set: tap left click for light, hold it for heavy.
        if (weaponFollowsHand && IsSet(heavyAttack))
        {
            if (Input.GetMouseButtonDown(0))
            {
                holdingAttack = true;
                holdStartTime = Time.time;
            }
            if (!holdingAttack) return;

            if (Time.time - holdStartTime >= heavyHoldTime)
            {
                holdingAttack = false;
                StartAttack(heavyAttack, true);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                holdingAttack = false;
                StartAttack(NextLightAttack(), false);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
            StartAttack(weaponFollowsHand ? NextLightAttack() : null, false);
    }

    private static bool IsSet(HandAttack attack) => attack != null && !string.IsNullOrEmpty(attack.name);

    // The attack for what the player is doing right now, or null for the normal light/heavy.
    private HandAttack SituationAttack()
    {
        if (movement == null) return null;
        if (!movement.IsGrounded && IsSet(jumpAttack)) return jumpAttack;
        if (movement.IsSprinting && IsSet(sprintAttack)) return sprintAttack;
        if (movement.IsCrouching && IsSet(crouchAttack)) return crouchAttack;
        return null;
    }

    // Starts an attack if the cooldown is over and there's enough stamina for it.
    // Heavy and situation attacks restart the light chain; a light attack moves it on.
    private void StartAttack(HandAttack attack, bool restartsChain)
    {
        if (Time.time < nextAttackTime) return;
        float staminaCost = attack != null ? attack.staminaCost : 0f;
        if (staminaCost > 0f && playerStats != null && !playerStats.TryUseStamina(staminaCost)) return;

        nextAttackTime = Time.time + equippedWeapon.stats.attackCooldown;
        CurrentAttack = attack;

        if (restartsChain) comboStep = 0;
        else if (lightAttacks != null && lightAttacks.Length > 0) comboStep = (comboStep + 1) % lightAttacks.Length;

        swingRoutine = StartCoroutine(SwingWeapon());

        // With the hand animation, damage waits for the blade (see SwingWeapon).
        if (!weaponFollowsHand) DealDamage(1f);
    }

    // The next light attack in the chain, or the first if the last attack ended a while ago.
    private HandAttack NextLightAttack()
    {
        if (lightAttacks == null || lightAttacks.Length == 0) return null;
        if (Time.time > comboEndTime + comboWindow) comboStep = 0;
        return lightAttacks[comboStep % lightAttacks.Length];
    }

    private void DealDamage(float multiplier)
    {
        if (equippedWeapon == null || equippedWeapon.stats == null) return;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, equippedWeapon.stats.attackRange))
        {
            Health targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(Mathf.RoundToInt(equippedWeapon.stats.damage * multiplier));
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
            HandAttack attack = CurrentAttack;
            float attackDuration = attack != null ? attack.duration : handAttackDuration;
            float hitDelay = Mathf.Min(attack != null ? attack.hitDelay : handHitDelay, attackDuration);
            yield return new WaitForSeconds(hitDelay);
            DealDamage(attack != null ? attack.damageMultiplier : 1f);
            yield return new WaitForSeconds(attackDuration - hitDelay);
            comboEndTime = Time.time;
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

