using UnityEngine;

// Where a weapon handle sits in a closed right fist, worked out from the knuckles, so a weapon
// sits across the fist with the blade out of the thumb side. Used by WeaponHandSocket and EnemyMelee.
// Everything is in the hand bone's local space; the knuckles don't move relative to it.
public struct FistGrip
{
    public Vector3 centre;
    // Pivot rotation: Y runs along the blade (see HeldWeaponModel), X faces the knuckles.
    public Quaternion rotation;
    public Vector3 knuckleAxis, bladeAxis, palmAxis;

    // False if the character has no index, middle and little finger bones on the right hand.
    public static bool TryMeasure(Animator animator, Transform hand, out FistGrip grip)
    {
        grip = default;
        Transform index = FindFinger(animator, hand, HumanBodyBones.RightIndexProximal, "index_01");
        Transform middle = FindFinger(animator, hand, HumanBodyBones.RightMiddleProximal, "middle_01");
        Transform little = FindFinger(animator, hand, HumanBodyBones.RightLittleProximal, "pinky_01");
        Transform thumb = FindFinger(animator, hand, HumanBodyBones.RightThumbProximal, "thumb_01");
        if (index == null || middle == null || little == null) return false;

        Vector3 middleKnuckle = hand.InverseTransformPoint(middle.position);
        Vector3 knuckles = middleKnuckle.normalized;
        Vector3 across = hand.InverseTransformPoint(index.position) - hand.InverseTransformPoint(little.position);
        Vector3 blade = Vector3.ProjectOnPlane(across, knuckles).normalized;
        if (blade.sqrMagnitude < 0.5f) return false;

        // The palm is on the thumb's side of the hand.
        Vector3 palm = Vector3.Cross(knuckles, blade);
        if (thumb != null && Vector3.Dot(hand.InverseTransformPoint(thumb.position), palm) < 0f) palm = -palm;

        grip.knuckleAxis = knuckles;
        grip.bladeAxis = blade;
        grip.palmAxis = palm;
        // The handle sits just short of the knuckles, a little into the palm.
        grip.centre = middleKnuckle * 0.8f + palm * (middleKnuckle.magnitude * 0.25f);
        grip.rotation = Quaternion.LookRotation(Vector3.Cross(knuckles, blade), blade);
        return true;
    }

    private static Transform FindFinger(Animator animator, Transform hand, HumanBodyBones bone, string nameStart)
    {
        Transform finger = animator != null && animator.isHuman ? animator.GetBoneTransform(bone) : null;
        if (finger != null) return finger;
        // Some avatars don't map fingers; look for the bone by name under the hand.
        foreach (Transform child in hand.GetComponentsInChildren<Transform>())
            if (child.name.StartsWith(nameStart) && child.name.EndsWith("_r")) return child;
        return null;
    }
}
