using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// RIGIDBODY VERSION
// Walk (WASD/arrows), Sprint (Left Shift), Jump (Space), Crouch (C toggle), Eat (E).
// Input is read in Update, physics is applied in FixedUpdate.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float turnSpeed = 10f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;

    [Header("Ground Check")]
    public LayerMask groundMask = ~0;          // "Everything" by default
    public float groundCheckDistance = 0.15f;

    [Header("Crouch")]
    public float standingHeight = 2f;
    public float crouchHeight = 1f;
    public Transform body;   

    // Read by the UI to show what the player is doing
    public string CurrentState { get; private set; } = "Idle";

    private Rigidbody rb;
    private CapsuleCollider col;
    private PlayerStats stats;

    private Vector3 moveDir;
    private float currentSpeed;
    private bool jumpRequested;
    private bool isCrouching;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        stats = GetComponent<PlayerStats>();

        rb.freezeRotation = true;                                   // stop it falling over
        rb.interpolation = RigidbodyInterpolation.Interpolate;      // smooth camera follow
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        isGrounded = CheckGrounded();

        // --- Input ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveDir = new Vector3(x, 0f, z).normalized;
        bool moving = moveDir.sqrMagnitude > 0.01f;

        if (Input.GetKeyDown(KeyCode.C)) SetCrouch(!isCrouching);
        if (Input.GetKeyDown(KeyCode.E)) stats.Eat();
        if (Input.GetButtonDown("Jump")) jumpRequested = true;

        // --- Sprint ---
        bool sprinting = Input.GetKey(KeyCode.LeftShift) && moving && isGrounded
                         && !isCrouching && !stats.IsExhausted && !stats.IsStarving;

        currentSpeed = isCrouching ? crouchSpeed : (sprinting ? sprintSpeed : walkSpeed);
        if (stats.IsStarving) currentSpeed *= 0.5f;   // starving = slow
        if (sprinting) stats.DrainStamina(stats.staminaDrainPerSecond * Time.deltaTime);

        // --- State for UI ---
        if (!isGrounded) CurrentState = "Jumping";
        else if (isCrouching) CurrentState = moving ? "Crouch Walking" : "Crouching";
        else if (sprinting) CurrentState = "Sprinting";
        else if (moving) CurrentState = "Walking";
        else CurrentState = "Idle";
    }

    void FixedUpdate()
    {
        // --- Horizontal movement (keep current vertical speed for gravity/jumps) ---
        Vector3 v = moveDir * currentSpeed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);

        // --- Face the move direction ---
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, turnSpeed * Time.fixedDeltaTime));
        }

        // --- Jump ---
        if (jumpRequested)
        {
            if (isGrounded && !isCrouching && stats.HasStamina(stats.jumpStaminaCost))
            {
                float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
                stats.DrainStamina(stats.jumpStaminaCost);
            }
            jumpRequested = false;
        }
    }

    bool CheckGrounded()
    {
        // Cast a small sphere down from the middle of the capsule to just below the feet
        Vector3 origin = transform.position + col.center;
        float radius = col.radius * 0.9f;
        float distance = col.height / 2f - col.radius + groundCheckDistance;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, distance,
                                  groundMask, QueryTriggerInteraction.Ignore);
    }

    void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
        float h = crouch ? crouchHeight : standingHeight;

        // Shrink the collider but keep the feet on the ground
        col.height = h;
        col.center = new Vector3(0f, -(standingHeight - h) / 2f, 0f);

        // Squash the visible capsule so you can SEE the crouch
        if (body != null)
        {
            body.localScale = new Vector3(1f, h / standingHeight, 1f);
            body.localPosition = new Vector3(0f, -(standingHeight - h) / 2f, 0f);
        }
    }
}