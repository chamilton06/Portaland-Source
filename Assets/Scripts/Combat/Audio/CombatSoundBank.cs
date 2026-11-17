using UnityEngine;

// The clip lists for combat sounds, kept in one asset so they're easy to swap.
// One clip is picked at random each time, so give each list a few variations.
[CreateAssetMenu(fileName = "CombatSounds", menuName = "Portaland/Combat Sound Bank")]
public class CombatSoundBank : ScriptableObject
{
    [Tooltip("Swooshes when a weapon or claw swings.")]
    public AudioClip[] swings;
    [Tooltip("A normal hit landing on a body.")]
    public AudioClip[] hits;
    [Tooltip("Ripostes, backstabs and guard breaks.")]
    public AudioClip[] heavyHits;
    [Tooltip("A hit stopped by the player's guard.")]
    public AudioClip[] blocks;
    [Tooltip("A successful parry.")]
    public AudioClip[] parries;
    [Tooltip("The player's roll.")]
    public AudioClip[] rolls;
    [Tooltip("Played when a boss is defeated, with the banner. The same sound for every boss in the game.")]
    public AudioClip victory;
    [Tooltip("Played with the YOU DIED screen.")]
    public AudioClip death;

    [Header("Volumes")]
    [Range(0f, 1f)] public float swingVolume = 0.5f;
    [Range(0f, 1f)] public float hitVolume = 0.9f;
    [Range(0f, 1f)] public float heavyHitVolume = 1f;
    [Range(0f, 1f)] public float blockVolume = 0.8f;
    [Range(0f, 1f)] public float parryVolume = 1f;
    [Range(0f, 1f)] public float rollVolume = 0.6f;
    [Range(0f, 1f)] public float victoryVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;
}
