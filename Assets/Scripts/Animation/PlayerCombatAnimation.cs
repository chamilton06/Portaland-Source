using UnityEngine;

// Plays the attack, guard, parry, guard break, hit and death animations for PlayerCombat and PlayerDefence.
// Combat and defence still decide what happens; this only shows it.
[RequireComponent(typeof(PlayerCombat))]
public class PlayerCombatAnimation : MonoBehaviour
{
    [Tooltip("The character model's Animator. Found automatically if left empty.")]
    public Animator characterAnimator;
    [Tooltip("Seconds to blend into each attack.")]
    [Min(0f)] public float blendIn = 0.05f;

    [Header("Guard")]
    [Tooltip("How far into the block animation the held guard pose is (0 = start, 1 = end).")]
    [Range(0f, 1f)] public float guardPose = 0.35f;
    [Tooltip("Seconds to raise the sword into the guard pose.")]
    [Min(0.01f)] public float guardRaiseTime = 0.12f;
    [Tooltip("Seconds the parry animation plays before going back to the guard.")]
    [Min(0.05f)] public float parryAnimationTime = 0.5f;

    [Header("Getting hit")]
    [Tooltip("Seconds the flinch plays after a hit lands, before the guard can show again.")]
    [Min(0.05f)] public float hitAnimationTime = 0.3f;

    private const string CombatLayerName = "Combat";
    private const string DeathLayerName = "Death";
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int GuardTime = Animator.StringToHash("GuardTime");
    private static readonly int EmptyState = Animator.StringToHash("Empty");
    private static readonly int GuardState = Animator.StringToHash("Guard");
    private static readonly int ParryState = Animator.StringToHash("Parry");
    private static readonly int GuardBreakState = Animator.StringToHash("Idle_Shield_Break");
    private static readonly int HitState = Animator.StringToHash("Hit_Chest");
    private static readonly int DeathState = Animator.StringToHash("Death01");

    private PlayerCombat combat;
    private PlayerDefence defence;
    private PlayerStats stats;
    private int deathLayer = -1;
    private bool dead;
    private bool hasAttackParameter;
    private bool hasGuard;
    private int combatLayer = -1;
    private bool wasAttacking;
    private bool guardShown;
    private float guardAmount;
    private float reactionEndsAt = float.NegativeInfinity;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        defence = GetComponent<PlayerDefence>();
        stats = GetComponent<PlayerStats>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();
        if (characterAnimator == null) return;

        bool hasGuardTime = false;
        foreach (AnimatorControllerParameter parameter in characterAnimator.parameters)
        {
            if (parameter.nameHash == Attack && parameter.type == AnimatorControllerParameterType.Trigger)
                hasAttackParameter = true;
            if (parameter.nameHash == GuardTime && parameter.type == AnimatorControllerParameterType.Float)
                hasGuardTime = true;
        }
        combatLayer = characterAnimator.GetLayerIndex(CombatLayerName);
        deathLayer = characterAnimator.GetLayerIndex(DeathLayerName);
        hasGuard = defence != null && hasGuardTime && combatLayer >= 0 && characterAnimator.HasState(combatLayer, GuardState);

        if (!hasAttackParameter || combatLayer < 0 || (defence != null && !hasGuard))
            Debug.LogWarning("The character's Animator is missing combat animations. Run Tools > Portaland > Prepare Combat Animations.", this);
    }

    private void OnEnable()
    {
        if (defence != null) defence.HitResolved += OnHitResolved;
        if (stats != null) stats.onDeath.AddListener(OnDeath);
    }

    private void OnDisable()
    {
        if (defence != null) defence.HitResolved -= OnHitResolved;
        if (stats != null) stats.onDeath.RemoveListener(OnDeath);
    }

    // The whole body falls on the Death layer; the scene reloads on respawn, so it never resets.
    private void OnDeath()
    {
        dead = true;
        if (deathLayer >= 0 && characterAnimator.HasState(deathLayer, DeathState))
            characterAnimator.CrossFadeInFixedTime(DeathState, 0.1f, deathLayer, 0f);
    }

    private void Update()
    {
        if (dead || !hasAttackParameter || combatLayer < 0) return;

        // Start once at the beginning of each attack, not every frame it lasts.
        bool attacking = combat.IsAttacking;
        if (attacking && !wasAttacking) PlayAttack(combat.CurrentAttack);
        wasAttacking = attacking;

        if (hasGuard && !attacking) UpdateGuard();
    }

    // Raises the sword while blocking and lowers it on release, unless a parry or stagger is playing.
    private void UpdateGuard()
    {
        bool blocking = defence.IsBlocking;
        guardAmount = Mathf.MoveTowards(guardAmount, blocking ? 1f : 0f, Time.deltaTime / guardRaiseTime);
        characterAnimator.SetFloat(GuardTime, guardAmount * guardPose);

        if (Time.time < reactionEndsAt) return;

        if (blocking && !guardShown)
        {
            characterAnimator.CrossFadeInFixedTime(GuardState, guardRaiseTime, combatLayer);
            guardShown = true;
        }
        else if (!blocking && guardShown)
        {
            characterAnimator.CrossFadeInFixedTime(EmptyState, guardRaiseTime, combatLayer);
            guardShown = false;
        }
    }

    private void OnHitResolved(PlayerDefence.HitResult result)
    {
        if (dead || combatLayer < 0) return;

        if (result == PlayerDefence.HitResult.Damaged)
            PlayReaction(HitState, hitAnimationTime);
        else if (result == PlayerDefence.HitResult.Parried)
            PlayReaction(ParryState, parryAnimationTime);
        else if (result == PlayerDefence.HitResult.GuardBroken)
            PlayReaction(GuardBreakState, defence.guardBreakDuration);
    }

    // A one-off reaction; when it ends, UpdateGuard puts the guard back if still blocking.
    private void PlayReaction(int state, float duration)
    {
        if (!characterAnimator.HasState(combatLayer, state)) return;
        characterAnimator.CrossFadeInFixedTime(state, 0.05f, combatLayer, 0f);
        reactionEndsAt = Time.time + duration;
        guardShown = false;
    }

    // Each attack jumps straight to its own state; anything else uses the default Attack.
    private void PlayAttack(HandAttack attack)
    {
        guardShown = false;
        reactionEndsAt = float.NegativeInfinity;

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
