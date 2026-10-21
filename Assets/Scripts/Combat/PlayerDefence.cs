using UnityEngine;

// Only incoming combat hits pass through this component. Fall damage and
// starvation continue to use PlayerStats directly.
[RequireComponent(typeof(PlayerStats), typeof(PlayerCombat))]
public class PlayerDefence : MonoBehaviour
{
    public enum HitResult { Ignored, Damaged, Blocked, Parried }

    [Min(0f)] public float parryWindow = 0.2f;
    [Range(1f, 180f)] public float blockAngle = 120f;
    [Min(0.1f)] public float parryMessageDuration = 0.75f;

    private PlayerStats stats;
    private PlayerCombat combat;
    private bool guarding;
    private bool focused = true;
    private float guardStartedAt = float.NegativeInfinity;
    private float messageUntil = float.NegativeInfinity;
    private bool parryUsed;
    private int inputFrame = -1;

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
        combat = GetComponent<PlayerCombat>();
    }

    private bool CanGuard()
    {
        return isActiveAndEnabled && focused && stats != null && !stats.IsDead()
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
        if (stats == null || stats.IsDead() || damage <= 0f) return HitResult.Ignored;

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

            // Unlimited prototype guard: no stamina drain or guard break.
            return HitResult.Blocked;
        }

        stats.TakeDamage(damage);
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
