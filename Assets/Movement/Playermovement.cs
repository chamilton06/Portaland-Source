using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    [Tooltip("Multiplies forward speed when jumping while sprinting (longer jumps).")]
    public float sprintJumpSpeedBoost = 1.15f;
    [Tooltip("Multiplies jump force when jumping while sprinting (slightly higher jumps).")]
    public float sprintJumpHeightBoost = 1.1f;
    [Tooltip("How much you can steer while in the air.")]
    public float airControl = 6f;

    [Header("Crouching")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    [Tooltip("How fast the capsule shrinks/grows when crouching.")]
    public float crouchTransitionSpeed = 4f;
    [Tooltip("Off = hold to crouch, On = press to toggle.")]
    public bool toggleCrouch = false;

    [Header("Keys")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode jumpKey = KeyCode.Space;

    // read-only state
    public bool IsGrounded { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    CharacterController controller;
    float verticalVelocity;
    Vector3 airVelocity;      // horizontal momentum carried into the air
    float airSpeedCap;        // max horizontal speed while airborne
    bool crouchToggled;
    float baseScaleY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        baseScaleY = transform.localScale.y;
    }

    void Update()
    {
        IsGrounded = controller.isGrounded;

        // keep the player pressed onto the ground so isGrounded stays reliable
        if (IsGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // inpt
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = transform.right * x + transform.forward * z;
        if (input.sqrMagnitude > 1f) input.Normalize(); // no faster diagonals

        HandleCrouch();

        // sprint
        // only sprint on the ground, moving forward, and not crouched.
        // in the air we keep whatever state we took off with.
        if (IsGrounded)
            IsSprinting = Input.GetKey(sprintKey) && z > 0f && !IsCrouching;

        float speed = IsCrouching ? crouchSpeed : (IsSprinting ? sprintSpeed : walkSpeed);

        // horz
        Vector3 horizontal;

        if (IsGrounded)
        {
            horizontal = input * speed;

            //jmp sprint
            if (Input.GetKeyDown(jumpKey) && !IsCrouching)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (IsSprinting)
                {
                    verticalVelocity *= sprintJumpHeightBoost;
                    horizontal *= sprintJumpSpeedBoost;
                }
            }

            
            airVelocity = horizontal;
            airSpeedCap = Mathf.Max(horizontal.magnitude, walkSpeed);
        }
        else
        {
            
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

        // don't stand up into a ceiling
        if (!wantsCrouch && IsCrouching && !CanStandUp())
            wantsCrouch = true;

        IsCrouching = wantsCrouch;

        // shrink/grow the capsule (visual + collider together)
        float targetScaleY = IsCrouching ? baseScaleY * (crouchHeight / standHeight) : baseScaleY;
        float currentScaleY = transform.localScale.y;
        float nextScaleY = Mathf.MoveTowards(currentScaleY, targetScaleY,
                                             crouchTransitionSpeed * baseScaleY * Time.deltaTime);

        if (!Mathf.Approximately(nextScaleY, currentScaleY))
        {
            float heightChange = (nextScaleY - currentScaleY) * controller.height;
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(s.x, nextScaleY, s.z);

            // scaling happens around the centre, so lift the player when standing
            // up so the bottom of the capsule doesn't sink into the floor.
            if (heightChange > 0f)
                controller.Move(Vector3.up * heightChange * 0.5f);
        }
    }

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