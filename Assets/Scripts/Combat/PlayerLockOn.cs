using System.Collections.Generic;
using UnityEngine;

// Elden Ring style lock-on. Click the middle mouse button to lock onto the enemy nearest the
// middle of the screen; the player then keeps facing it, so moving sideways circles around it
// and the camera (which follows the player's facing) keeps it in view. Flick the mouse left or
// right to switch targets, click again to let go. C does the same as middle click. Anything with a Health component can be locked.
// Runs after PlayerTurn, so while locked it replaces mouse turning.
[DefaultExecutionOrder(10)]
public class PlayerLockOn : MonoBehaviour
{
    [Tooltip("Mouse button that locks on and lets go (2 = middle click).")]
    public int lockButton = 2;
    [Tooltip("Optional extra key to lock on and let go, for trackpads.")]
    public KeyCode lockKey = KeyCode.C;
    [Tooltip("Furthest an enemy can be to lock onto it, in metres.")]
    [Min(1f)] public float lockRange = 15f;
    [Tooltip("The lock breaks if the target gets further than this.")]
    [Min(1f)] public float breakRange = 20f;
    [Tooltip("Degrees per second the player turns to keep facing the target.")]
    [Min(1f)] public float turnSpeed = 720f;
    [Tooltip("How hard the mouse must flick sideways to switch target.")]
    [Min(0.1f)] public float switchFlick = 3f;
    [Tooltip("Seconds between target switches.")]
    [Min(0f)] public float switchCooldown = 0.35f;
    public bool showMarker = true;

    public Health Target { get; private set; }
    public bool IsLockedOn => Target != null;

    private PlayerMovement movement;
    private PlayerStats stats;
    private PlayerCombat combat;
    private Camera viewCamera;
    private float nextSwitchTime;
    private readonly HashSet<Health> dead = new HashSet<Health>();
    private readonly HashSet<Health> watched = new HashSet<Health>();

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (viewCamera == null) viewCamera = Camera.main;
        bool menuOpen = (combat != null && combat.inventoryOpen) || MenuState.AnyOpen;
        if ((stats != null && stats.IsDead()) || viewCamera == null)
        {
            Target = null;
            return;
        }

        if (!menuOpen && (Input.GetMouseButtonDown(lockButton) || (lockKey != KeyCode.None && Input.GetKeyDown(lockKey))))
            Target = IsLockedOn ? null : FindTarget(0);

        if (!IsLockedOn) return;

        if (!IsValid(Target) || Vector3.Distance(transform.position, Target.transform.position) > breakRange)
        {
            Target = null;
            return;
        }

        if (!menuOpen) TrySwitch();

        // Rolls and sprints go where the player points them; otherwise keep facing the target.
        bool freeMoving = movement != null && (movement.IsRolling || movement.IsSprinting);
        if (!freeMoving) FaceTarget();
    }

    private void TrySwitch()
    {
        float flick = Input.GetAxis("Mouse X");
        if (Time.time < nextSwitchTime || Mathf.Abs(flick) < switchFlick) return;

        Health next = FindTarget(flick > 0f ? 1 : -1);
        if (next != null) Target = next;
        nextSwitchTime = Time.time + switchCooldown;
    }

    private void FaceTarget()
    {
        Vector3 direction = Target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(direction, Vector3.up), turnSpeed * Time.deltaTime);
    }

    // side 0: the target nearest the middle of the screen.
    // side 1 or -1: the nearest target to the right or left of the current one on screen.
    private Health FindTarget(int side)
    {
        float currentX = side != 0 && Target != null ? viewCamera.WorldToViewportPoint(AimPoint(Target)).x : 0.5f;
        Health best = null;
        float bestScore = float.MaxValue;

        foreach (Health candidate in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            Watch(candidate);
            if (candidate == Target || !IsValid(candidate)) continue;
            if (candidate.transform.IsChildOf(transform) || transform.IsChildOf(candidate.transform)) continue;
            if (Vector3.Distance(transform.position, candidate.transform.position) > lockRange) continue;

            Vector3 view = viewCamera.WorldToViewportPoint(AimPoint(candidate));
            if (view.z <= 0f || view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f) continue;
            if (!CanSee(candidate)) continue;

            float offset = view.x - currentX;
            if (side != 0 && Mathf.Sign(offset) != side) continue;
            float score = side == 0 ? new Vector2(view.x - 0.5f, view.y - 0.5f).sqrMagnitude : Mathf.Abs(offset);
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }
        return best;
    }

    private bool CanSee(Health candidate)
    {
        Vector3 from = viewCamera.transform.position;
        Vector3 to = AimPoint(candidate);
        foreach (RaycastHit hit in Physics.RaycastAll(from, to - from, Vector3.Distance(from, to),
                     Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform.IsChildOf(candidate.transform)) continue;
            return false;
        }
        return true;
    }

    private bool IsValid(Health candidate) =>
        candidate != null && candidate.isActiveAndEnabled && !dead.Contains(candidate);

    // Health has no "is dead" check, so remember who has died.
    private void Watch(Health candidate)
    {
        if (!watched.Add(candidate)) return;
        candidate.onDeath.AddListener(() => dead.Add(candidate));
    }

    // The target's chest (like the Souls games), or the middle of its body if it has no skeleton.
    public static Vector3 AimPoint(Health target)
    {
        Animator animator = target.GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (chest == null) chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest != null) return chest.position;
        }
        Collider body = target.GetComponentInChildren<Collider>();
        return body != null ? body.bounds.center : target.transform.position + Vector3.up;
    }

    [Tooltip("Lock-on dot size as a share of the screen height.")]
    [Range(0.005f, 0.05f)] public float markerSize = 0.014f;
    private static Texture2D dotTexture;

    // A small soft white dot with a faint glow, centred on the target's chest.
    private void OnGUI()
    {
        if (!showMarker || !IsLockedOn || viewCamera == null) return;
        Vector3 point = viewCamera.WorldToScreenPoint(AimPoint(Target));
        if (point.z <= 0f) return;
        if (dotTexture == null) dotTexture = MakeDot(64);

        float size = Screen.height * markerSize;
        Vector2 centre = new Vector2(point.x, Screen.height - point.y);
        Color previous = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.35f);
        GUI.DrawTexture(new Rect(centre.x - size, centre.y - size, size * 2f, size * 2f), dotTexture);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(centre.x - size * 0.5f, centre.y - size * 0.5f, size, size), dotTexture);
        GUI.color = previous;
    }

    // A white circle that fades out at its edge.
    private static Texture2D MakeDot(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        float radius = resolution * 0.5f;
        for (int y = 0; y < resolution; y++)
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01((1f - distance) * 3f)));
            }
        texture.Apply();
        return texture;
    }
}
