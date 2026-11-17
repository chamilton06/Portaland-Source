using UnityEngine;

public enum CombatSound { Swing, Hit, HeavyHit, Block, Parry, Roll, Victory, Death }

// Plays combat sounds. Goes on the Player, with a CombatSoundBank. It hears the player's own
// blocks, parries, hits taken and rolls by itself; the player's attacks and enemies call
// CombatAudio.Play. If a scene has no CombatAudio, nothing plays, so scenes stay silent until
// it's added (Prototype doesn't have it).
[RequireComponent(typeof(PlayerDefence))]
public class CombatAudio : MonoBehaviour
{
    public CombatSoundBank bank;
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Tooltip("Random pitch change per sound, so repeats don't sound identical.")]
    [Range(0f, 0.3f)] public float pitchVariation = 0.08f;
    [Tooltip("How many sounds can overlap.")]
    [Range(2, 16)] public int voices = 8;

    private static CombatAudio instance;
    private AudioSource[] pool;
    private AudioSource flat;   // Not positional, for sounds like the victory sting.
    private int next;
    private PlayerDefence defence;
    private PlayerMovement movement;
    private bool wasRolling;

    private void Awake()
    {
        instance = this;
        defence = GetComponent<PlayerDefence>();
        movement = GetComponent<PlayerMovement>();
        defence.HitResolved += OnHitResolved;

        // A few sources that move to wherever the sound happens.
        pool = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            AudioSource source = new GameObject("CombatSound").AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.playOnAwake = false;
            source.spatialBlend = 0.6f;      // Mostly positional, but never too quiet to hear.
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 3f;
            source.maxDistance = 35f;
            pool[i] = source;
        }
        flat = gameObject.AddComponent<AudioSource>();
        flat.playOnAwake = false;
        flat.spatialBlend = 0f;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        if (defence != null) defence.HitResolved -= OnHitResolved;
    }

    private void Update()
    {
        bool rolling = movement != null && movement.IsRolling;
        if (rolling && !wasRolling) Play(CombatSound.Roll, transform.position);
        wasRolling = rolling;
    }

    // Sounds for what happened when the player was hit.
    private void OnHitResolved(PlayerDefence.HitResult result)
    {
        Vector3 at = transform.position + Vector3.up;
        switch (result)
        {
            case PlayerDefence.HitResult.Blocked: Play(CombatSound.Block, at); break;
            case PlayerDefence.HitResult.Parried: Play(CombatSound.Parry, at); break;
            case PlayerDefence.HitResult.GuardBroken: Play(CombatSound.Block, at); Play(CombatSound.HeavyHit, at); break;
            case PlayerDefence.HitResult.Damaged:
            case PlayerDefence.HitResult.Poised: Play(CombatSound.Hit, at); break;
        }
    }

    // Plays a combat sound at a point in the world. Does nothing if the scene has no CombatAudio.
    public static void Play(CombatSound sound, Vector3 position)
    {
        if (instance == null || instance.bank == null) return;
        // Screen-wide moments, not placed in the world.
        if (sound == CombatSound.Victory || sound == CombatSound.Death)
        {
            bool victory = sound == CombatSound.Victory;
            AudioClip clip = victory ? instance.bank.victory : instance.bank.death;
            float level = victory ? instance.bank.victoryVolume : instance.bank.deathVolume;
            if (clip != null) instance.flat.PlayOneShot(clip, level * instance.masterVolume);
            return;
        }
        instance.PlayClip(instance.Pick(sound, out float volume), position, volume);
    }

    private AudioClip Pick(CombatSound sound, out float volume)
    {
        AudioClip[] clips;
        switch (sound)
        {
            case CombatSound.Swing: clips = bank.swings; volume = bank.swingVolume; break;
            case CombatSound.Hit: clips = bank.hits; volume = bank.hitVolume; break;
            case CombatSound.HeavyHit: clips = bank.heavyHits; volume = bank.heavyHitVolume; break;
            case CombatSound.Block: clips = bank.blocks; volume = bank.blockVolume; break;
            case CombatSound.Parry: clips = bank.parries; volume = bank.parryVolume; break;
            default: clips = bank.rolls; volume = bank.rollVolume; break;
        }
        return clips != null && clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;
    }

    private void PlayClip(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        AudioSource source = pool[next];
        next = (next + 1) % pool.Length;
        source.transform.position = position;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume * masterVolume);
    }
}
