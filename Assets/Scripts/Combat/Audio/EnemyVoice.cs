using UnityEngine;

// An enemy's voice: growls when it wakes, grunts on some attacks, cries out when staggered,
// screams when it dies and roars at its second phase. Goes on an enemy with EnemyMelee.
// One clip is picked at random from each list; leave a list empty to stay quiet for it.
[RequireComponent(typeof(EnemyMelee))]
public class EnemyVoice : MonoBehaviour
{
    public AudioClip[] wake;
    public AudioClip[] attack;
    [Tooltip("Share of attacks that get a grunt, so it isn't every single one.")]
    [Range(0f, 1f)] public float attackChance = 0.5f;
    public AudioClip[] pain;
    public AudioClip[] death;
    public AudioClip[] secondPhase;

    [Range(0f, 1f)] public float volume = 0.9f;
    [Tooltip("Lower than 1 makes the voice deeper.")]
    [Range(0.5f, 1.5f)] public float pitch = 0.9f;
    [Range(0f, 0.3f)] public float pitchVariation = 0.06f;

    private EnemyMelee enemy;
    private AudioSource source;

    private void Awake()
    {
        enemy = GetComponent<EnemyMelee>();
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0.8f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 4f;
        source.maxDistance = 40f;

        // Listen for combat events instead of checking the enemy state every frame.
        // EnemyMelee decides what happened; this component chooses a voice clip.
        enemy.Woke += OnWoke;
        enemy.AttackStarted += OnAttack;
        enemy.WasStaggered += OnPain;
        enemy.Died += OnDeath;
        enemy.EnteredSecondPhase += OnSecondPhase;
    }

    private void OnDestroy()
    {
        if (enemy == null) return;
        enemy.Woke -= OnWoke;
        enemy.AttackStarted -= OnAttack;
        enemy.WasStaggered -= OnPain;
        enemy.Died -= OnDeath;
        enemy.EnteredSecondPhase -= OnSecondPhase;
    }

    private void OnWoke() => Say(wake);
    private void OnAttack() { if (Random.value < attackChance) Say(attack); }
    private void OnPain() => Say(pain);
    private void OnDeath() => Say(death, interrupt: true);
    private void OnSecondPhase() => Say(secondPhase, interrupt: true);

    private void Say(AudioClip[] clips, bool interrupt = false)
    {
        if (clips == null || clips.Length == 0) return;
        if (interrupt) source.Stop();
        else if (source.isPlaying) return; // Don't talk over itself.
        source.pitch = pitch + Random.Range(-pitchVariation, pitchVariation);
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.Play();
    }
}
