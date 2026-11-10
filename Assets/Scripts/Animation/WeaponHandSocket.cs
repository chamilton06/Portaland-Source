using UnityEngine;

// Moves PlayerCombat's WeaponPivot into the character's right hand, so the weapon follows
// the animations instead of floating beside the player. Add it to the Player beside PlayerCombat.
[RequireComponent(typeof(PlayerCombat))]
public class WeaponHandSocket : MonoBehaviour
{
    [Tooltip("The character model's Animator. It needs a Humanoid Avatar. Found automatically if left empty.")]
    public Animator characterAnimator;

    [Header("Automatic grip (needs finger bones)")]
    [Tooltip("Works out the grip from the knuckles, so the handle sits across the closed fist and the blade comes out " +
             "on the thumb side. Turn off to place the weapon with Grip Position and Grip Rotation below instead.")]
    public bool autoGrip = true;
    [Tooltip("Moves the grip in metres, along the grip's own axes: X = toward the knuckles, Y = along the blade, Z = into the palm.")]
    public Vector3 autoGripNudge = Vector3.zero;
    [Tooltip("Turns the weapon in the fist, in degrees: X tips the blade forward or back, Y twists it, Z tips it sideways.")]
    public Vector3 autoGripTilt = Vector3.zero;

    [Header("Manual grip (when Auto Grip is off or there are no finger bones)")]
    [Tooltip("Where the grip sits relative to the hand bone. Adjust in Play mode, then copy the values back.")]
    public Vector3 gripPosition = new Vector3(0f, 0.08f, 0.03f);
    [Tooltip("How the weapon is turned in the hand. Adjust in Play mode, then copy the values back.")]
    public Vector3 gripRotation = new Vector3(90f, 0f, 0f);

    [Tooltip("Draws the grip in the Scene view (and the Game view with Gizmos on) to help tune it: " +
             "yellow ball = where the hand holds, green = blade direction, red = blade edge, blue = flat of the blade.")]
    public bool showGripMarker = false;

    private PlayerCombat combat;
    private Transform hand;
    private bool hasFistGrip;
    private Vector3 fistCentre;        // In the hand bone's local space.
    private Quaternion fistRotation;   // Pivot rotation in the hand bone's local space.
    private Vector3 knuckleAxis, bladeAxis, palmAxis;

    private void Awake()
    {
        combat = GetComponent<PlayerCombat>();
        if (characterAnimator == null) characterAnimator = GetComponentInChildren<Animator>();

        if (characterAnimator != null && characterAnimator.isHuman)
            hand = characterAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (hand == null || combat.weaponPivot == null)
        {
            Debug.LogWarning("Weapon Hand Socket needs a Humanoid character Animator and PlayerCombat's Weapon Pivot. The weapon stays on the old pivot.", this);
            enabled = false;
            return;
        }

        hasFistGrip = MeasureFist();
        if (autoGrip && !hasFistGrip)
            Debug.LogWarning("Auto Grip needs the right hand's index, middle and little finger bones. Using Grip Position and Grip Rotation instead.", this);

        // Combat stops posing the pivot by code once the hand carries it.
        combat.weaponFollowsHand = true;
        combat.weaponPivot.SetParent(hand, false);
        ApplyGrip();
    }

    // Finds the fist from where the knuckles sit on the hand. The knuckles don't move relative to
    // the hand bone, whatever the fingers do, so this is measured once.
    private bool MeasureFist()
    {
        Transform index = FindFinger(HumanBodyBones.RightIndexProximal, "index_01");
        Transform middle = FindFinger(HumanBodyBones.RightMiddleProximal, "middle_01");
        Transform little = FindFinger(HumanBodyBones.RightLittleProximal, "pinky_01");
        Transform thumb = FindFinger(HumanBodyBones.RightThumbProximal, "thumb_01");
        if (index == null || middle == null || little == null) return false;

        Vector3 middleKnuckle = hand.InverseTransformPoint(middle.position);
        Vector3 knuckles = middleKnuckle.normalized;
        Vector3 across = hand.InverseTransformPoint(index.position) - hand.InverseTransformPoint(little.position);
        Vector3 blade = Vector3.ProjectOnPlane(across, knuckles).normalized;
        if (blade.sqrMagnitude < 0.5f) return false;

        // The palm is on the thumb's side of the hand.
        Vector3 palm = Vector3.Cross(knuckles, blade);
        if (thumb != null && Vector3.Dot(hand.InverseTransformPoint(thumb.position), palm) < 0f) palm = -palm;

        knuckleAxis = knuckles;
        bladeAxis = blade;
        palmAxis = palm;
        // The handle sits just short of the knuckles, a little into the palm.
        fistCentre = middleKnuckle * 0.8f + palm * (middleKnuckle.magnitude * 0.25f);
        // Pivot Y runs along the blade (see HeldWeaponModel); pivot X faces the knuckles, so the edge leads the swing.
        fistRotation = Quaternion.LookRotation(Vector3.Cross(knuckles, blade), blade);
        return true;
    }

    private Transform FindFinger(HumanBodyBones bone, string nameStart)
    {
        Transform finger = characterAnimator.GetBoneTransform(bone);
        if (finger != null) return finger;
        // Some avatars don't map fingers; look for the bone by name under the hand.
        foreach (Transform child in hand.GetComponentsInChildren<Transform>())
            if (child.name.StartsWith(nameStart) && child.name.EndsWith("_r")) return child;
        return null;
    }

    // Applied every frame so the grip can be tuned live in the Inspector.
    private void LateUpdate()
    {
        ApplyGrip();
    }

    private void ApplyGrip()
    {
        Vector3 scale = hand.lossyScale;
        Vector3 metresToLocal = new Vector3(
            1f / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
            1f / Mathf.Max(0.0001f, Mathf.Abs(scale.y)),
            1f / Mathf.Max(0.0001f, Mathf.Abs(scale.z)));

        if (autoGrip && hasFistGrip)
        {
            Vector3 nudge = knuckleAxis * autoGripNudge.x + bladeAxis * autoGripNudge.y + palmAxis * autoGripNudge.z;
            combat.weaponPivot.localPosition = fistCentre + Vector3.Scale(nudge, metresToLocal);
            combat.weaponPivot.localRotation = fistRotation * Quaternion.Euler(autoGripTilt);
            return;
        }

        // Grip Position is in metres, so divide out any scale the hand bone carries.
        combat.weaponPivot.localPosition = Vector3.Scale(gripPosition, metresToLocal);
        combat.weaponPivot.localRotation = Quaternion.Euler(gripRotation);
    }

    private void OnDrawGizmos()
    {
        if (!showGripMarker || combat == null || combat.weaponPivot == null) return;
        Transform pivot = combat.weaponPivot;
        Vector3 grip = pivot.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(grip, 0.025f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(grip, grip + pivot.up * 0.6f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(grip, grip + pivot.right * 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(grip, grip + pivot.forward * 0.2f);

        // The hand bone itself, to compare against the grip.
        if (hand != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hand.position, 0.035f);
        }
    }
}
