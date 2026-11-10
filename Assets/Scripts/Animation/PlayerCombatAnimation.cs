using UnityEngine;

// Plays the attack animation whenever PlayerCombat starts an attack.
// Combat still decides when an attack happens and what it hits; this only shows it.
[RequireComponent(typeof(PlayerCombat))]
public class PlayerCombatAnimation : MonoBehaviour
{
    [Tooltip("The character model's Animator. Found automatically if left empty.")]
    public Animator characterAnimator;

    private static readonly int Attack = Animator.StringToHash("Attack");
    private PlayerCombat combat;
    private bool hasAttackParameter;
    private bool wasAttacking;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator == null) return;

        foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
            if (parameter.nameHash == Attack && parameter.type == AnimatorControllerParameterType.Trigger)
                hasAttackParameter = true;

        if (!hasAttackParameter)
            Debug.LogWarning("The character's Animator has no Attack trigger. Run Tools > Portaland > Prepare Combat Animations.", this);
    }

    private void Update()
    {
        if (!hasAttackParameter) return;

        // Trigger once at the start of each attack, not every frame it lasts.
        bool attacking = combat.IsAttacking;
        if (attacking && !wasAttacking) characterAnimator.SetTrigger(Attack);
        wasAttacking = attacking;
    }
}
