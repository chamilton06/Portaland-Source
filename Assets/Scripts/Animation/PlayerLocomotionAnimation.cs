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
    private float timeInAir;
    private bool hasCrouchParameters;

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
            foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
            {
                if (parameter.nameHash == Crouching && parameter.type == AnimatorControllerParameterType.Bool) hasCrouching = true;
                if (parameter.nameHash == CrouchSpeed && parameter.type == AnimatorControllerParameterType.Float) hasCrouchSpeed = true;
            }
            hasCrouchParameters = hasCrouching && hasCrouchSpeed;
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
        if (hasCrouchParameters)
        {
            characterAnimator.SetBool(Crouching, movement.IsCrouching);
            characterAnimator.SetFloat(CrouchSpeed, speed / Mathf.Max(0.01f, movement.crouchSpeed), blendTime, Time.deltaTime);
        }
    }

    void OnDisable()
    {
        if (characterAnimator == null) return;
        characterAnimator.SetFloat(Speed, 0f);
        characterAnimator.SetFloat(VerticalSpeed, 0f);
        characterAnimator.speed = 0f;
    }
}
