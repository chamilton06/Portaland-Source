using UnityEngine;

// Reads movement after PlayerMovement has moved the controller. No movement input lives here.
[RequireComponent(typeof(CharacterController), typeof(PlayerMovement))]
public class PlayerLocomotionAnimation : MonoBehaviour
{
    public Animator characterAnimator;
    [Min(0f)] public float blendTime = 0.12f;
    [Min(0f)] public float groundGraceTime = 0.08f;

    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerStats stats;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int CrouchSpeed = Animator.StringToHash("CrouchSpeed");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveZ = Animator.StringToHash("MoveZ");
    private static readonly int CrouchX = Animator.StringToHash("CrouchX");
    private static readonly int CrouchZ = Animator.StringToHash("CrouchZ");
    private float timeInAir;
    private bool hasCrouchParameters;
    private bool hasDirectionalParameters;
    private bool hasCrouchDirections;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
        if (characterAnimator != null)
        {
            characterAnimator.applyRootMotion = false;
            bool hasCrouching = false;
            bool hasCrouchSpeed = false;
            bool hasMoveX = false, hasMoveZ = false, hasCrouchX = false, hasCrouchZ = false;
            foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
            {
                if (parameter.nameHash == Crouching && parameter.type == AnimatorControllerParameterType.Bool) hasCrouching = true;
                if (parameter.nameHash == CrouchSpeed && parameter.type == AnimatorControllerParameterType.Float) hasCrouchSpeed = true;
                if (parameter.type != AnimatorControllerParameterType.Float) continue;
                if (parameter.nameHash == MoveX) hasMoveX = true;
                if (parameter.nameHash == MoveZ) hasMoveZ = true;
                if (parameter.nameHash == CrouchX) hasCrouchX = true;
                if (parameter.nameHash == CrouchZ) hasCrouchZ = true;
            }
            hasCrouchParameters = hasCrouching && hasCrouchSpeed;
            hasDirectionalParameters = hasMoveX && hasMoveZ;
            hasCrouchDirections = hasCrouchX && hasCrouchZ;
            if (!hasCrouchParameters)
                Debug.LogWarning("Ranger crouch setup is missing. Stop Play mode and run Tools > Portaland > Prepare Ranger Movement Test, then save the test scene.", this);
        }
    }

    void Update()
    {
        if (characterAnimator == null) return;
        bool canAnimate = movement.isActiveAndEnabled && (stats == null || !stats.IsDead());
        characterAnimator.speed = canAnimate ? 1f : 0f;
        float verticalSpeed = canAnimate ? controller.velocity.y : 0f;
        timeInAir = controller.isGrounded ? 0f : timeInAir + Time.deltaTime;
        // Tiny drops on slopes shouldn't flicker into the falling pose. Jumps respond straight away.
        bool grounded = controller.isGrounded || (verticalSpeed <= 0f && timeInAir < groundGraceTime);
        characterAnimator.SetBool(Grounded, grounded);
        characterAnimator.SetFloat(VerticalSpeed, verticalSpeed);
        // Wall collisions reduce actual speed, so pushing a wall won't play a full sprint.
        float speed = canAnimate ? Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude : 0f;
        characterAnimator.SetFloat(Speed, speed, blendTime, Time.deltaTime);
        Vector3 localVelocity = canAnimate ? transform.InverseTransformDirection(
            Vector3.ProjectOnPlane(controller.velocity, Vector3.up)) : Vector3.zero;
        // These directions follow Player facing, including when the camera turns.
        // Clamp sprint to the forward edge; the forward speed tree still selects the sprint clip.
        if (hasDirectionalParameters)
            SetDirection(MoveX, MoveZ, localVelocity, Mathf.Max(0.01f, movement.walkSpeed));
        if (hasCrouchDirections)
            SetDirection(CrouchX, CrouchZ, localVelocity, Mathf.Max(0.01f, movement.crouchSpeed));
        if (hasCrouchParameters)
        {
            characterAnimator.SetBool(Crouching, movement.IsCrouching);
            characterAnimator.SetFloat(CrouchSpeed, speed / Mathf.Max(0.01f, movement.crouchSpeed), blendTime, Time.deltaTime);
        }
    }

    private void SetDirection(int xParameter, int zParameter, Vector3 velocity, float referenceSpeed)
    {
        Vector2 direction = Vector2.ClampMagnitude(new Vector2(velocity.x, velocity.z) / referenceSpeed, 1f);
        characterAnimator.SetFloat(xParameter, direction.x, blendTime, Time.deltaTime);
        characterAnimator.SetFloat(zParameter, direction.y, blendTime, Time.deltaTime);
    }

    void OnDisable()
    {
        if (characterAnimator == null) return;
        characterAnimator.SetFloat(Speed, 0f);
        characterAnimator.SetFloat(VerticalSpeed, 0f);
        if (hasDirectionalParameters) { characterAnimator.SetFloat(MoveX, 0f); characterAnimator.SetFloat(MoveZ, 0f); }
        if (hasCrouchDirections) { characterAnimator.SetFloat(CrouchX, 0f); characterAnimator.SetFloat(CrouchZ, 0f); }
        characterAnimator.speed = 0f;
    }
}
