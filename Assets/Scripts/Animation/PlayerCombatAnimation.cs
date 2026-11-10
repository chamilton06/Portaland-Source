using UnityEngine;

// Plays the attack animation whenever PlayerCombat starts an attack.
// Combat still decides when an attack happens and what it hits; this only shows it.
[RequireComponent(typeof(PlayerCombat))]
public class PlayerCombatAnimation : MonoBehaviour
{
    [Tooltip("The character model's Animator. Found automatically if left empty.")]
    public Animator characterAnimator;
    [Tooltip("Seconds to blend into each attack.")]
    [Min(0f)] public float blendIn = 0.05f;

    private const string CombatLayerName = "Combat";
    private static readonly int Attack = Animator.StringToHash("Attack");
    private PlayerCombat combat;
    private bool hasAttackParameter;
    private int combatLayer = -1;
    private bool wasAttacking;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator == null) return;

        foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
            if (parameter.nameHash == Attack && parameter.type == AnimatorControllerParameterType.Trigger)
                hasAttackParameter = true;
        combatLayer = characterAnimator.GetLayerIndex(CombatLayerName);

        if (!hasAttackParameter || combatLayer < 0)
            Debug.LogWarning("The character's Animator has no Combat layer. Run Tools > Portaland > Prepare Combat Animations.", this);
    }

    private void Update()
    {
        if (!hasAttackParameter || combatLayer < 0) return;

        // Start once at the beginning of each attack, not every frame it lasts.
        bool attacking = combat.IsAttacking;
        if (attacking && !wasAttacking) PlayAttack(combat.CurrentAttack);
        wasAttacking = attacking;
    }

    // Combo attacks jump straight to their own state; anything else uses the default Attack.
    private void PlayAttack(HandAttack attack)
    {
        int state = attack != null && !string.IsNullOrEmpty(attack.name) ? Animator.StringToHash(attack.name) : 0;
        if (state != 0 && characterAnimator.HasState(combatLayer, state))
        {
            characterAnimator.CrossFadeInFixedTime(state, blendIn, combatLayer, 0f);
            return;
        }

        if (attack != null)
            Debug.LogWarning("No Combat layer state called " + attack.name + ". Playing the default attack.", this);
        characterAnimator.SetTrigger(Attack);
    }
}
