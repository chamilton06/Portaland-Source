using UnityEngine;

// Moves PlayerCombat's WeaponPivot into the character's right hand, so the weapon follows
// the animations instead of floating beside the player. Add it to the Player beside PlayerCombat.
[RequireComponent(typeof(PlayerCombat))]
public class WeaponHandSocket : MonoBehaviour
{
    [Tooltip("The character model's Animator. It needs a Humanoid Avatar. Found automatically if left empty.")]
    public Animator characterAnimator;
    [Tooltip("Where the grip sits relative to the hand bone. Adjust in Play mode, then copy the values back.")]
    public Vector3 gripPosition = new Vector3(0f, 0.08f, 0.03f);
    [Tooltip("How the weapon is turned in the hand. Adjust in Play mode, then copy the values back.")]
    public Vector3 gripRotation = new Vector3(90f, 0f, 0f);

    private PlayerCombat combat;
    private Transform hand;

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

        // Combat stops posing the pivot by code once the hand carries it.
        combat.weaponFollowsHand = true;
        combat.weaponPivot.SetParent(hand, false);
        ApplyGrip();
    }

    // Applied every frame so the grip can be tuned live in the Inspector.
    private void LateUpdate()
    {
        ApplyGrip();
    }

    // Grip Position is in metres, so divide out any scale the hand bone carries.
    private void ApplyGrip()
    {
        Vector3 scale = hand.lossyScale;
        combat.weaponPivot.localPosition = new Vector3(
            gripPosition.x / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
            gripPosition.y / Mathf.Max(0.0001f, Mathf.Abs(scale.y)),
            gripPosition.z / Mathf.Max(0.0001f, Mathf.Abs(scale.z)));
        combat.weaponPivot.localRotation = Quaternion.Euler(gripRotation);
    }
}
