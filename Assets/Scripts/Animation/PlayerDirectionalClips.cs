using UnityEngine;

// Keep clip choices in an asset so changing scenes doesn't mean editing code.
[CreateAssetMenu(menuName = "Portaland/Player Directional Clips")]
public class PlayerDirectionalClips : ScriptableObject
{
    public AnimationClip walkLeft;
    public AnimationClip walkRight;
    public AnimationClip walkBack;

    [Header("Optional crouch directions (use all three together)")]
    public AnimationClip crouchLeft;
    public AnimationClip crouchRight;
    public AnimationClip crouchBack;
}
