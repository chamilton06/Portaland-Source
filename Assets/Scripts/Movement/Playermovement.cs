// ============================================================================
// PlayerMovement.cs
// First-person-style character movement: WASD move, Shift sprint, Space jump,
// Ctrl crouch. Handles ground and air acceleration, sprint jumps, fall damage,
// and crouching with a ceiling check. Sprint and jump are limited by stamina
// via PlayerStats.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wasd move, shift sprint, space jump, ctrl crouch
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    // movement
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;

    // acceleration
    public float groundAccel = 60f;
    public float groundDecel = 80f;

    // jumping
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float sprintJumpSpeedBoost = 1.15f;   // longer jump when sprinting
    public float sprintJumpHeightBoost = 1.1f;   // bit higher jump when sprinting
    public float airControl = 6f;                // steering in the air

    // crouching
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 4f;     // how fast capsule shrinks/grows
    public bool toggleCrouch = false;            // off = hold, on = press to toggle

    // keys
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Dodge Roll")]
    public KeyCode rollKey = KeyCode.Q;
    [Min(0.1f)] public float rollDuration = 0.45f;
    [Min(0f)] public float rollSpeed = 8f;
    [Min(0f)] public float rollStaminaCost = 25f;
    [Min(0f)] public float rollCooldown = 0.35f;
    [Min(0f)] public float dodgeWindowStart = 0.08f;
    [Min(0f)] public float dodgeWindowEnd = 0.28f;
    public Transform rollVisual; // Visual only; never assign Player or its weapon pivot.
    public bool createCapsuleRollVisual = true;

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------

    // state for other scripts to read
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    public bool IsRolling { get; private set; }
    public bool IsInDodgeWindow => isActiveAndEnabled && IsRolling
        && (stats == null || !stats.IsDead())
        && Time.time - rollStartedAt >= dodgeWindowStart
        && Time.time - rollStartedAt <= Mathf.Min(dodgeWindowEnd, rollDuration);

    private PlayerCombat combat;
    private PlayerDefence defence;
    private Vector3 rollDirection;
    private float rollStartedAt;
    private float nextRollAt;
    private Quaternion visualRestRotation;
    private CharacterController controller;
    private PlayerStats stats;        // limits sprint/jump by stamina
    private float verticalVelocity;
    private Vector3 airVelocity;      // momentum kept in the air
    private float airSpeedCap;        // max speed in the air
    private bool wasGrounded;
    private bool crouchToggled;
    private float baseScaleY;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Caches the CharacterController and PlayerStats and records the starting height scale.
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        baseScaleY = transform.localScale.y;
        combat = GetComponent<PlayerCombat>();
        defence = GetComponent<PlayerDefence>();
    }

    private void Start()
    {
        // The prototype capsule mesh lives on Player itself. Copy only its visual
        // components to a child; leave the controller, camera and weapon upright.
        if (rollVisual == null && createCapsuleRollVisual)
        {
            MeshFilter mesh = GetComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (mesh != null && renderer != null && renderer.enabled)
            {
                GameObject visual = new GameObject("RollVisual");
                visual.layer = gameObject.layer;
                visual.transform.SetParent(transform, false);
                visual.AddComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
                MeshRenderer copy = visual.AddComponent<MeshRenderer>();
                copy.sharedMaterials = renderer.sharedMaterials;
                copy.shadowCastingMode = renderer.shadowCastingMode;
                copy.receiveShadows = renderer.receiveShadows;
                renderer.enabled = false;
                rollVisual = visual.transform;
            }
        }
        if (rollVisual == transform || (rollVisual != null && !rollVisual.IsChildOf(transform)))
        {
            Debug.LogWarning("Roll Visual must be a visual child of Player.", this);
            rollVisual = null;
        }
        if (rollVisual != null) visualRestRotation = rollVisual.localRotation;
    }

    // Reads input and applies movement, jumping, gravity, and fall damage each frame.
    void Update()
    {
        // --- Ground check & fall damage ---
        IsGrounded = controller.isGrounded;

        if (IsGrounded && !wasGrounded && stats != null)
            stats.ApplyFallDamage(-verticalVelocity);
        wasGrounded = IsGrounded;
        if (stats != null && stats.IsDead()) return;

        // keep player pushed to ground so grounded check works
        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // --- Input ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = transform.right * x + transform.forward * z;
        if (input.sqrMagnitude > 1f) input.Normalize(); // no faster diagonals

        if (IsRolling && (Time.time - rollStartedAt >= rollDuration
            || (combat != null && combat.inventoryOpen)))
            EndRoll();

        if (!IsRolling) HandleCrouch();

        if (Input.GetKeyDown(rollKey) && !IsRolling && IsGrounded && !IsCrouching
            && !Input.GetKey(crouchKey) && Time.time >= nextRollAt
            && Mathf.Abs(transform.localScale.y - baseScaleY) < 0.01f
            && (combat == null || (!combat.inventoryOpen && !combat.IsAttacking))
            && (stats == null || stats.TryUseStamina(rollStaminaCost)))
        {
            IsRolling = true;
            rollStartedAt = Time.time;
            rollDirection = Vector3.ProjectOnPlane(input.sqrMagnitude > 0.001f
                ? input : transform.forward, Vector3.up).normalized;
            IsSprinting = false;
            if (defence != null) defence.CancelGuard();
            if (combat != null) combat.CancelAttack();
        }

        // sprint only on ground, moving forward, not crouched
        // in the air keep the state from takeoff
        if (IsGrounded && !IsRolling)
            IsSprinting = Input.GetKey(sprintKey) && z > 0f && !IsCrouching
                          && (stats == null || stats.CanSprint());

        float speed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);

        // --- Horizontal movement (ground / air) ---
        Vector3 horizontal;

        if (IsRolling)
        {
            IsSprinting = false;
            horizontal = rollDirection * rollSpeed;
            // Do not carry boosted roll speed into normal airborne movement.
            airVelocity = rollDirection * walkSpeed;
            airSpeedCap = walkSpeed;
        }
        else if (IsGrounded)
        {
            // Horizontal changes with acceleration/deceleration
            horizontal = Vector3.MoveTowards(airVelocity, input * speed, groundAccel * Time.deltaTime);

            // jump, uses stamina
            if (Input.GetKeyDown(jumpKey) && !IsCrouching
                && (stats == null || stats.TryUseJumpStamina()))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // sprint jump
                if (IsSprinting)
                {
                    verticalVelocity *= sprintJumpHeightBoost;
                    horizontal *= sprintJumpSpeedBoost;
                }
            }

            // save momentum in case we leave the ground
            airVelocity = horizontal;
            airSpeedCap = Mathf.Max(horizontal.magnitude, walkSpeed);
        }
        else
        {
            // in the air, keep momentum, little steering, no speeding up
            airVelocity += input * airControl * Time.deltaTime;
            airVelocity = Vector3.ClampMagnitude(airVelocity, airSpeedCap);
            horizontal = airVelocity;
        }

        // --- Gravity & apply movement ---
        float g = gravity;
        if (verticalVelocity < 0f) g *= 1.5f;
        else if (!Input.GetKey(jumpKey)) g *= 2f;
        verticalVelocity += g * Time.deltaTime;

        Vector3 move = horizontal + Vector3.up * verticalVelocity;

        var flags = controller.Move(move * Time.deltaTime);

        // --- Collision responses ---
        // hit head on ceiling
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;

        // hit wall midair
        if ((flags & CollisionFlags.Sides) != 0 && !IsGrounded)
            airVelocity = Vector3.ProjectOnPlane(controller.velocity, Vector3.up);
    }

    private void LateUpdate()
    {
        if (!IsRolling || rollVisual == null) return;
        float progress = Mathf.Clamp01((Time.time - rollStartedAt) / rollDuration);
        Vector3 worldAxis = Vector3.Cross(Vector3.up, rollDirection);
        Vector3 localAxis = transform.InverseTransformDirection(worldAxis);
        rollVisual.localRotation = Quaternion.AngleAxis(progress * 360f, localAxis)
            * visualRestRotation;
    }

    private void EndRoll()
    {
        if (IsRolling) nextRollAt = Time.time + rollCooldown;
        IsRolling = false;
        if (rollVisual != null) rollVisual.localRotation = visualRestRotation;
    }

    private void OnDisable()
    {
        EndRoll();
        IsSprinting = false;
    }

    // ------------------------------------------------------------------------
    // Crouching
    // ------------------------------------------------------------------------

    // Updates crouch state from input and smoothly resizes the capsule.
    void HandleCrouch()
    {
        bool wantsCrouch;
        if (toggleCrouch)
        {
            if (Input.GetKeyDown(crouchKey)) crouchToggled = !crouchToggled;
            wantsCrouch = crouchToggled;
        }
        else
        {
            wantsCrouch = Input.GetKey(crouchKey);
        }

        // dont stand up into a ceiling
        if (!wantsCrouch && IsCrouching && !CanStandUp())
            wantsCrouch = true;

        IsCrouching = wantsCrouch;

        // shrink/grow capsule
        float targetScaleY = IsCrouching ? baseScaleY * (crouchHeight / standHeight) : baseScaleY;
        float currentScaleY = transform.localScale.y;
        float nextScaleY = Mathf.MoveTowards(currentScaleY, targetScaleY,
                                             crouchTransitionSpeed * baseScaleY * Time.deltaTime);

        if (!Mathf.Approximately(nextScaleY, currentScaleY))
        {
            float heightChange = (nextScaleY - currentScaleY) * controller.height;
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(s.x, nextScaleY, s.z);

            // lift player when standing so they dont sink into floor
            if (heightChange > 0f)
                controller.Move(Vector3.up * heightChange * 0.5f);
        }
    }

    // checks if theres space above to stand
    bool CanStandUp()
    {
        float radius = controller.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        float currentHeight = controller.height * transform.localScale.y;
        float fullHeight = controller.height * baseScaleY;

        Vector3 top = transform.position + Vector3.up * (currentHeight * 0.5f - radius);
        float distance = (fullHeight - currentHeight) + controller.skinWidth;

        return !Physics.SphereCast(top, radius * 0.95f, Vector3.up, out _, distance,
                                   ~0, QueryTriggerInteraction.Ignore);
    }
}