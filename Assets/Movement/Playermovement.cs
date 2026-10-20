using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wasd move, shift sprint, space jump, ctrl crouch
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // movement
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;

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

    // state for other scripts to read
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    private CharacterController controller;
    private PlayerStats stats;        // limits sprint/jump by stamina
    private float verticalVelocity;
    private Vector3 airVelocity;      // momentum kept in the air
    private float airSpeedCap;        // max speed in the air
    private bool crouchToggled;
    private float baseScaleY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        baseScaleY = transform.localScale.y;
    }

    void Update()
    {
        IsGrounded = controller.isGrounded;

        // keep player pushed to ground so grounded check works
        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // input
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

        // horizontal movement
        Vector3 horizontal;

        if (IsGrounded)
        {
            horizontal = input * speed;

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

        // gravity
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = horizontal + Vector3.up * verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

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