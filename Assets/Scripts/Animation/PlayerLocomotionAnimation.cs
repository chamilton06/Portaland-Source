using UnityEngine;

// Reads movement after PlayerMovement has moved the controller. No movement input lives here.
[RequireComponent(typeof(CharacterController), typeof(PlayerMovement))]
public class PlayerLocomotionAnimation : MonoBehaviour
{
    public Animator characterAnimator;
    [Min(0f)] public float blendTime = 0.12f;

    private CharacterController controller;
    private PlayerMovement movement;
    private PlayerStats stats;
    private static readonly int Speed = Animator.StringToHash("Speed");

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
        if (characterAnimator != null) characterAnimator.applyRootMotion = false;
    }

    void Update()
    {
        if (characterAnimator == null) return;
        bool canAnimate = movement.isActiveAndEnabled && (stats == null || !stats.IsDead());
        characterAnimator.speed = canAnimate ? 1f : 0f;
        // Wall collisions reduce actual speed, so pushing a wall won't play a full sprint.
        float speed = canAnimate ? Vector3.ProjectOnPlane(controller.velocity, Vector3.up).magnitude : 0f;
        characterAnimator.SetFloat(Speed, speed, blendTime, Time.deltaTime);
    }

    void OnDisable()
    {
        if (characterAnimator == null) return;
        characterAnimator.SetFloat(Speed, 0f);
        characterAnimator.speed = 0f;
    }
}
