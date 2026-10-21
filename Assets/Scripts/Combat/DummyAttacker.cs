using UnityEngine;

// Stationary combat test opponent. Turns toward the player during preparation,
// then locks its direction for the strike so movement can make it miss.
[RequireComponent(typeof(Health))]
public class DummyAttacker : MonoBehaviour
{
    public PlayerStats player;
    public Weapon weapon;
    public Transform weaponPivot;
    [Min(0.05f)] public float swingDuration = 0.15f;
    public bool logHitResults = true;
    [Min(1f)] public float turnSpeed = 180f;
    [Min(0f)] public float damage = 10f;
    [Min(0.1f)] public float attackRange = 2.5f;
    [Range(1f, 180f)] public float attackAngle = 100f;
    [Min(0.1f)] public float windUpDuration = 0.6f;
    [Min(0.1f)] public float recoveryDuration = 1f;
    public bool showAttackLabel = true;

    private enum AttackState { Idle, WindUp, Swing, Return, Recovery }
    private AttackState state;
    private float stateEndsAt;
    private Health health;
    private bool dead;
    private Camera viewCamera;
    private Quaternion restRotation;
    private Quaternion raisedRotation;
    private Quaternion strikeRotation;
    private GameObject weaponInstance;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.onDeath.AddListener(StopAttacking);
    }

    private void Start()
    {
        viewCamera = Camera.main;
        if (weaponPivot == null)
        {
            GameObject pivot = new GameObject("DummyWeaponPivot");
            weaponPivot = pivot.transform;
            weaponPivot.SetParent(transform, false);
            weaponPivot.localPosition = new Vector3(0.65f, 0f, 0.4f);
            weaponPivot.localRotation = Quaternion.Euler(0f, 0f, -15f);
        }
        restRotation = weaponPivot.localRotation;
        raisedRotation = restRotation * Quaternion.Euler(-30f, 0f, 20f);
        strikeRotation = restRotation * Quaternion.Euler(60f, 0f, -40f);
        if (weapon != null && weapon.weaponPrefab != null)
        {
            weaponInstance = Instantiate(weapon.weaponPrefab, weaponPivot, false);
            weaponInstance.transform.localPosition = Vector3.zero;
            // The axe blade extends along model +X. Turn it toward the dummy's
            // +Z attack direction, independently of the pivot's swing rotation.
            weaponInstance.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            // This is a visual weapon; the timed strike handles damage.
            foreach (Collider collider in weaponInstance.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }
        else
            Debug.LogWarning("Assign a Weapon asset on DummyAttacker for its visible weapon.", this);
        if (player == null)
            Debug.LogWarning("Assign Player Stats on DummyAttacker.", this);
    }

    private void Update()
    {
        if (dead || player == null || !player.isActiveAndEnabled || player.IsDead())
        {
            state = AttackState.Idle;
            ResetPose();
            return;
        }

        // Only yaw toward the player. Stop tracking once the swing begins.
        if (state == AttackState.Idle || state == AttackState.WindUp)
            FacePlayer();

        if (state == AttackState.Idle)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > attackRange)
                return;

            state = AttackState.WindUp;
            stateEndsAt = Time.time + windUpDuration;
        }
        else
        {
            float duration = state == AttackState.WindUp ? windUpDuration : swingDuration;
            float progress = Mathf.Clamp01(1f - (stateEndsAt - Time.time) / Mathf.Max(0.05f, duration));
            if (state == AttackState.WindUp)
                weaponPivot.localRotation = Quaternion.Slerp(restRotation, raisedRotation, progress);
            else if (state == AttackState.Swing)
                weaponPivot.localRotation = Quaternion.Slerp(raisedRotation, strikeRotation, progress);
            else if (state == AttackState.Return)
                weaponPivot.localRotation = Quaternion.Slerp(strikeRotation, restRotation, progress);

            if (Time.time < stateEndsAt) return;
            if (state == AttackState.WindUp)
            {
                state = AttackState.Swing;
                stateEndsAt = Time.time + swingDuration;
            }
            else if (state == AttackState.Swing)
            {
                state = AttackState.Return;
                stateEndsAt = Time.time + swingDuration;
                TryStrike();
            }
            else if (state == AttackState.Return)
            {
                ResetPose();
                state = AttackState.Recovery;
                stateEndsAt = Time.time + recoveryDuration;
            }
            else
                state = AttackState.Idle;
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void TryStrike()
    {
        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude > attackRange) { Report("Miss: out of range"); return; }
        direction.y = 0f;
        if (Vector3.Angle(transform.forward, direction) > attackAngle * 0.5f) { Report("Miss: outside attack cone"); return; }

        // Walls or another dummy can obstruct the strike.
        Vector3 origin = transform.position + Vector3.up * 0.25f;
        Vector3 destination = player.transform.position + Vector3.up * 0.25f;
        Vector3 ray = destination - origin;
        RaycastHit[] hits = Physics.RaycastAll(origin, ray.normalized, ray.magnitude,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            // Explicitly ignore every collider belonging to this attacker.
            if (hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.collider.GetComponentInParent<PlayerStats>() == player) break;
            Report("Miss: obstructed by " + hit.collider.name);
            return;
        }

        float before = player.GetHealthPercent() * player.maxHealth;
        player.TakeDamage(damage);
        float after = player.GetHealthPercent() * player.maxHealth;
        Report("Hit: HP " + before.ToString("F1") + " -> " + after.ToString("F1"));
    }

    private void Report(string result)
    {
        if (logHitResults) Debug.Log(name + ": " + result, this);
    }

    private void ResetPose()
    {
        if (weaponPivot != null) weaponPivot.localRotation = restRotation;
    }

    private void StopAttacking()
    {
        dead = true;
        ResetPose();
        state = AttackState.Idle;
    }

    private void OnDisable()
    {
        ResetPose();
        state = AttackState.Idle;
    }

    private void OnDestroy()
    {
        if (health != null) health.onDeath.RemoveListener(StopAttacking);
    }

    // Prototype telegraph without changing materials used by DamageFlash.
    private void OnGUI()
    {
        if (!showAttackLabel || dead || viewCamera == null || player == null || player.IsDead()) return;
        Vector3 point = viewCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
        if (point.z <= 0f) return;
        string label = state == AttackState.WindUp ? "Preparing attack..."
            : state == AttackState.Swing || state == AttackState.Return ? "Striking!" : "";
        if (!string.IsNullOrEmpty(label))
            GUI.Box(new Rect(point.x - 120f, Screen.height - point.y, 240f, 30f), label);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawRay(transform.position,
            Quaternion.Euler(0f, -attackAngle * 0.5f, 0f) * transform.forward * attackRange);
        Gizmos.DrawRay(transform.position,
            Quaternion.Euler(0f, attackAngle * 0.5f, 0f) * transform.forward * attackRange);
    }
}
