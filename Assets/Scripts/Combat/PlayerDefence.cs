using UnityEngine;

// Only incoming combat hits pass through this component. Fall damage and
// starvation continue to use PlayerStats directly.
[RequireComponent(typeof(PlayerStats), typeof(PlayerCombat))]
public class PlayerDefence : MonoBehaviour
{
    public enum HitResult { Ignored, Damaged, Blocked, Parried, Dodged, GuardBroken }

    [Min(0f)] public float parryWindow = 0.2f;
    [Range(1f, 180f)] public float blockAngle = 120f;
    [Min(0.1f)] public float parryMessageDuration = 0.75f;
    [Tooltip("Stamina each blocked hit costs. Without enough, the guard breaks and the hit lands. 0 = unlimited guard.")]
    [Min(0f)] public float blockStaminaCost = 0f;
    [Tooltip("Seconds the player staggers after a guard break, unable to guard or attack.")]
    [Min(0f)] public float guardBreakDuration = 1f;
    [Tooltip("Seconds a hit that lands interrupts the player: cancels their attack and stops attacking or guarding. 0 = no hit stun.")]
    [Min(0f)] public float hitStunDuration = 0f;

    // Lets animation react to blocks, parries and guard breaks.
    public event System.Action<HitResult> HitResolved;

    private PlayerMovement movement;
    private PlayerStats stats;
    private PlayerCombat combat;
    private bool guarding;
    private bool focused = true;
    private float guardStartedAt = float.NegativeInfinity;
    private float messageUntil = float.NegativeInfinity;
    private bool parryUsed;
    private int inputFrame = -1;
    private float staggeredUntil = float.NegativeInfinity;

    public bool IsStaggered => Time.time < staggeredUntil;

    public bool IsBlocking
    {
        get
        {
            RefreshInput();
            return guarding && CanGuard();
        }
    }

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    private bool CanGuard()
    {
        return (movement == null || !movement.IsRolling) && !IsStaggered && isActiveAndEnabled && focused && stats != null && !stats.IsDead()
            && combat != null && combat.isActiveAndEnabled && !combat.inventoryOpen
            && combat.weaponEquipped && combat.equippedWeapon != null && !combat.IsAttacking;
    }

    private void Update()
    {
        RefreshInput();
        if (!CanGuard()) CancelGuard();
    }

    // Hit resolution and PlayerCombat can read input before this Update runs.
    // Poll once per frame so script execution order cannot change parry timing.
    private void RefreshInput()
    {
        if (inputFrame == Time.frameCount) return;
        inputFrame = Time.frameCount;

        if (!CanGuard() || !Input.GetMouseButton(1))
        {
            CancelGuard();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            guarding = true;
            guardStartedAt = Time.time;
            parryUsed = false;
        }
    }

    public void CancelGuard()
    {
        guarding = false;
        guardStartedAt = float.NegativeInfinity;
    }

    public HitResult ReceiveHit(float damage, Vector3 attackerPosition)
    {
        HitResult result = ResolveHit(damage, attackerPosition);
        if (result != HitResult.Ignored) HitResolved?.Invoke(result);
        return result;
    }

    private HitResult ResolveHit(float damage, Vector3 attackerPosition)
    {
        if (stats == null || stats.IsDead() || damage <= 0f) return HitResult.Ignored;

        if (movement != null && movement.IsInDodgeWindow) return HitResult.Dodged;

        Vector3 towardAttacker = attackerPosition - transform.position;
        towardAttacker.y = 0f;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        bool frontal = towardAttacker.sqrMagnitude > 0.0001f
            && Vector3.Angle(forward, towardAttacker) <= blockAngle * 0.5f;

        if (IsBlocking && frontal)
        {
            if (!parryUsed && Time.time - guardStartedAt <= parryWindow)
            {
                parryUsed = true;
                messageUntil = Time.time + parryMessageDuration;
                return HitResult.Parried;
            }

            // Each block costs stamina. Run out and the guard breaks: the hit lands and the player staggers.
            if (blockStaminaCost <= 0f || stats.TryUseStamina(blockStaminaCost))
                return HitResult.Blocked;

            CancelGuard();
            staggeredUntil = Time.time + guardBreakDuration;
            stats.TakeDamage(damage);
            return HitResult.GuardBroken;
        }

        stats.TakeDamage(damage);
        if (hitStunDuration > 0f)
        {
            if (combat != null) combat.CancelAttack();
            staggeredUntil = Mathf.Max(staggeredUntil, Time.time + hitStunDuration);
        }
        return HitResult.Damaged;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        focused = hasFocus;
        if (!hasFocus) CancelGuard();
    }

    private void OnDisable()
    {
        CancelGuard();
        messageUntil = float.NegativeInfinity;
    }

    private void OnGUI()
    {
        if (stats == null || stats.IsDead() || Time.time >= messageUntil) return;
        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Box(new Rect((Screen.width - 200f) * 0.5f, Screen.height * 0.35f, 200f, 50f),
            "Parried", style);
    }
}
