using UnityEngine;

// A model to hold in the hand instead of a weapon's normal Weapon Prefab.
// The model is measured when equipped, so supplier models work whatever their import scale.
[System.Serializable]
public class HeldWeaponModel
{
    [Tooltip("The inventory weapon this model replaces when held.")]
    public Weapon weapon;
    [Tooltip("The model to hold, for example a Quaternius weapon FBX. The blade or head should point along its Y axis.")]
    public GameObject model;
    [Tooltip("Total length of the held model in metres, from the bottom of the handle to the tip.")]
    [Min(0.05f)] public float length = 1f;
    [Tooltip("Where the hand holds it: 0 is the bottom of the handle, 1 is the tip.")]
    [Range(0f, 1f)] public float gripPoint = 0.08f;
    [Tooltip("Extra rotation for this model only, if one weapon sits differently from the others.")]
    public Vector3 rotationOffset;

    // Scales the new instance to Length and moves it so the grip point sits on the pivot.
    public void FitToGrip(Transform instance, Transform pivot)
    {
        // Keep the model's own import rotation (Blender FBX files stand upright with it).
        Quaternion rotation = Quaternion.Euler(rotationOffset) * model.transform.localRotation;
        instance.localPosition = Vector3.zero;
        instance.localRotation = rotation;

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // Measure the model in the pivot's space, where its Y axis runs along the weapon.
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.bounds;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 world = bounds.center + Vector3.Scale(bounds.extents, new Vector3(
                    (corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                Vector3 local = pivot.InverseTransformPoint(world);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }
        }

        // Bones can carry their own scale, so compare against the real world-space length.
        float height = max.y - min.y;
        float worldHeight = height * Mathf.Abs(pivot.lossyScale.y);
        if (worldHeight <= 0.0001f) return;

        float scale = length / worldHeight;
        Vector3 grip = new Vector3((min.x + max.x) * 0.5f, min.y + height * gripPoint, (min.z + max.z) * 0.5f);

        // Scaling happens around the pivot, so the grip point moves with it; shift it back onto the pivot.
        instance.localScale *= scale;
        instance.localPosition = -grip * scale;
    }
}
