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

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------

    // state for other scripts to read
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

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
    }

    // Reads input and applies movement, jumping, gravity, and fall damage each frame.
    void Update()
    {
        // --- Ground check & fall damage ---
        IsGrounded = controller.isGrounded;

        if (IsGrounded && !wasGrounded && stats != null)
            stats.ApplyFallDamage(-verticalVelocity);
        wasGrounded = IsGrounded;

        // keep player pushed to ground so grounded check works
        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // --- Input ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = transform.right * x + transform.forward * z;
        if (input.sqrMagnitude > 1f) input.Normalize(); // no faster diagonals

        HandleCrouch();

        // sprint only on ground, moving forward, not crouched
        // in the air keep the state from takeoff
        if (IsGrounded)
            IsSprinting = Input.GetKey(sprintKey) && z > 0f && !IsCrouching
                          && (stats == null || stats.CanSprint());

        float speed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);

        // --- Horizontal movement (ground / air) ---
        Vector3 horizontal;

        if (IsGrounded)
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